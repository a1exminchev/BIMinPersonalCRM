using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BIMinPersonalCRM.Commands;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.Services;

namespace BIMinPersonalCRM.ViewModels
{
    /// <summary>
    ///     Root view model for the application. Exposes collections of clients and
    ///     tasks along with commands to manipulate them and computed statistics.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;

        private TaskItem? _selectedTask;
        private Client? _selectedClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            // Use a default JSON file in the user's local application data folder.
            var dataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIMinPersonalCRM",
                "data.json");
            _dataService = new JsonDataService(dataPath);

            Tasks = new ObservableCollection<TaskItem>();
            Clients = new ObservableCollection<Client>();

            // Commands
            AddTaskCommand = new RelayCommand(_ => AddTask());
            AddClientCommand = new RelayCommand(_ => AddClient());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            LoadCommand = new RelayCommand(async _ => await LoadAsync());

            // Load data on startup
            _ = LoadAsync();
        }

        /// <summary>
        ///     Gets or sets the collection of tasks displayed in the UI.
        /// </summary>
        public ObservableCollection<TaskItem> Tasks { get; }

        /// <summary>
        ///     Gets or sets the collection of clients displayed in the UI.
        /// </summary>
        public ObservableCollection<Client> Clients { get; }

        /// <summary>
        ///     Gets or sets the currently selected task in the UI.
        /// </summary>
        public TaskItem? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask != value)
                {
                    _selectedTask = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the currently selected client in the UI.
        /// </summary>
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Commands

        /// <summary>
        ///     Command to add a new task to the collection.
        /// </summary>
        public ICommand AddTaskCommand { get; }

        /// <summary>
        ///     Command to add a new client to the collection.
        /// </summary>
        public ICommand AddClientCommand { get; }

        /// <summary>
        ///     Command to persist the current state to disk.
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        ///     Command to load the persisted state from disk.
        /// </summary>
        public ICommand LoadCommand { get; }

        #endregion

        #region Computed properties

        /// <summary>
        ///     Gets the total number of hours spent across all tasks.
        /// </summary>
        public double TotalHoursSpent => Tasks.Sum(t => t.HoursSpent);

        /// <summary>
        ///     Gets the total money earned across all tasks.
        /// </summary>
        public decimal TotalMoneyEarned => Tasks.Sum(t => t.MoneyEarned);

        /// <summary>
        ///     Gets the average hourly rate across all tasks. If no hours have
        ///     been logged returns zero to avoid division by zero.
        /// </summary>
        public decimal AverageHourlyRate
        {
            get
            {
                var totalHours = TotalHoursSpent;
                return totalHours > 0 ? TotalMoneyEarned / (decimal)totalHours : 0m;
            }
        }

        #endregion

        #region Private methods

        private void AddTask()
        {
            // Assign the new task to the currently selected client if available.
            var task = new TaskItem
            {
                Title = "New Task",
                StartDate = DateTime.Today,
                ClientId = SelectedClient?.Id,
                ClientName = SelectedClient?.Name
            };
            Tasks.Add(task);
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(TotalMoneyEarned));
            OnPropertyChanged(nameof(AverageHourlyRate));
        }

        private void AddClient()
        {
            var client = new Client { Name = "New Client" };
            Clients.Add(client);
        }

        private async Task SaveAsync()
        {
            var data = new DataStore
            {
                Clients = Clients.ToList(),
                Tasks = Tasks.ToList()
            };
            await _dataService.SaveAsync(data);
        }

        private async Task LoadAsync()
        {
            var data = await _dataService.LoadAsync();
            Clients.Clear();
            foreach (var client in data.Clients)
            {
                Clients.Add(client);
            }
            Tasks.Clear();
            foreach (var task in data.Tasks)
            {
                // Populate the ClientName property for display purposes.
                if (task.ClientId.HasValue)
                {
                    var client = data.Clients.FirstOrDefault(c => c.Id == task.ClientId.Value);
                    task.ClientName = client?.Name;
                }
                Tasks.Add(task);
            }
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(TotalMoneyEarned));
            OnPropertyChanged(nameof(AverageHourlyRate));
        }

        #endregion
    }
}
