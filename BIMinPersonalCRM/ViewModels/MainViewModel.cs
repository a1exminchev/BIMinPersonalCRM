using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using BIMinPersonalCRM.Commands;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BIMinPersonalCRM.ViewModels
{
    /// <summary>
    ///     Ð“Ð»Ð°Ð²Ð½Ð°Ñ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¿Ñ€ÐµÐ´ÑÑ‚Ð°Ð²Ð»ÐµÐ½Ð¸Ñ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ. Ð£Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ ÐºÐ¾Ð»Ð»ÐµÐºÑ†Ð¸ÑÐ¼Ð¸ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¹,
    ///     Ð·Ð°ÐºÐ°Ð·Ð¾Ð² Ð¸ Ð·Ð°Ð´Ð°Ñ‡, Ð° Ñ‚Ð°ÐºÐ¶Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸ÐµÐ¹ ÐºÐ¾Ð¼Ð°Ð½Ð´ Ð¸ Ñ‚Ð°Ð¹Ð¼ÐµÑ€Ð°.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly System.Timers.Timer _timer;

        private Company? _selectedCompany;
        private Order? _selectedOrder;
        private TaskItem? _selectedTask;
        private Employee? _selectedEmployee;
        private FileAttachment? _selectedFile;
        private DateTime? _timerStartTime;
        
        // ÐŸÐ¾Ð¸ÑÐº Ð¸ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€Ð°Ñ†Ð¸Ñ
        private string _companySearchText = string.Empty;
        private string _orderSearchText = string.Empty;
        private RelationshipStatus? _selectedRelationshipFilter;
        private PaymentAbilityStatus? _selectedPaymentAbilityFilter;
        private Company? _selectedCompanyFilter;
        private OrderExecutionStatus? _selectedOrderStatusFilter;
        private readonly ObservableCollection<ISeries> _revenueTrendSeries = new();
        private readonly ObservableCollection<ISeries> _ordersByStatusSeries = new();
        private readonly ObservableCollection<ISeries> _tasksStatusSeries = new();
        private readonly ObservableCollection<ISeries> _revenueByCompanySeries = new();
        private readonly ObservableCollection<CompanyRevenueSummary> _companyRevenueSummaries = new();

        private Axis[] _revenueTrendXAxis = Array.Empty<Axis>();
        private Axis[] _revenueTrendYAxis = Array.Empty<Axis>();
        private Axis[] _ordersStatusXAxis = Array.Empty<Axis>();
        private Axis[] _ordersStatusYAxis = Array.Empty<Axis>();
        private Axis[] _tasksStatusXAxis = Array.Empty<Axis>();
        private Axis[] _tasksStatusYAxis = Array.Empty<Axis>();

        private StatisticsPeriodOption _selectedStatisticsPeriod;
        private StatisticsSortOption _selectedStatisticsSort;
        private string _statisticsSearchText = string.Empty;
        private double _statisticsMinOrders;

        /// <summary>
        ///     Ð˜Ð½Ð¸Ñ†Ð¸Ð°Ð»Ð¸Ð·Ð¸Ñ€ÑƒÐµÑ‚ Ð½Ð¾Ð²ÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¿Ñ€ÐµÐ´ÑÑ‚Ð°Ð²Ð»ÐµÐ½Ð¸Ñ Ð¸ Ð·Ð°Ð³Ñ€ÑƒÐ¶Ð°ÐµÑ‚ Ð´Ð°Ð½Ð½Ñ‹Ðµ.
        /// </summary>
        public MainViewModel()
        {
            // Ð¤Ð°Ð¹Ð» Ð´Ð°Ð½Ð½Ñ‹Ñ… Ð² Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ð¾Ð¼ ÐºÐ°Ñ‚Ð°Ð»Ð¾Ð³Ðµ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ.
            var dataPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIMinPersonalCRM",
                "data.json");
            _dataService = new JsonDataService(dataPath);

            Companies = new ObservableCollection<Company>();

            // Ð˜Ð½Ð¸Ñ†Ð¸Ð°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ ÐºÐ¾Ð¼Ð°Ð½Ð´
            AddCompanyCommand = new RelayCommand(_ => AddCompany());
            AddOrderCommand = new RelayCommand(_ => AddOrder(), _ => SelectedCompany != null);
            AddTaskCommand = new RelayCommand(_ => AddTask(), _ => SelectedOrder != null);
            AddEmployeeCommand = new RelayCommand(_ => AddEmployee(), _ => SelectedCompany != null);
            RemoveEmployeeCommand = new RelayCommand(RemoveEmployee, _ => SelectedCompany != null);
            AddFileCommand = new RelayCommand(_ => AddFile(), _ => SelectedOrder != null);
            RemoveFileCommand = new RelayCommand(RemoveFile, _ => SelectedOrder != null);
            DeleteCompanyCommand = new RelayCommand(_ => DeleteCompany(), _ => SelectedCompany != null);
            SelectLogoCommand = new RelayCommand(_ => SelectLogo(), _ => SelectedCompany != null);
            RemoveLogoCommand = new RelayCommand(_ => RemoveLogo(), _ => SelectedCompany != null);
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            StartTimerCommand = new RelayCommand(_ => StartTimer(), _ => SelectedTask != null && !IsTimerRunning);
            StopTimerCommand = new RelayCommand(_ => StopTimer(), _ => SelectedTask != null && IsTimerRunning);

            // Ð¢Ð°Ð¹Ð¼ÐµÑ€ Ñ ÑÐµÐºÑƒÐ½Ð´Ð½Ð¾Ð¹ Ð¿ÐµÑ€Ð¸Ð¾Ð´Ð¸Ñ‡Ð½Ð¾ÑÑ‚ÑŒÑŽ, Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÐµÑ‚ Ñ‡Ð°ÑÑ‹ Ñ€Ð°Ð· Ð² ÑÐµÐºÑƒÐ½Ð´Ñƒ.
            StatisticsPeriods = new List<StatisticsPeriodOption>
            {
                new(StatisticsPeriod.Last30Days, "Последние 30 дней"),
                new(StatisticsPeriod.Last90Days, "Последние 90 дней"),
                new(StatisticsPeriod.YearToDate, "С начала года"),
                new(StatisticsPeriod.AllTime, "За все время")
            };
            _selectedStatisticsPeriod = StatisticsPeriods[0];

            StatisticsSortOptions = new List<StatisticsSortOption>
            {
                new("По выручке (убывание)", nameof(CompanyRevenueSummary.TotalRevenue), ListSortDirection.Descending),
                new("По средней ставке (убывание)", nameof(CompanyRevenueSummary.AverageRate), ListSortDirection.Descending),
                new("По количеству заказов", nameof(CompanyRevenueSummary.OrdersCount), ListSortDirection.Descending),
                new("По дате последнего заказа", nameof(CompanyRevenueSummary.LastOrderDate), ListSortDirection.Descending)
            };
            _selectedStatisticsSort = StatisticsSortOptions[0];

            CompanyRevenueView = CollectionViewSource.GetDefaultView(_companyRevenueSummaries);
            CompanyRevenueView.Filter = FilterCompanyRevenueSummary;

            ApplyStatisticsSort();
            RefreshStatisticsDashboard();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (_, _) => OnTimerTick();

            // Ð—Ð°Ð³Ñ€ÑƒÐ·ÐºÐ° Ð´Ð°Ð½Ð½Ñ‹Ñ… Ð¿Ñ€Ð¸ Ð·Ð°Ð¿ÑƒÑÐºÐµ
            _ = LoadAsync();
        }

        #region ÐšÐ¾Ð»Ð»ÐµÐºÑ†Ð¸Ð¸ Ð¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ðµ ÑÐ»ÐµÐ¼ÐµÐ½Ñ‚Ñ‹

        /// <summary>
        ///     ÐšÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸, Ð¾Ñ‚Ð¾Ð±Ñ€Ð°Ð¶Ð°ÐµÐ¼Ñ‹Ðµ Ð² UI.
        /// </summary>
        public ObservableCollection<Company> Companies { get; }

        /// <summary>
        ///     Ð¢ÐµÐºÑƒÑ‰Ð°Ñ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð°Ñ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ñ.
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
                    OnPropertyChanged(nameof(Employees));
                    // Ð¡Ð±Ñ€Ð¾ÑÐ¸Ñ‚ÑŒ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ð·Ð°ÐºÐ°Ð· Ð¸ Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ð¿Ñ€Ð¸ ÑÐ¼ÐµÐ½Ðµ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸
                    SelectedOrder = null;
                    SelectedTask = null;
                    SelectedEmployee = null;
                    // ÐžÐ±Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ð¾ÑÑ‚ÑŒ ÐºÐ¾Ð¼Ð°Ð½Ð´
                    ((RelayCommand)AddOrderCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Ð—Ð°ÐºÐ°Ð·Ñ‹ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð¹ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸. Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ Ð¿ÑƒÑÑ‚Ð¾Ð¹ ÑÐ¿Ð¸ÑÐ¾Ðº, ÐµÑÐ»Ð¸ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ñ Ð½Ðµ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð°.
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
        ///     Ð¢ÐµÐºÑƒÑ‰Ð¸Ð¹ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ð·Ð°ÐºÐ°Ð·.
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
                    OnPropertyChanged(nameof(AttachedFiles));
                    // ÐŸÑ€Ð¸ ÑÐ¼ÐµÐ½Ðµ Ð·Ð°ÐºÐ°Ð·Ð° ÑÐ±Ñ€Ð¾ÑÐ¸Ñ‚ÑŒ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ
                    SelectedTask = null;
                    SelectedFile = null;
                    ((RelayCommand)AddTaskCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Ð—Ð°Ð´Ð°Ñ‡Ð¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð³Ð¾ Ð·Ð°ÐºÐ°Ð·Ð°. ÐŸÑƒÑÑ‚Ð¾Ð¹ ÑÐ¿Ð¸ÑÐ¾Ðº, ÐµÑÐ»Ð¸ Ð·Ð°ÐºÐ°Ð· Ð½Ðµ Ð²Ñ‹Ð±Ñ€Ð°Ð½.
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
        ///     Ð¢ÐµÐºÑƒÑ‰Ð°Ñ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð°.
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
        ///     Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ Ð¿Ñ€Ð¸Ð·Ð½Ð°Ðº, Ñ‡Ñ‚Ð¾ Ñ‚Ð°Ð¹Ð¼ÐµÑ€ Ð·Ð°Ð¿ÑƒÑ‰ÐµÐ½.
        /// </summary>
        public bool IsTimerRunning => _timer.Enabled && SelectedTask?.IsTimerRunning == true;

        /// <summary>
        /// Ð¡Ð¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸ÐºÐ¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð¹ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸
        /// </summary>
        public ObservableCollection<Employee> Employees
        {
            get
            {
                if (SelectedCompany != null)
                {
                    return new ObservableCollection<Employee>(SelectedCompany.Employees);
                }
                return new ObservableCollection<Employee>();
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸Ðº
        /// </summary>
        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ð¤Ð°Ð¹Ð»Ñ‹ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð³Ð¾ Ð·Ð°ÐºÐ°Ð·Ð°
        /// </summary>
        public ObservableCollection<FileAttachment> AttachedFiles
        {
            get
            {
                if (SelectedOrder != null)
                {
                    return new ObservableCollection<FileAttachment>(SelectedOrder.AttachedFiles);
                }
                return new ObservableCollection<FileAttachment>();
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ñ„Ð°Ð¹Ð»
        /// </summary>
        public FileAttachment? SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region ÐšÐ¾Ð¼Ð°Ð½Ð´Ñ‹

        public ICommand AddCompanyCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand RemoveEmployeeCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand RemoveFileCommand { get; }        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand StartTimerCommand { get; }
        public ICommand StopTimerCommand { get; }
        public ICommand DeleteCompanyCommand { get; }
        public ICommand SelectLogoCommand { get; }
        public ICommand RemoveLogoCommand { get; }

        #endregion

        #region Ð’Ñ‹Ñ‡Ð¸ÑÐ»ÑÐµÐ¼Ñ‹Ðµ ÑÐ²Ð¾Ð¹ÑÑ‚Ð²Ð°

        /// <summary>
        ///     ÐžÐ±Ñ‰Ð¸Ðµ Ð·Ð°Ñ‚Ñ€Ð°Ñ‡ÐµÐ½Ð½Ñ‹Ðµ Ñ‡Ð°ÑÑ‹ Ð¿Ð¾ Ð²ÑÐµÐ¼ Ð·Ð°Ð´Ð°Ñ‡Ð°Ð¼ Ð²ÑÐµÑ… Ð·Ð°ÐºÐ°Ð·Ð¾Ð².
        /// </summary>
        public double TotalHoursSpent => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Sum(t => t.HoursSpent);

        /// <summary>
        ///     ÐžÐ±Ñ‰Ð°Ñ ÑÑƒÐ¼Ð¼Ð° Ð´ÐµÐ½ÐµÐ³ Ð¿Ð¾ Ð²ÑÐµÐ¼ Ð·Ð°ÐºÐ°Ð·Ð°Ð¼.
        /// </summary>
        public double TotalMoneyEarned => Companies
            .SelectMany(c => c.Orders)
            .Sum(o => o.Price);

        /// <summary>
        ///     Ð¡Ñ€ÐµÐ´Ð½ÑÑ ÑÑ‚Ð°Ð²ÐºÐ° Ð¿Ð¾ Ð²ÑÐµÐ¼ Ð·Ð°ÐºÐ°Ð·Ð°Ð¼ (Ñ€ÑƒÐ±/Ñ‡Ð°Ñ). Ð•ÑÐ»Ð¸ Ñ‡Ð°ÑÐ¾Ð² Ð½ÐµÑ‚, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ 0.
        /// </summary>
        public double AverageHourlyRate
        {
            get
            {
                var hours = TotalHoursSpent;
                return hours > 0 ? TotalMoneyEarned / hours : 0;
            }
        }

        /// <summary>
        /// ÐžÐ±Ñ‰ÐµÐµ ÐºÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°Ð´Ð°Ñ‡
        /// </summary>
        public int TotalTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count();

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°Ð²ÐµÑ€ÑˆÐµÐ½Ð½Ñ‹Ñ… Ð·Ð°Ð´Ð°Ñ‡
        /// </summary>
        public int CompletedTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count(t => t.Status == Models.TaskStatus.Completed);

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°Ð´Ð°Ñ‡ Ð² Ñ€Ð°Ð±Ð¾Ñ‚Ðµ
        /// </summary>
        public int InProgressTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count(t => t.Status == Models.TaskStatus.InProgress);

        /// <summary>
        /// ÐŸÑ€Ð¾Ñ†ÐµÐ½Ñ‚ Ð·Ð°Ð²ÐµÑ€ÑˆÐµÐ½Ð¸Ñ Ð·Ð°Ð´Ð°Ñ‡
        /// </summary>
        public double TaskCompletionPercentage => 
            TotalTasksCount > 0 ? (double)CompletedTasksCount / TotalTasksCount * 100 : 0;

        /// <summary>
        /// ÐžÐ±Ñ‰ÐµÐµ ÐºÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public int TotalOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count();

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°Ð²ÐµÑ€ÑˆÐµÐ½Ð½Ñ‹Ñ… Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public int CompletedOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid);

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°ÐºÐ°Ð·Ð¾Ð² Ð² Ñ€Ð°Ð±Ð¾Ñ‚Ðµ
        /// </summary>
        public int InProgressOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress);

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð¾Ð¿Ð»Ð°Ñ‡ÐµÐ½Ð½Ñ‹Ñ… Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public int PaidOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid);

        /// <summary>
        /// ÐžÐ±Ñ‰ÐµÐµ ÐºÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¹
        /// </summary>
        public int TotalCompaniesCount => Companies.Count;

        /// <summary>
        /// ÐšÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð°ÐºÑ‚Ð¸Ð²Ð½Ñ‹Ñ… ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¹ (Ð¸Ð¼ÐµÑŽÑ‰Ð¸Ñ… Ð·Ð°ÐºÐ°Ð·Ñ‹)
        /// </summary>
        public int ActiveCompaniesCount => Companies.Count(c => c.Orders.Any());

        /// <summary>
        /// Ð¡Ñ€ÐµÐ´Ð½ÐµÐµ ÐºÐ¾Ð»Ð¸Ñ‡ÐµÑÑ‚Ð²Ð¾ Ð·Ð°ÐºÐ°Ð·Ð¾Ð² Ð½Ð° ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸ÑŽ
        /// </summary>
        public double AverageOrdersPerCompany => 
            TotalCompaniesCount > 0 ? (double)TotalOrdersCount / TotalCompaniesCount : 0;

        /// <summary>
        /// Ð¡Ñ€ÐµÐ´Ð½ÑÑ ÑÑ‚Ð¾Ð¸Ð¼Ð¾ÑÑ‚ÑŒ Ð·Ð°ÐºÐ°Ð·Ð°
        /// </summary>
        public double AverageOrderValue => 
            TotalOrdersCount > 0 ? TotalMoneyEarned / TotalOrdersCount : 0;

        /// <summary>
        /// Ð¡Ñ€ÐµÐ´Ð½ÐµÐµ Ñ‡Ð°ÑÐ¾Ð² Ð½Ð° Ð·Ð°Ð´Ð°Ñ‡Ñƒ
        /// </summary>
        public double AverageHoursPerTask => 
            TotalTasksCount > 0 ? TotalHoursSpent / TotalTasksCount : 0;

        /// <summary>
        /// ÐÐ°Ð·Ð²Ð°Ð½Ð¸Ðµ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¹ Ð·Ð°Ð´Ð°Ñ‡Ð¸
        /// </summary>
        public string CurrentTaskName => SelectedTask?.Name ?? "ÐÐµ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð°";

        /// <summary>
        /// Ð¡Ñ‚Ð°Ñ‚ÑƒÑ Ñ‚Ð°Ð¹Ð¼ÐµÑ€Ð°
        /// </summary>
        public string TimerStatus => IsTimerRunning ? "Ð—Ð°Ð¿ÑƒÑ‰ÐµÐ½" : "ÐžÑÑ‚Ð°Ð½Ð¾Ð²Ð»ÐµÐ½";

        /// <summary>
        /// Ð§Ð°ÑÑ‹ Ð·Ð° ÑÐµÐ³Ð¾Ð´Ð½Ñ
        /// </summary>
        public double TodayHours => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Where(t => t.StartDate.Date == DateTime.Today)
            .Sum(t => t.HoursSpent);

        /// <summary>
        /// Ð§Ð°ÑÑ‹ Ð·Ð° Ñ‚ÐµÐºÑƒÑ‰ÑƒÑŽ Ð½ÐµÐ´ÐµÐ»ÑŽ
        /// </summary>
        public double WeekHours
        {
            get
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                return Companies
                    .SelectMany(c => c.Orders)
                    .SelectMany(o => o.Tasks)
                    .Where(t => t.StartDate >= startOfWeek)
                    .Sum(t => t.HoursSpent);
            }
        }

        /// <summary>
        /// ÐŸÑ€Ð¸Ð±Ð»Ð¸Ð·Ð¸Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ñ‡Ð°ÑÑ‹ Ð² Ð¼ÐµÑÑÑ† (Ð½Ð° Ð¾ÑÐ½Ð¾Ð²Ðµ Ð½ÐµÐ´ÐµÐ»ÑŒÐ½Ð¾Ð¹ ÑÑ‚Ð°Ñ‚Ð¸ÑÑ‚Ð¸ÐºÐ¸)
        /// </summary>
        public double MonthlyHours => WeekHours * 4.33;

        /// <summary>
        /// ÐŸÑ€Ð¸Ð±Ð»Ð¸Ð·Ð¸Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ð¹ Ð´Ð¾Ñ…Ð¾Ð´ Ð² Ð¼ÐµÑÑÑ†
        /// </summary>
        public double MonthlyIncome => MonthlyHours * AverageHourlyRate;

        /// <summary>
        /// ÐÐ½Ð°Ð»Ð¸Ð· Ð¿Ñ€Ð¸Ð±Ñ‹Ð»ÑŒÐ½Ð¾ÑÑ‚Ð¸ Ð¿Ð¾ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸ÑÐ¼
        /// </summary>
        /// <summary>
        ///     Provides profitability insights grouped by company.
        /// </summary>
        public List<ProfitabilityAnalysisItem> ProfitabilityAnalysis
        {
            get
            {
                return Companies.Where(c => c.Orders.Any()).Select(c => new ProfitabilityAnalysisItem
                {
                    CompanyName = c.Name,
                    OrdersCount = c.Orders.Count,
                    TotalAmount = c.Orders.Sum(o => o.Price),
                    TotalHours = c.Orders.SelectMany(o => o.Tasks).Sum(t => t.HoursSpent),
                    AverageRate = c.Orders.SelectMany(o => o.Tasks).Sum(t => t.HoursSpent) > 0
                        ? c.Orders.Sum(o => o.Price) / c.Orders.SelectMany(o => o.Tasks).Sum(t => t.HoursSpent)
                        : 0,
                    ProfitabilityLevel = c.AverageProfitability.GetDescription()
                }).OrderByDescending(x => x.AverageRate).ToList();
            }
        }

        #region Statistics dashboard

        public ObservableCollection<ISeries> RevenueTrendSeries => _revenueTrendSeries;

        public ObservableCollection<ISeries> OrdersByStatusSeries => _ordersByStatusSeries;

        public ObservableCollection<ISeries> TasksStatusSeries => _tasksStatusSeries;

        public ObservableCollection<ISeries> RevenueByCompanySeries => _revenueByCompanySeries;

        public Axis[] RevenueTrendXAxis => _revenueTrendXAxis;

        public Axis[] RevenueTrendYAxis => _revenueTrendYAxis;

        public Axis[] OrdersByStatusXAxis => _ordersStatusXAxis;

        public Axis[] OrdersByStatusYAxis => _ordersStatusYAxis;

        public Axis[] TasksStatusXAxis => _tasksStatusXAxis;

        public Axis[] TasksStatusYAxis => _tasksStatusYAxis;

        public IReadOnlyList<StatisticsPeriodOption> StatisticsPeriods { get; }

        public IReadOnlyList<StatisticsSortOption> StatisticsSortOptions { get; }

        public StatisticsPeriodOption SelectedStatisticsPeriod
        {
            get => _selectedStatisticsPeriod;
            set
            {
                if (_selectedStatisticsPeriod != value)
                {
                    _selectedStatisticsPeriod = value;
                    OnPropertyChanged();
                    RefreshStatisticsDashboard();
                }
            }
        }

        public StatisticsSortOption SelectedStatisticsSort
        {
            get => _selectedStatisticsSort;
            set
            {
                if (_selectedStatisticsSort != value)
                {
                    _selectedStatisticsSort = value;
                    OnPropertyChanged();
                    ApplyStatisticsSort();
                }
            }
        }

        public string StatisticsSearchText
        {
            get => _statisticsSearchText;
            set
            {
                if (_statisticsSearchText != value)
                {
                    _statisticsSearchText = value;
                    OnPropertyChanged();
                    CompanyRevenueView?.Refresh();
                }
            }
        }

        public double StatisticsMinOrders
        {
            get => _statisticsMinOrders;
            set
            {
                if (Math.Abs(_statisticsMinOrders - value) > 0.0001)
                {
                    _statisticsMinOrders = value;
                    OnPropertyChanged();
                    CompanyRevenueView?.Refresh();
                }
            }
        }

        public ObservableCollection<CompanyRevenueSummary> CompanyRevenueSummaries => _companyRevenueSummaries;

        public ICollectionView CompanyRevenueView { get; }

        #endregion

        #region ÐŸÐ¾Ð¸ÑÐº Ð¸ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€Ð°Ñ†Ð¸Ñ

        /// <summary>
        /// Ð¢ÐµÐºÑÑ‚ Ð¿Ð¾Ð¸ÑÐºÐ° ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¹
        /// </summary>
        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                if (_companySearchText != value)
                {
                    _companySearchText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredCompanies));
                }
            }
        }

        /// <summary>
        /// Ð¢ÐµÐºÑÑ‚ Ð¿Ð¾Ð¸ÑÐºÐ° Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public string OrderSearchText
        {
            get => _orderSearchText;
            set
            {
                if (_orderSearchText != value)
                {
                    _orderSearchText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredOrders));
                }
            }
        }

        /// <summary>
        /// Ð¤Ð¸Ð»ÑŒÑ‚Ñ€Ð¾Ð²Ð°Ð½Ð½Ñ‹Ðµ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸
        /// </summary>
        public ObservableCollection<Company> FilteredCompanies
        {
            get
            {
                var filtered = Companies.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(CompanySearchText))
                {
                    filtered = filtered.Where(c => c.Name.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) ||
                                                   c.Phone.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) ||
                                                   c.Website.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (_selectedRelationshipFilter.HasValue)
                {
                    filtered = filtered.Where(c => c.RelationshipStatus == _selectedRelationshipFilter.Value);
                }

                if (_selectedPaymentAbilityFilter.HasValue)
                {
                    filtered = filtered.Where(c => c.PaymentAbilityStatus == _selectedPaymentAbilityFilter.Value);
                }

                return new ObservableCollection<Company>(filtered);
            }
        }

        /// <summary>
        /// Ð¤Ð¸Ð»ÑŒÑ‚Ñ€Ð¾Ð²Ð°Ð½Ð½Ñ‹Ðµ Ð·Ð°ÐºÐ°Ð·Ñ‹
        /// </summary>
        public ObservableCollection<Order> FilteredOrders
        {
            get
            {
                var allOrders = Companies.SelectMany(c => c.Orders);

                if (!string.IsNullOrWhiteSpace(OrderSearchText))
                {
                    allOrders = allOrders.Where(o => o.Name.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                     o.SoftwareType.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (_selectedCompanyFilter != null)
                {
                    allOrders = allOrders.Where(o => Companies.FirstOrDefault(c => c.Orders.Contains(o)) == _selectedCompanyFilter);
                }

                if (_selectedOrderStatusFilter.HasValue)
                {
                    allOrders = allOrders.Where(o => o.ExecutionStatus == _selectedOrderStatusFilter.Value);
                }

                return new ObservableCollection<Order>(allOrders.OrderByDescending(o => o.CreatedDate));
            }
        }

        /// <summary>
        /// Ð’ÑÐµ Ð·Ð°Ð´Ð°Ñ‡Ð¸ Ð¸Ð· Ð²ÑÐµÑ… Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public ObservableCollection<TaskItem> AllTasks
        {
            get
            {
                var allTasks = Companies.SelectMany(c => c.Orders).SelectMany(o => o.Tasks);
                return new ObservableCollection<TaskItem>(allTasks.OrderByDescending(t => t.StartDate));
            }
        }

        /// <summary>
        /// Ð¤Ð¸Ð»ÑŒÑ‚Ñ€Ñ‹ Ð´Ð»Ñ ÑÑ‚Ð°Ñ‚ÑƒÑÐ¾Ð² Ð¾Ñ‚Ð½Ð¾ÑˆÐµÐ½Ð¸Ð¹
        /// </summary>
        public List<RelationshipStatus?> RelationshipStatusFilter
        {
            get
            {
                var list = new List<RelationshipStatus?> { null };
                list.AddRange(Enum.GetValues<RelationshipStatus>().Cast<RelationshipStatus?>());
                return list;
            }
        }

        /// <summary>
        /// Ð¤Ð¸Ð»ÑŒÑ‚Ñ€Ñ‹ Ð´Ð»Ñ ÑÑ‚Ð°Ñ‚ÑƒÑÐ¾Ð² Ð¿Ð»Ð°Ñ‚ÐµÐ¶ÐµÑÐ¿Ð¾ÑÐ¾Ð±Ð½Ð¾ÑÑ‚Ð¸
        /// </summary>
        public List<PaymentAbilityStatus?> PaymentAbilityFilter
        {
            get
            {
                var list = new List<PaymentAbilityStatus?> { null };
                list.AddRange(Enum.GetValues<PaymentAbilityStatus>().Cast<PaymentAbilityStatus?>());
                return list;
            }
        }

        /// <summary>
        /// Ð¤Ð¸Ð»ÑŒÑ‚Ñ€Ñ‹ Ð´Ð»Ñ ÑÑ‚Ð°Ñ‚ÑƒÑÐ¾Ð² Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public List<OrderExecutionStatus?> OrderStatusFilter
        {
            get
            {
                var list = new List<OrderExecutionStatus?> { null };
                list.AddRange(Enum.GetValues<OrderExecutionStatus>().Cast<OrderExecutionStatus?>());
                return list;
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€ Ð¾Ñ‚Ð½Ð¾ÑˆÐµÐ½Ð¸Ð¹
        /// </summary>
        public RelationshipStatus? SelectedRelationshipFilter
        {
            get => _selectedRelationshipFilter;
            set
            {
                if (_selectedRelationshipFilter != value)
                {
                    _selectedRelationshipFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredCompanies));
                }
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€ Ð¿Ð»Ð°Ñ‚ÐµÐ¶ÐµÑÐ¿Ð¾ÑÐ¾Ð±Ð½Ð¾ÑÑ‚Ð¸
        /// </summary>
        public PaymentAbilityStatus? SelectedPaymentAbilityFilter
        {
            get => _selectedPaymentAbilityFilter;
            set
            {
                if (_selectedPaymentAbilityFilter != value)
                {
                    _selectedPaymentAbilityFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredCompanies));
                }
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸ Ð´Ð»Ñ Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
        /// </summary>
        public Company? SelectedCompanyFilter
        {
            get => _selectedCompanyFilter;
            set
            {
                if (_selectedCompanyFilter != value)
                {
                    _selectedCompanyFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredOrders));
                }
            }
        }

        /// <summary>
        /// Ð’Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ð¹ Ñ„Ð¸Ð»ÑŒÑ‚Ñ€ ÑÑ‚Ð°Ñ‚ÑƒÑÐ° Ð·Ð°ÐºÐ°Ð·Ð°
        /// </summary>
        public OrderExecutionStatus? SelectedOrderStatusFilter
        {
            get => _selectedOrderStatusFilter;
            set
            {
                if (_selectedOrderStatusFilter != value)
                {
                    _selectedOrderStatusFilter = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FilteredOrders));
                }
            }
        }

        #endregion

        #endregion

        #region ÐœÐµÑ‚Ð¾Ð´Ñ‹ Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½Ð¸Ñ

        private void AddCompany()
        {
            var dialog = new Views.Dialogs.AddCompanyDialog();
            if (dialog.ShowDialog() == true)
            {
                var company = dialog.Company;
                
                // ÐŸÑ€Ð¾Ð²ÐµÑ€ÑÐµÐ¼ ÑƒÐ½Ð¸ÐºÐ°Ð»ÑŒÐ½Ð¾ÑÑ‚ÑŒ Ð¸Ð¼ÐµÐ½Ð¸
                var baseName = company.Name;
                var name = baseName;
                var counter = 1;
                
                while (Companies.Any(c => c.Name == name))
                {
                    name = $"{baseName} ({counter})";
                    counter++;
                }
                
                company.Id = GetNextCompanyId();
                company.Name = name;
                
                Companies.Add(company);
                SelectedCompany = company;
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        private void AddOrder()
        {
            if (SelectedCompany == null) return;

            var order = new Order
            {
                Id = GetNextOrderId(),
                Name = GenerateUniqueOrderName(SelectedCompany, "Новый заказ"),
                Price = 0,
                SoftwareType = "Revit",
                ExpectedDurationDays = 1
            };

            SelectedCompany.Orders.Add(order);
            order.UpdateProfitabilityStatus();

            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(FilteredOrders));

            SelectedOrder = order;
            UpdateAllStatistics();
        }

        private void AddTask()
        {
            if (SelectedOrder == null) return;
            var task = new TaskItem
            {
                Id = GetNextTaskId(),
                Name = "ÐÐ¾Ð²Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð°",
                StartDate = DateTime.Today
            };
            SelectedOrder.Tasks.Add(task);
            OnPropertyChanged(nameof(Tasks));
            SelectedTask = task;
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
            UpdateAllStatistics();
        }

        private void AddEmployee()
        {
            if (SelectedCompany == null) return;
            
            var baseName = "ÐÐ¾Ð²Ñ‹Ð¹ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸Ðº";
            var name = baseName;
            var counter = 1;
            
            // ÐŸÑ€Ð¾Ð²ÐµÑ€ÑÐµÐ¼ ÑƒÐ½Ð¸ÐºÐ°Ð»ÑŒÐ½Ð¾ÑÑ‚ÑŒ Ð¸Ð¼ÐµÐ½Ð¸ ÑÑ€ÐµÐ´Ð¸ Ð²ÑÐµÑ… ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸ÐºÐ¾Ð²
            var allEmployees = Companies.SelectMany(c => c.Employees).ToList();
            while (allEmployees.Any(e => e.FullName == name))
            {
                name = $"{baseName} ({counter})";
                counter++;
            }
            
            var employee = new Employee
            {
                Id = GetNextEmployeeId(),
                FullName = name,
                Position = "Ð”Ð¾Ð»Ð¶Ð½Ð¾ÑÑ‚ÑŒ"
            };
            SelectedCompany.Employees.Add(employee);
            OnPropertyChanged(nameof(Employees));
            SelectedEmployee = employee;
        }

        private void RemoveEmployee(object? parameter)
        {
            if (SelectedCompany == null || SelectedEmployee == null) return;
            SelectedCompany.Employees.Remove(SelectedEmployee);
            OnPropertyChanged(nameof(Employees));
            SelectedEmployee = null;
        }

        private void AddFile()
        {
            if (SelectedOrder == null) return;
            
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Ð’Ñ‹Ð±ÐµÑ€Ð¸Ñ‚Ðµ Ñ„Ð°Ð¹Ð» Ð´Ð»Ñ Ð¿Ñ€Ð¸ÐºÑ€ÐµÐ¿Ð»ÐµÐ½Ð¸Ñ",
                Filter = "Ð’ÑÐµ Ñ„Ð°Ð¹Ð»Ñ‹ (*.*)|*.*|Ð”Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ñ‹ (*.pdf;*.doc;*.docx)|*.pdf;*.doc;*.docx|Ð˜Ð·Ð¾Ð±Ñ€Ð°Ð¶ÐµÐ½Ð¸Ñ (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                var fileAttachment = new FileAttachment
                {
                    Id = GetNextFileId(),
                    Name = fileInfo.Name,
                    FilePath = openFileDialog.FileName,
                    FileSize = fileInfo.Length,
                    FileType = DetermineFileType(fileInfo.Extension)
                };
                
                SelectedOrder.AttachedFiles.Add(fileAttachment);
                OnPropertyChanged(nameof(AttachedFiles));
            }
        }

        private void RemoveFile(object? parameter)
        {
            if (SelectedOrder == null || SelectedFile == null) return;
            SelectedOrder.AttachedFiles.Remove(SelectedFile);
            OnPropertyChanged(nameof(AttachedFiles));
            SelectedFile = null;
        }

        private void DeleteCompany()
        {
            if (SelectedCompany == null) return;
            
            var result = System.Windows.MessageBox.Show(
                $"Ð’Ñ‹ Ð´ÐµÐ¹ÑÑ‚Ð²Ð¸Ñ‚ÐµÐ»ÑŒÐ½Ð¾ Ñ…Ð¾Ñ‚Ð¸Ñ‚Ðµ ÑƒÐ´Ð°Ð»Ð¸Ñ‚ÑŒ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸ÑŽ '{SelectedCompany.Name}'? Ð’ÑÐµ ÑÐ²ÑÐ·Ð°Ð½Ð½Ñ‹Ðµ Ð·Ð°ÐºÐ°Ð·Ñ‹ Ð¸ Ð·Ð°Ð´Ð°Ñ‡Ð¸ Ñ‚Ð°ÐºÐ¶Ðµ Ð±ÑƒÐ´ÑƒÑ‚ ÑƒÐ´Ð°Ð»ÐµÐ½Ñ‹.",
                "ÐŸÐ¾Ð´Ñ‚Ð²ÐµÑ€Ð¶Ð´ÐµÐ½Ð¸Ðµ ÑƒÐ´Ð°Ð»ÐµÐ½Ð¸Ñ",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Companies.Remove(SelectedCompany);
                SelectedCompany = null;
                OnPropertyChanged(nameof(FilteredCompanies));
                UpdateAllStatistics();
            }
        }

        private void SelectLogo()
        {
            if (SelectedCompany == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Ð’Ñ‹Ð±ÐµÑ€Ð¸Ñ‚Ðµ Ð»Ð¾Ð³Ð¾Ñ‚Ð¸Ð¿ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸",
                Filter = "Ð˜Ð·Ð¾Ð±Ñ€Ð°Ð¶ÐµÐ½Ð¸Ñ|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Ð’ÑÐµ Ñ„Ð°Ð¹Ð»Ñ‹|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                SelectedCompany.LogoPath = dialog.FileName;
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        private void RemoveLogo()
        {
            if (SelectedCompany == null) return;
            SelectedCompany.LogoPath = "";
            OnPropertyChanged(nameof(SelectedCompany));
        }
        #endregion

        #region Ð Ð°Ð±Ð¾Ñ‚Ð° Ñ Ñ‚Ð°Ð¹Ð¼ÐµÑ€Ð¾Ð¼

        private void StartTimer()
        {
            if (SelectedTask == null)
                return;
            
            // ÐžÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ Ñ‚Ð°Ð¹Ð¼ÐµÑ€ Ð´Ð»Ñ Ð²ÑÐµÑ… Ð´Ñ€ÑƒÐ³Ð¸Ñ… Ð·Ð°Ð´Ð°Ñ‡
            foreach (var company in Companies)
            {
                foreach (var order in company.Orders)
                {
                    foreach (var task in order.Tasks)
                    {
                        if (task != SelectedTask && task.IsTimerRunning)
                        {
                            task.IsTimerRunning = false;
                            task.TimerStartTime = null;
                        }
                    }
                }
            }
            
            _timerStartTime = DateTime.Now;
            SelectedTask.TimerStartTime = _timerStartTime;
            SelectedTask.IsTimerRunning = true;
            _timer.Start();
            OnPropertyChanged(nameof(IsTimerRunning));
        }

        private void StopTimer()
        {
            if (SelectedTask == null || !_timerStartTime.HasValue)
                return;
            _timer.Stop();
            var elapsed = DateTime.Now - _timerStartTime.Value;
            // ÐšÐ¾Ð½Ð²ÐµÑ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ Ð² Ñ‡Ð°ÑÑ‹ Ñ Ñ‚Ð¾Ñ‡Ð½Ð¾ÑÑ‚ÑŒÑŽ Ð´Ð¾ Ð´Ð²ÑƒÑ… Ð·Ð½Ð°ÐºÐ¾Ð².
            SelectedTask.HoursSpent += Math.Round(elapsed.TotalHours, 2);
            SelectedTask.IsTimerRunning = false;
            SelectedTask.TimerStartTime = null;
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
            UpdateAllStatistics();
        }

        private void OnTimerTick()
        {
            // ÐŸÐ¾ÐºÐ° Ð½Ð¸Ñ‡ÐµÐ³Ð¾ Ð½Ðµ Ð´ÐµÐ»Ð°ÐµÐ¼ ÐºÐ°Ð¶Ð´ÑƒÑŽ ÑÐµÐºÑƒÐ½Ð´Ñƒ, Ð¼Ð¾Ð¶Ð½Ð¾ Ð´Ð¾Ð±Ð°Ð²Ð¸Ñ‚ÑŒ Ð¸Ð½Ð´Ð¸ÐºÐ°Ñ†Ð¸ÑŽ.
        }

        #endregion

        #region Ð’ÑÐ¿Ð¾Ð¼Ð¾Ð³Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ð¼ÐµÑ‚Ð¾Ð´Ñ‹

        /// <summary>
        /// Ð“ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ð¹ ID Ð´Ð»Ñ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸
        /// </summary>
        private int GetNextCompanyId()
        {
            return Companies.Any() ? Companies.Max(c => c.Id) + 1 : 1;
        }

        /// <summary>
        /// Ð“ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ð¹ ID Ð´Ð»Ñ Ð·Ð°ÐºÐ°Ð·Ð°
        /// </summary>
        private int GetNextOrderId()
        {
            var allOrders = Companies.SelectMany(c => c.Orders);
            return allOrders.Any() ? allOrders.Max(o => o.Id) + 1 : 1;
        }

        /// <summary>
        /// Ð“ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ð¹ ID Ð´Ð»Ñ Ð·Ð°Ð´Ð°Ñ‡Ð¸
        /// </summary>
        private int GetNextTaskId()
        {
            var allTasks = Companies.SelectMany(c => c.Orders).SelectMany(o => o.Tasks);
            return allTasks.Any() ? allTasks.Max(t => t.Id) + 1 : 1;
        }

        /// <summary>
        /// Ð“ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ð¹ ID Ð´Ð»Ñ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸ÐºÐ°
        /// </summary>
        private int GetNextEmployeeId()
        {
            var allEmployees = Companies.SelectMany(c => c.Employees);
            return allEmployees.Any() ? allEmployees.Max(e => e.Id) + 1 : 1;
        }

        /// <summary>
        /// Ð“ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ð¹ ID Ð´Ð»Ñ Ñ„Ð°Ð¹Ð»Ð°
        /// </summary>
        private int GetNextFileId()
        {
            var allFiles = Companies.SelectMany(c => c.Orders).SelectMany(o => o.AttachedFiles);
            return allFiles.Any() ? allFiles.Max(f => f.Id) + 1 : 1;
        }

        /// <summary>
        /// ÐžÐ¿Ñ€ÐµÐ´ÐµÐ»ÑÐµÑ‚ Ñ‚Ð¸Ð¿ Ñ„Ð°Ð¹Ð»Ð° Ð¿Ð¾ Ñ€Ð°ÑÑˆÐ¸Ñ€ÐµÐ½Ð¸ÑŽ
        /// </summary>
        private FileType DetermineFileType(string extension)

        {

            return extension.ToLower() switch

            {

                ".pdf" when extension.Contains("", StringComparison.OrdinalIgnoreCase) => FileType.Contract,

                ".doc" or ".docx" when extension.Contains("", StringComparison.OrdinalIgnoreCase) => FileType.TechnicalSpec,

                ".pdf" when extension.Contains("", StringComparison.OrdinalIgnoreCase) => FileType.Invoice,

                ".pdf" when extension.Contains("", StringComparison.OrdinalIgnoreCase) => FileType.Receipt,

                _ => FileType.Other

            };

        }



        private string GenerateUniqueOrderName(Company company, string? proposedName, Order? ignoreOrder = null)

        {

            var baseName = string.IsNullOrWhiteSpace(proposedName) ? "Новый заказ" : proposedName.Trim();

            baseName = Regex.Replace(baseName, @"\s\(\d+\)$", string.Empty);



            var uniqueName = baseName;

            var counter = 1;



            while (company.Orders.Any(o => !ReferenceEquals(o, ignoreOrder) && string.Equals(o.Name, uniqueName, StringComparison.OrdinalIgnoreCase)))

            {

                uniqueName = $"{baseName} ({counter})";

                counter++;

            }



            return uniqueName;

        }



        public void EnsureUniqueOrderName(Order order)

        {

            if (order == null) return;



            var owner = Companies.FirstOrDefault(c => c.Orders.Contains(order)) ?? SelectedCompany;

            if (owner == null) return;



            var uniqueName = GenerateUniqueOrderName(owner, order.Name, order);

            if (!string.Equals(order.Name, uniqueName, StringComparison.Ordinal))

            {

                order.Name = uniqueName;

                OnPropertyChanged(nameof(Orders));

                OnPropertyChanged(nameof(FilteredOrders));

                UpdateAllStatistics();

            }

        }



        private void EnsureUniqueOrderNames(Company company)

        {

            foreach (var order in company.Orders)

            {

                var uniqueName = GenerateUniqueOrderName(company, order.Name, order);

                if (!string.Equals(order.Name, uniqueName, StringComparison.Ordinal))

                {

                    order.Name = uniqueName;

                }

            }

        }

        /// <summary>
        /// ÐžÐ±Ð½Ð¾Ð²Ð»ÑÐµÑ‚ Ð²ÑÐµ ÑÑ‚Ð°Ñ‚Ð¸ÑÑ‚Ð¸Ñ‡ÐµÑÐºÐ¸Ðµ ÑÐ²Ð¾Ð¹ÑÑ‚Ð²Ð°
        /// </summary>


        #endregion

        #region Ð¡Ð¾Ñ…Ñ€Ð°Ð½ÐµÐ½Ð¸Ðµ Ð¸ Ð·Ð°Ð³Ñ€ÑƒÐ·ÐºÐ°

        private void RefreshStatisticsDashboard()
        {
            var culture = CultureInfo.CurrentCulture;
            var startDate = GetStatisticsStartDate();

            var orders = Companies
                .SelectMany(company => company.Orders, (company, order) => new { company, order })
                .Where(x => !startDate.HasValue || x.order.CreatedDate.Date >= startDate.Value)
                .ToList();

            var monthlyRevenue = orders
                .GroupBy(x => new DateTime(x.order.CreatedDate.Year, x.order.CreatedDate.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(x => x.order.Price) })
                .ToList();

            _revenueTrendSeries.Clear();
            if (monthlyRevenue.Any())
            {
                _revenueTrendSeries.Add(new LineSeries<double>
                {
                    Values = monthlyRevenue.Select(m => m.Revenue).ToArray(),
                    Fill = null,
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                    GeometryStroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                    GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255), 1),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture)
                });
            }

            _revenueTrendXAxis = new[]
            {
                new Axis
                {
                    Labels = monthlyRevenue.Select(m => m.Month.ToString("MMM yyyy", culture)).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            _revenueTrendYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };
            OnPropertyChanged(nameof(RevenueTrendXAxis));
            OnPropertyChanged(nameof(RevenueTrendYAxis));

            var statusData = orders
                .GroupBy(x => x.order.ExecutionStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderBy(g => g.Status)
                .ToList();

            _ordersByStatusSeries.Clear();
            if (statusData.Any())
            {
                _ordersByStatusSeries.Add(new ColumnSeries<int>
                {
                    Values = statusData.Select(s => s.Count).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(30, 136, 229, 180)),
                    Stroke = new SolidColorPaint(new SKColor(30, 136, 229)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture)
                });
            }

            _ordersStatusXAxis = new[]
            {
                new Axis
                {
                    Labels = statusData.Select(s => s.Status.GetDescription()).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            _ordersStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };
            OnPropertyChanged(nameof(OrdersByStatusXAxis));
            OnPropertyChanged(nameof(OrdersByStatusYAxis));

            IEnumerable<TaskItem> tasksQuery = Companies.SelectMany(company => company.Orders).SelectMany(order => order.Tasks);
            if (startDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(task => task.StartDate >= startDate.Value);
            }

            var tasksData = tasksQuery
                .GroupBy(task => task.Status)
                .Select(g => new { Status = g.Key, Hours = g.Sum(t => t.HoursSpent) })
                .OrderBy(g => g.Status)
                .ToList();

            _tasksStatusSeries.Clear();
            if (tasksData.Any())
            {
                _tasksStatusSeries.Add(new ColumnSeries<double>
                {
                    Values = tasksData.Select(t => t.Hours).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(76, 175, 80, 180)),
                    Stroke = new SolidColorPaint(new SKColor(56, 142, 60)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N1", culture)
                });
            }

            _tasksStatusXAxis = new[]
            {
                new Axis
                {
                    Labels = tasksData.Select(t => t.Status.GetDescription()).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            _tasksStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N1", culture),
                    TextSize = 12
                }
            };
            OnPropertyChanged(nameof(TasksStatusXAxis));
            OnPropertyChanged(nameof(TasksStatusYAxis));

            _revenueByCompanySeries.Clear();
            var revenueByCompany = orders
                .GroupBy(x => x.company.Name)
                .Select(g => new { Name = g.Key, Revenue = g.Sum(item => item.order.Price) })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            if (revenueByCompany.Any())
            {
                var topCompanies = revenueByCompany.Take(7).ToList();
                var otherRevenue = revenueByCompany.Skip(7).Sum(x => x.Revenue);
                if (otherRevenue > 0)
                {
                    topCompanies.Add(new { Name = "Прочие", Revenue = otherRevenue });
                }

                foreach (var item in topCompanies.Where(x => x.Revenue > 0))
                {
                    _revenueByCompanySeries.Add(new PieSeries<double>
                    {
                        Values = new[] { item.Revenue },
                        Name = item.Name,
                        DataLabelsPaint = new SolidColorPaint(new SKColor(33, 33, 33)),
                        DataLabelsSize = 12,
                        DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture),
                        Pushout = topCompanies.Count > 1 ? 4 : 0
                    });
                }

                if (topCompanies.All(x => x.Revenue == 0))
                {
                    _revenueByCompanySeries.Add(new PieSeries<double>
                    {
                        Values = new[] { 1d },
                        Name = "Нет данных",
                        DataLabelsPaint = new SolidColorPaint(new SKColor(33, 33, 33)),
                        DataLabelsSize = 12,
                        DataLabelsFormatter = _ => string.Empty
                    });
                }
            }

            _companyRevenueSummaries.Clear();
            foreach (var company in Companies)
            {
                var companyOrders = company.Orders
                    .Where(order => !startDate.HasValue || order.CreatedDate.Date >= startDate.Value)
                    .ToList();

                var totalRevenue = companyOrders.Sum(o => o.Price);
                var totalHours = companyOrders.SelectMany(o => o.Tasks).Sum(t => t.HoursSpent);

                var summary = new CompanyRevenueSummary
                {
                    CompanyName = company.Name,
                    OrdersCount = companyOrders.Count,
                    ActiveOrdersCount = companyOrders.Count(o => o.ExecutionStatus is OrderExecutionStatus.InProgress or OrderExecutionStatus.Testing or OrderExecutionStatus.AwaitingPayment),
                    CompletedOrdersCount = companyOrders.Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid),
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = companyOrders.Count > 0 ? totalRevenue / companyOrders.Count : 0,
                    TotalHours = totalHours,
                    AverageRate = totalHours > 0 ? totalRevenue / totalHours : 0,
                    Profitability = company.AverageProfitability,
                    Relationship = company.RelationshipStatus.GetDescription(),
                    PaymentAbility = company.PaymentAbilityStatus.GetDescription(),
                    LastOrderDate = companyOrders.Any() ? companyOrders.Max(o => (DateTime?)o.CreatedDate) : null
                };

                _companyRevenueSummaries.Add(summary);
            }

            ApplyStatisticsSort();
            CompanyRevenueView?.Refresh();
        }

        private DateTime? GetStatisticsStartDate()
        {
            var period = _selectedStatisticsPeriod?.Period ?? StatisticsPeriod.AllTime;
            return period switch
            {
                StatisticsPeriod.Last30Days => DateTime.Today.AddDays(-30),
                StatisticsPeriod.Last90Days => DateTime.Today.AddDays(-90),
                StatisticsPeriod.YearToDate => new DateTime(DateTime.Today.Year, 1, 1),
                _ => null
            };
        }

        private bool FilterCompanyRevenueSummary(object? obj)
        {
            if (obj is not CompanyRevenueSummary summary)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_statisticsSearchText) &&
                !summary.CompanyName.Contains(_statisticsSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var minOrders = (int)Math.Round(_statisticsMinOrders, MidpointRounding.AwayFromZero);
            if (minOrders > 0 && summary.OrdersCount < minOrders)
            {
                return false;
            }

            return true;
        }

        private void ApplyStatisticsSort()
        {
            if (CompanyRevenueView == null) return;

            using (CompanyRevenueView.DeferRefresh())
            {
                CompanyRevenueView.SortDescriptions.Clear();
                if (_selectedStatisticsSort != null)
                {
                    CompanyRevenueView.SortDescriptions.Add(new SortDescription(_selectedStatisticsSort.PropertyName, _selectedStatisticsSort.Direction));
                }
            }
        }
        private void UpdateAllStatistics()
        {
            OnPropertyChanged(nameof(TotalTasksCount));
            OnPropertyChanged(nameof(CompletedTasksCount));
            OnPropertyChanged(nameof(InProgressTasksCount));
            OnPropertyChanged(nameof(TaskCompletionPercentage));
            OnPropertyChanged(nameof(TotalOrdersCount));
            OnPropertyChanged(nameof(CompletedOrdersCount));
            OnPropertyChanged(nameof(InProgressOrdersCount));
            OnPropertyChanged(nameof(PaidOrdersCount));
            OnPropertyChanged(nameof(TotalCompaniesCount));
            OnPropertyChanged(nameof(ActiveCompaniesCount));
            OnPropertyChanged(nameof(AverageOrdersPerCompany));
            OnPropertyChanged(nameof(AverageOrderValue));
            OnPropertyChanged(nameof(AverageHoursPerTask));
            OnPropertyChanged(nameof(CurrentTaskName));
            OnPropertyChanged(nameof(TimerStatus));
            OnPropertyChanged(nameof(TodayHours));
            OnPropertyChanged(nameof(WeekHours));
            OnPropertyChanged(nameof(MonthlyHours));
            OnPropertyChanged(nameof(MonthlyIncome));
            OnPropertyChanged(nameof(ProfitabilityAnalysis));

            RefreshStatisticsDashboard();
        }
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
                    EnsureUniqueOrderNames(company);
                    Companies.Add(company);
                }
            }
            // Ð’Ð¾ÑÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ð¸ ÑÐ¾ÑÑ‚Ð¾ÑÐ½Ð¸Ðµ Ñ‚Ð°Ð¹Ð¼ÐµÑ€Ð°
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
                            
                            // Ð’Ð¾ÑÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ ÑÐ¾ÑÑ‚Ð¾ÑÐ½Ð¸Ðµ Ñ‚Ð°Ð¹Ð¼ÐµÑ€Ð°, ÐµÑÐ»Ð¸ Ð¾Ð½ Ð±Ñ‹Ð» Ð·Ð°Ð¿ÑƒÑ‰ÐµÐ½
                            if (task.IsTimerRunning && task.TimerStartTime.HasValue)
                            {
                                _timerStartTime = task.TimerStartTime;
                                _timer.Start();
                            }
                            break;
                        }
                    }
                    if (SelectedTask != null) break;
                }
            }
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
            OnPropertyChanged(nameof(TotalMoneyEarned));
            OnPropertyChanged(nameof(FilteredCompanies));
            OnPropertyChanged(nameof(FilteredOrders));
            OnPropertyChanged(nameof(AllTasks));
            UpdateAllStatistics();
        }

        #endregion
    }

    public record StatisticsPeriodOption(StatisticsPeriod Period, string DisplayName);

    public record StatisticsSortOption(string DisplayName, string PropertyName, ListSortDirection Direction);

}

