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
    ///     Главная модель представления приложения. Управляет коллекциями компаний,
    ///     заказов и задач, а также реализацией команд и таймера.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly System.Timers.Timer _timer;

        private Company? _selectedCompany;
        private Order? _selectedOrder;
        private TaskItem? _selectedTask;
        private DateTime? _timerStartTime;

        /// <summary>
        ///     Инициализирует новую модель представления и загружает данные.
        /// </summary>
        public MainViewModel()
        {
            // Файл данных в локальном каталоге пользователя.
            var dataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIMinPersonalCRM",
                "data.json");
            _dataService = new JsonDataService(dataPath);

            Companies = new ObservableCollection<Company>();

            // Инициализация команд
            AddCompanyCommand = new RelayCommand(_ => AddCompany());
            AddOrderCommand = new RelayCommand(_ => AddOrder(), _ => SelectedCompany != null);
            AddTaskCommand = new RelayCommand(_ => AddTask(), _ => SelectedOrder != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            StartTimerCommand = new RelayCommand(_ => StartTimer(), _ => SelectedTask != null && !IsTimerRunning);
            StopTimerCommand = new RelayCommand(_ => StopTimer(), _ => SelectedTask != null && IsTimerRunning);

            // Таймер с секундной периодичностью, обновляет часы раз в секунду.
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (_, _) => OnTimerTick();

            // Загрузка данных при запуске
            _ = LoadAsync();
        }

        #region Коллекции и выбранные элементы

        /// <summary>
        ///     Компании, отображаемые в UI.
        /// </summary>
        public ObservableCollection<Company> Companies { get; }

        /// <summary>
        ///     Текущая выбранная компания.
        /// </summary>
        public Company? SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (_selectedCompany != value)
                {
                    _selectedCompany = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Orders));
                    // Сбросить выбранный заказ и задачу при смене компании
                    SelectedOrder = null;
                    SelectedTask = null;
                    // Обновить доступность команд
                    ((RelayCommand)AddOrderCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Заказы выбранной компании. Возвращает пустой список, если компания не выбрана.
        /// </summary>
        public ObservableCollection<Order> Orders
        {
            get
            {
                if (SelectedCompany != null)
                {
                    return new ObservableCollection<Order>(SelectedCompany.Orders);
                }
                return new ObservableCollection<Order>();
            }
        }

        /// <summary>
        ///     Текущий выбранный заказ.
        /// </summary>
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder != value)
                {
                    _selectedOrder = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Tasks));
                    // При смене заказа сбросить выбранную задачу
                    SelectedTask = null;
                    ((RelayCommand)AddTaskCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Задачи выбранного заказа. Пустой список, если заказ не выбран.
        /// </summary>
        public ObservableCollection<TaskItem> Tasks
        {
            get
            {
                if (SelectedOrder != null)
                {
                    return new ObservableCollection<TaskItem>(SelectedOrder.Tasks);
                }
                return new ObservableCollection<TaskItem>();
            }
        }

        /// <summary>
        ///     Текущая выбранная задача.
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
                    ((RelayCommand)StartTimerCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopTimerCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Возвращает признак, что таймер запущен.
        /// </summary>
        public bool IsTimerRunning => _timer.Enabled;

        #endregion

        #region Команды

        public ICommand AddCompanyCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand StartTimerCommand { get; }
        public ICommand StopTimerCommand { get; }

        #endregion

        #region Вычисляемые свойства

        /// <summary>
        ///     Общие затраченные часы по всем задачам всех заказов.
        /// </summary>
        public double TotalHoursSpent => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Sum(t => t.HoursSpent);

        /// <summary>
        ///     Общая сумма денег по всем заказам.
        /// </summary>
        public double TotalMoneyEarned => Companies
            .SelectMany(c => c.Orders)
            .Sum(o => o.Price);

        /// <summary>
        ///     Средняя ставка по всем заказам (руб/час). Если часов нет, возвращает 0.
        /// </summary>
        public double AverageHourlyRate
        {
            get
            {
                var hours = TotalHoursSpent;
                return hours > 0 ? TotalMoneyEarned / hours : 0;
            }
        }

        #endregion

        #region Методы добавления

        private void AddCompany()
        {
            var company = new Company
            {
                Name = "Новая компания"
            };
            Companies.Add(company);
            SelectedCompany = company;
        }

        private void AddOrder()
        {
            if (SelectedCompany == null) return;
            var order = new Order
            {
                Price = 0,
                SoftwareType = "Revit",
                ExpectedDurationDays = 1
            };
            SelectedCompany.Orders.Add(order);
            OnPropertyChanged(nameof(Orders));
            SelectedOrder = order;
        }

        private void AddTask()
        {
            if (SelectedOrder == null) return;
            var task = new TaskItem
            {
                Title = "Новая задача",
                StartDate = DateTime.Today
            };
            SelectedOrder.Tasks.Add(task);
            OnPropertyChanged(nameof(Tasks));
            SelectedTask = task;
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
        }

        #endregion

        #region Работа с таймером

        private void StartTimer()
        {
            if (SelectedTask == null)
                return;
            _timerStartTime = DateTime.Now;
            _timer.Start();
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        private void StopTimer()
        {
            if (SelectedTask == null || !_timerStartTime.HasValue)
                return;
            _timer.Stop();
            var elapsed = DateTime.Now - _timerStartTime.Value;
            // Конвертировать в часы с точностью до двух знаков.
            SelectedTask.HoursSpent += Math.Round(elapsed.TotalHours, 2);
            _timerStartTime = null;
            OnPropertyChanged(nameof(Tasks));
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
            SelectedOrder?.UpdateProfitabilityStatus();
            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(SelectedOrder));
            OnPropertyChanged(nameof(Companies));
            OnPropertyChanged(nameof(TotalMoneyEarned));
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        private void OnTimerTick()
        {
            // Пока ничего не делаем каждую секунду, можно добавить индикацию.
        }

        #endregion

        #region Сохранение и загрузка

        private async Task SaveAsync()
        {
            var data = new DataStore
            {
                Companies = Companies.ToList(),
                CurrentTaskId = SelectedTask?.Id
            };
            await _dataService.SaveAsync(data);
        }

        private async Task LoadAsync()
        {
            var data = await _dataService.LoadAsync();
            Companies.Clear();
            if (data.Companies != null)
            {
                foreach (var company in data.Companies)
                {
                    Companies.Add(company);
                }
            }
            // Восстановить выбранную задачу
            if (data.CurrentTaskId.HasValue)
            {
                foreach (var company in Companies)
                {
                    foreach (var order in company.Orders)
                    {
                        var task = order.Tasks.FirstOrDefault(t => t.Id == data.CurrentTaskId.Value);
                        if (task != null)
                        {
                            SelectedCompany = company;
                            SelectedOrder = order;
                            SelectedTask = task;
                            break;
                        }
                    }
                    if (SelectedTask != null) break;
                }
            }
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
            OnPropertyChanged(nameof(TotalMoneyEarned));
        }

        #endregion
    }
}
