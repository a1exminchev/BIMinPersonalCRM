using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using BIMinPersonalCRM.Commands;
using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.MVVM;
using BIMinPersonalCRM.Services;
using BIMinPersonalCRM.ViewModels.Entities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BIMinPersonalCRM.ViewModels
{
    /// <summary>
    ///     Главная модель представления приложения. Управляет коллекциями компаний,
    ///     заказов и задач, а также реализацией команд и таймера.
    /// </summary>
    public class MainVM : VMObject
    {
        #region private fields
        private readonly IDataService _dataService;
        private readonly System.Timers.Timer _timer;
        private DateTime? _timerStartTime;
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

        /// <summary>
        /// Общее количество задач
        /// </summary>
        public int TotalTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count();

        /// <summary>
        /// Количество завершенных задач
        /// </summary>
        public int CompletedTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count(t => t.Status == Models.TaskStatus.Completed);

        /// <summary>
        /// Количество задач в работе
        /// </summary>
        public int InProgressTasksCount => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Count(t => t.Status == Models.TaskStatus.InProgress);

        /// <summary>
        /// Процент завершения задач
        /// </summary>
        public double TaskCompletionPercentage =>
            TotalTasksCount > 0 ? (double)CompletedTasksCount / TotalTasksCount * 100 : 0;

        /// <summary>
        /// Общее количество заказов
        /// </summary>
        public int TotalOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count();

        /// <summary>
        /// Количество завершенных заказов
        /// </summary>
        public int CompletedOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid);

        /// <summary>
        /// Количество заказов в работе
        /// </summary>
        public int InProgressOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.InProgress);

        /// <summary>
        /// Количество оплаченных заказов
        /// </summary>
        public int PaidOrdersCount => Companies
            .SelectMany(c => c.Orders)
            .Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid);

        /// <summary>
        /// Общее количество компаний
        /// </summary>
        public int TotalCompaniesCount => Companies.Count;

        /// <summary>
        /// Количество активных компаний (имеющих заказы)
        /// </summary>
        public int ActiveCompaniesCount => Companies.Count(c => c.Orders.Any());

        /// <summary>
        /// Среднее количество заказов на компанию
        /// </summary>
        public double AverageOrdersPerCompany =>
            TotalCompaniesCount > 0 ? (double)TotalOrdersCount / TotalCompaniesCount : 0;

        /// <summary>
        /// Средняя стоимость заказа
        /// </summary>
        public double AverageOrderValue =>
            TotalOrdersCount > 0 ? TotalMoneyEarned / TotalOrdersCount : 0;

        /// <summary>
        /// Среднее часов на задачу
        /// </summary>
        public double AverageHoursPerTask =>
            TotalTasksCount > 0 ? TotalHoursSpent / TotalTasksCount : 0;

        /// <summary>
        /// Название текущей задачи
        /// </summary>
        public string CurrentTaskName => SelectedTask?.Name ?? "Не выбрана";

        /// <summary>
        /// Статус таймера
        /// </summary>
        public string TimerStatus => IsTimerRunning ? "Запущен" : "Остановлен";

        /// <summary>
        /// Часы за сегодня
        /// </summary>
        public double TodayHours => Companies
            .SelectMany(c => c.Orders)
            .SelectMany(o => o.Tasks)
            .Where(t => t.StartDate.Date == DateTime.Today)
            .Sum(t => t.HoursSpent);

        /// <summary>
        /// Часы за текущую неделю
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
        /// Приблизительные часы в месяц (на основе недельной статистики)
        /// </summary>
        public double MonthlyHours => WeekHours * 4.33;

        /// <summary>
        /// Приблизительный доход в месяц
        /// </summary>
        public double MonthlyIncome => MonthlyHours * AverageHourlyRate;

        /// <summary>
        /// Анализ прибыльности по компаниям
        /// </summary>
        /// <summary>
        ///     Возвращает статистику прибыльности по компаниям.
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

        public ObservableCollection<ISeries> RevenueTrendSeries
        {
            get => GetValue<ObservableCollection<ISeries>>(nameof(RevenueTrendSeries)) ?? new();
            set => SetValue(nameof(RevenueTrendSeries), value);
        }

        public ObservableCollection<ISeries> OrdersByStatusSeries
        {
            get => GetValue<ObservableCollection<ISeries>>(nameof(OrdersByStatusSeries)) ?? new();
            set => SetValue(nameof(OrdersByStatusSeries), value);
        }

        public ObservableCollection<ISeries> TasksStatusSeries
        {
            get => GetValue<ObservableCollection<ISeries>>(nameof(TasksStatusSeries)) ?? new();
            set => SetValue(nameof(TasksStatusSeries), value);
        }

        public ObservableCollection<ISeries> RevenueByCompanySeries
        {
            get => GetValue<ObservableCollection<ISeries>>(nameof(RevenueByCompanySeries)) ?? new();
            set => SetValue(nameof(RevenueByCompanySeries), value);
        }

        public Axis[] RevenueTrendXAxis
        {
            get => GetValue<Axis[]>(nameof(RevenueTrendXAxis));
            set => SetValue(nameof(RevenueTrendXAxis), value);
        }

        public Axis[] RevenueTrendYAxis
        {
            get => GetValue<Axis[]>(nameof(RevenueTrendYAxis));
            set => SetValue(nameof(RevenueTrendYAxis), value);
        }

        public Axis[] OrdersByStatusXAxis
        {
            get => GetValue<Axis[]>(nameof(OrdersByStatusXAxis));
            set => SetValue(nameof(OrdersByStatusXAxis), value);
        }

        public Axis[] OrdersByStatusYAxis
        {
            get => GetValue<Axis[]>(nameof(OrdersByStatusYAxis));
            set => SetValue(nameof(OrdersByStatusYAxis), value);
        }

        public Axis[] TasksStatusXAxis
        {
            get => GetValue<Axis[]>(nameof(TasksStatusXAxis));
            set => SetValue(nameof(TasksStatusXAxis), value);
        }

        public Axis[] TasksStatusYAxis
        {
            get => GetValue<Axis[]>(nameof(TasksStatusYAxis));
            set => SetValue(nameof(TasksStatusYAxis), value);
        }

        public IReadOnlyList<StatisticsPeriodOption> StatisticsPeriods { get; }

        public IReadOnlyList<StatisticsSortOption> StatisticsSortOptions { get; }

        public StatisticsPeriodOption SelectedStatisticsPeriod
        {
            get => GetValue<StatisticsPeriodOption>(nameof(SelectedStatisticsPeriod));
            set
            {
                SetValue(nameof(SelectedStatisticsPeriod), value);
                RefreshStatisticsDashboard();
            }
        }

        public StatisticsSortOption SelectedStatisticsSort
        {
            get => GetValue<StatisticsSortOption>(nameof(SelectedStatisticsSort));
            set
            {
                SetValue(nameof(StatisticsSortOption), value);
                ApplyStatisticsSort();
            }
        }

        public string StatisticsSearchText
        {
            get => GetValue<string>(nameof(StatisticsSearchText));
            set
            {
                SetValue(nameof(StatisticsSearchText), value);
                CompanyRevenueView?.Refresh();
            }
        }

        public double StatisticsMinOrders
        {
            get => GetValue<double>(nameof(StatisticsMinOrders));
            set
            {
                if (Math.Abs(StatisticsMinOrders - value) > 0.0001)
                {
                    SetValue(nameof(StatisticsMinOrders), value);
                    CompanyRevenueView?.Refresh();
                }
            }
        }

        public ObservableCollection<CompanyRevenueSummary> CompanyRevenueSummaries
        {
            get => GetValue<ObservableCollection<CompanyRevenueSummary>>(nameof(CompanyRevenueSummaries)) ?? new();
            set => SetValue(nameof(CompanyRevenueSummaries), value);
        }

        public ICollectionView CompanyRevenueView { get; }

        #endregion

        #region Поиск и фильтрация

        /// <summary>
        /// Текст поиска компаний
        /// </summary>
        public string CompanySearchText
        {
            get => GetValue<string>(nameof(CompanySearchText));
            set
            {
                SetValue(nameof(CompanySearchText), value);
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        /// <summary>
        /// Текст поиска заказов
        /// </summary>
        public string OrderSearchText
        {
            get => GetValue<string>(nameof(OrderSearchText));
            set
            {
                SetValue(nameof(OrderSearchText), value);
                OnPropertyChanged(nameof(FilteredOrders));
            }
        }

        /// <summary>
        /// Фильтрованные компании
        /// </summary>
        public ObservableCollection<CompanyVM> FilteredCompanies
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

                if (SelectedRelationshipFilter.HasValue)
                {
                    filtered = filtered.Where(c => c.RelationshipStatus == SelectedRelationshipFilter.Value);
                }

                if (SelectedPaymentAbilityFilter.HasValue)
                {
                    filtered = filtered.Where(c => c.PaymentAbilityStatus == SelectedPaymentAbilityFilter.Value);
                }

                return new(filtered);
            }
        }

        /// <summary>
        /// Фильтрованные заказы
        /// </summary>
        public ObservableCollection<OrderVM> FilteredOrders
        {
            get
            {
                var allOrders = Companies.SelectMany(c => c.Orders);

                if (!string.IsNullOrWhiteSpace(OrderSearchText))
                {
                    allOrders = allOrders.Where(o => o.Name.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                     o.SoftwareType.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (SelectedCompanyFilter != null)
                {
                    allOrders = allOrders.Where(o => o.CompanyName == SelectedCompanyFilter.Name);
                }

                if (SelectedOrderStatusFilter.HasValue)
                {
                    allOrders = allOrders.Where(o => o.ExecutionStatus == SelectedOrderStatusFilter.Value);
                }

                return new(allOrders.OrderByDescending(o => o.CreatedDate));
            }
        }

        /// <summary>
        /// Все задачи из всех заказов
        /// </summary>
        public ObservableCollection<TaskVM> AllTasks
        {
            get
            {
                var allTasks = Companies.SelectMany(c => c.Orders).SelectMany(o => o.Tasks);
                return new(allTasks.OrderByDescending(t => t.StartDate));
            }
        }

        /// <summary>
        /// Фильтры для статусов отношений
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
        /// Фильтры для статусов платежеспособности
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
        /// Фильтры для статусов заказов
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
        /// Выбранный фильтр отношений
        /// </summary>
        public RelationshipStatus? SelectedRelationshipFilter
        {
            get => GetValue<RelationshipStatus?>(nameof(SelectedRelationshipFilter));
            set
            {
                SetValue(nameof(SelectedRelationshipFilter), value);
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        /// <summary>
        /// Выбранный фильтр платежеспособности
        /// </summary>
        public PaymentAbilityStatus? SelectedPaymentAbilityFilter
        {
            get => GetValue<PaymentAbilityStatus?>(nameof(SelectedPaymentAbilityFilter));
            set
            {
                SetValue(nameof(SelectedPaymentAbilityFilter), value);
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        /// <summary>
        /// Выбранный фильтр компании для заказов
        /// </summary>
        public CompanyVM? SelectedCompanyFilter
        {
            get => GetValue<CompanyVM?>(nameof(SelectedCompanyFilter));
            set
            {
                SetValue(nameof(SelectedCompanyFilter), value);
                OnPropertyChanged(nameof(FilteredOrders));
            }
        }

        /// <summary>
        /// Выбранный фильтр статуса заказа
        /// </summary>
        public OrderExecutionStatus? SelectedOrderStatusFilter
        {
            get => GetValue<OrderExecutionStatus?>(nameof(SelectedOrderStatusFilter));
            set
            {
                SetValue(nameof(SelectedOrderStatusFilter), value);
                OnPropertyChanged(nameof(FilteredOrders));
            }
        }

        #endregion

        #endregion

        #region Коллекции и выбранные элементы

        /// <summary>
        ///     Компании, отображаемые в UI.
        /// </summary>
        public ObservableCollection<CompanyVM> Companies { get; }

        /// <summary>
        ///     Текущая выбранная компания.
        /// </summary>
        public CompanyVM SelectedCompany
        {
            get => GetValue<CompanyVM>(nameof(SelectedCompany));
            set
            {
                SetValue(nameof(SelectedCompany), value);
                // Сбросить выбранный заказ и задачу при смене компании
                SelectedOrder = null;
                SelectedTask = null;
                SelectedEmployee = null;
            }
        }

        /// <summary>
        ///     Заказы выбранной компании. Возвращает пустой список, если компания не выбрана.
        /// </summary>
        public ObservableCollection<OrderVM> Orders
        {
            get
            {
                if (SelectedCompany != null)
                {
                    return new(SelectedCompany.Orders);
                }
                return new();
            }
        }

        /// <summary>
        ///     Текущий выбранный заказ.
        /// </summary>
        public OrderVM SelectedOrder
        {
            get => GetValue<OrderVM>(nameof(SelectedOrder));
            set
            {
                SetValue(nameof(SelectedOrder), value);
                // При смене заказа сбросить выбранную задачу
                SelectedTask = null;
                SelectedFile = null;
            }
        }

        /// <summary>
        ///     Задачи выбранного заказа. Пустой список, если заказ не выбран.
        /// </summary>
        public ObservableCollection<TaskVM> Tasks
        {
            get
            {
                if (SelectedOrder != null)
                {
                    return new(SelectedOrder.Tasks);
                }
                return new();
            }
        }

        /// <summary>
        ///     Текущая выбранная задача.
        /// </summary>
        public TaskVM SelectedTask
        {
            get => GetValue<TaskVM>(nameof(SelectedTask));
            set => SetValue(nameof(SelectedTask), value);
        }

        /// <summary>
        ///     Возвращает признак, что таймер запущен.
        /// </summary>
        public bool IsTimerRunning => _timer.Enabled && SelectedTask?.IsTimerRunning == true;

        /// <summary>
        /// Выбранный сотрудник
        /// </summary>
        public EmployeeVM SelectedEmployee
        {
            get => GetValue<EmployeeVM>(nameof(SelectedEmployee));
            set => SetValue(nameof(SelectedEmployee), value);
        }

        /// <summary>
        /// Выбранный файл
        /// </summary>
        public FileAttachmentVM SelectedFile
        {
            get => GetValue<FileAttachmentVM>(nameof(SelectedFile));
            set => SetValue(nameof(SelectedFile), value);
        }

        #endregion

        #region Команды

        public DelegateCommand AddCompanyCommand { get; }
        public DelegateCommand AddOrderCommand { get; }
        public DelegateCommand<OrderVM> RemoveOrderCommand { get; }
        public DelegateCommand AddTaskCommand { get; }
        public DelegateCommand AddEmployeeCommand { get; }
        public DelegateCommand<EmployeeVM> RemoveEmployeeCommand { get; }
        public DelegateCommand AddFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> RemoveFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> OpenFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> RepathFileCommand { get; }
        public DelegateCommand ToggleThemeCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand StartTimerCommand { get; }
        public DelegateCommand StopTimerCommand { get; }
        public DelegateCommand DeleteCompanyCommand { get; }
        public DelegateCommand SelectLogoCommand { get; }
        public DelegateCommand RemoveLogoCommand { get; }
        public DelegateCommand<EmployeeVM> SelectEmployeeAvatarCommand { get; }
        public DelegateCommand<EmployeeVM> RemoveEmployeeAvatarCommand { get; }

        #endregion

        /// <summary>
        ///     Инициализирует новую модель представления и загружает данные.
        /// </summary>
        public MainVM()
        {
            _dataService = new JsonDataService(AppSettings.DataFile);
            if (!System.IO.Directory.Exists(AppSettings.CompanyLogosFolder))
            {
                System.IO.Directory.CreateDirectory(AppSettings.CompanyLogosFolder);
            }
            if (!System.IO.Directory.Exists(AppSettings.EmployeeAvatarsFolder))
            {
                System.IO.Directory.CreateDirectory(AppSettings.EmployeeAvatarsFolder);
            }

            Companies = new();

            IsLightTheme = ThemeManager.IsLightTheme;
            ThemeManager.ThemeChanged += ThemeManagerOnThemeChanged;

            // Инициализация команд
            AddCompanyCommand = new(AddCompany);
            AddOrderCommand = new(AddOrder, _ => SelectedCompany != null);
            RemoveOrderCommand = new(RemoveOrder, parameter => SelectedCompany != null && parameter is OrderVM);
            AddTaskCommand = new(AddTask, _ => SelectedOrder != null);
            AddEmployeeCommand = new(AddEmployee, _ => SelectedCompany != null);
            RemoveEmployeeCommand = new(RemoveEmployee, parameter => SelectedCompany != null && parameter is EmployeeVM);
            AddFileCommand = new(AddFile, _ => SelectedOrder != null);
            RemoveFileCommand = new(RemoveFile, parameter => SelectedOrder != null && parameter is FileAttachmentVM);
            OpenFileCommand = new(OpenFile, parameter => parameter is FileAttachmentVM f && System.IO.File.Exists(f.FilePath));
            RepathFileCommand = new(RepathFile, parameter => SelectedOrder != null && parameter is FileAttachmentVM);
            DeleteCompanyCommand = new(DeleteCompany, _ => SelectedCompany != null);
            SelectLogoCommand = new(SelectLogo, _ => SelectedCompany != null);
            RemoveLogoCommand = new(RemoveLogo, _ => SelectedCompany != null && SelectedCompany.LogoPath != "");
            SelectEmployeeAvatarCommand = new(SelectEmployeeAvatar, _ => true);
            RemoveEmployeeAvatarCommand = new(RemoveEmployeeAvatar, _ => true);
            ToggleThemeCommand = new(ToggleTheme);
            SaveCommand = new(async () => await SaveAsync());
            LoadCommand = new (async () => await LoadAsync());
            StartTimerCommand = new(StartTimer, _ => SelectedTask != null && !IsTimerRunning);
            StopTimerCommand = new(StopTimer, _ => SelectedTask != null && IsTimerRunning);

            // Таймер с секундной периодичностью, обновляет часы раз в секунду.
            StatisticsPeriods = new List<StatisticsPeriodOption>
            {
                new(StatisticsPeriod.Last30Days, "Последние 30 дней"),
                new(StatisticsPeriod.Last90Days, "Последние 90 дней"),
                new(StatisticsPeriod.YearToDate, "С начала года"),
                new(StatisticsPeriod.AllTime, "За все время")
            };
            SelectedStatisticsPeriod = StatisticsPeriods[0];

            StatisticsSortOptions = new List<StatisticsSortOption>
            {
                new("По выручке (убывание)", nameof(CompanyRevenueSummary.TotalRevenue), ListSortDirection.Descending),
                new("По средней ставке (убывание)", nameof(CompanyRevenueSummary.AverageRate), ListSortDirection.Descending),
                new("По количеству заказов", nameof(CompanyRevenueSummary.OrdersCount), ListSortDirection.Descending),
                new("По дате последнего заказа", nameof(CompanyRevenueSummary.LastOrderDate), ListSortDirection.Descending)
            };
            SelectedStatisticsSort = StatisticsSortOptions[0];

            CompanyRevenueView = CollectionViewSource.GetDefaultView(CompanyRevenueSummaries);
            CompanyRevenueView.Filter = FilterCompanyRevenueSummary;

            ApplyStatisticsSort();
            RefreshStatisticsDashboard();
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (_, _) => OnTimerTick();

            // Загрузка данных при запуске
            _ = LoadAsync();
        }

        #region Имплементация команд

        private void AddCompany()
        {
            var dialog = new Views.Dialogs.AddCompanyDialog();
            if (dialog.ShowDialog() == true)
            {
                var company = dialog.Company;
                
                // Проверяем уникальность имени
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

            var order = new OrderVM
            {
                Id = GetNextOrderId(),
                Name = GenerateUniqueOrderName(SelectedCompany, "Новый заказ"),
                Price = 0,
                SoftwareType = "Revit",
                ExpectedDurationDays = 10,
                CompanyName = SelectedCompany.Name
            };

            SelectedCompany.Orders.Add(order);
            order.UpdateProfitabilityStatus();

            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(FilteredOrders));

            SelectedOrder = order;
            UpdateAllStatistics();
        }

        private void RemoveOrder(OrderVM order)
        {
            if (SelectedCompany == null || order == null) return;

            if (!SelectedCompany.Orders.Remove(order)) return;

            if (ReferenceEquals(SelectedOrder, order))
            {
                SelectedOrder = null;
            }

            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(FilteredOrders));
            UpdateAllStatistics();
        }

        private void AddTask()
        {
            if (SelectedOrder == null) return;
            var task = new TaskVM
            {
                Id = GetNextTaskId(),
                Name = "Новая задача",
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
            
            var baseName = "Новый сотрудник";
            var name = baseName;
            var counter = 1;
            
            // Проверяем уникальность имени среди всех сотрудников
            var allEmployees = Companies.SelectMany(c => c.Employees).ToList();
            while (allEmployees.Any(e => e.FullName == name))
            {
                name = $"{baseName} ({counter})";
                counter++;
            }
            
            var employee = new EmployeeVM
            {
                Id = GetNextEmployeeId(),
                FullName = name,
                Position = "Должность"
            };
            SelectedCompany.Employees.Add(employee);
            SelectedEmployee = employee;
        }

        private void RemoveEmployee(EmployeeVM employee)
        {
            if (SelectedCompany == null || employee == null) return;

            if (SelectedCompany.Employees.Remove(employee) && ReferenceEquals(SelectedEmployee, employee))
            {
                SelectedEmployee = null;
            }
        }

        private void AddFile()
        {
            if (SelectedOrder == null) return;
            
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите файл для прикрепления",
                Filter = "Все файлы (*.*)|*.*|Документы (*.pdf;*.doc;*.docx)|*.pdf;*.doc;*.docx|Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new System.IO.FileInfo(openFileDialog.FileName);
                var fileAttachment = new FileAttachmentVM
                {
                    Id = GetNextFileId(),
                    Name = fileInfo.Name,
                    FilePath = openFileDialog.FileName,
                    FileSize = fileInfo.Length,
                    FileType = DetermineFileType(fileInfo.Extension)
                };
                
                SelectedOrder.AttachedFiles.Add(fileAttachment);
            }
        }

        private void RemoveFile(FileAttachmentVM file)
        {
            if (SelectedOrder == null || file == null) return;

            if (SelectedOrder.AttachedFiles.Remove(file) && ReferenceEquals(SelectedFile, file))
            {
                SelectedFile = null;
            }
        }

        private void OpenFile(FileAttachmentVM file)
        {
            if (file == null) return;
            try
            {
                if (!System.IO.File.Exists(file.FilePath)) return;
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                // ignore
            }
        }

        private void RepathFile(FileAttachmentVM file)
        {
            if (SelectedOrder == null || file == null) return;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Указать путь к файлу",
                Filter = "Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var fi = new System.IO.FileInfo(openFileDialog.FileName);
                file.FilePath = fi.FullName;
                file.Name = fi.Name;
                file.FileSize = fi.Length;
                file.FileType = DetermineFileType(fi.Extension);
            }
        }

        private void DeleteCompany()
        {
            if (SelectedCompany == null) return;
            
            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить компанию '{SelectedCompany.Name}'? Все связанные заказы и задачи также будут удалены.",
                "Подтверждение удаления",
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
                Title = "Выберите логотип компании",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var ext = System.IO.Path.GetExtension(dialog.FileName);
                    var unique = System.Guid.NewGuid().ToString("N") + ext;
                    var destPath = System.IO.Path.Combine(AppSettings.CompanyLogosFolder, unique);
                    System.IO.File.Copy(dialog.FileName, destPath, true);
                    SelectedCompany.LogoPath = destPath;
                }
                catch { }
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        private void RemoveLogo()
        {
            if (SelectedCompany == null) return;
            SelectedCompany.LogoPath = "";
            OnPropertyChanged(nameof(SelectedCompany));
        }

        private void SelectEmployeeAvatar(EmployeeVM employee)
        {
            if (employee == null) return;

            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выбрать фото сотрудника",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Все файлы (*.*)|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var ext = System.IO.Path.GetExtension(ofd.FileName);
                    var unique = System.Guid.NewGuid().ToString("N") + ext;
                    var destPath = System.IO.Path.Combine(AppSettings.EmployeeAvatarsFolder, unique);
                    System.IO.File.Copy(ofd.FileName, destPath, true);
                    employee.AvatarPath = destPath;
                }
                catch { }
            }
        }

        private void RemoveEmployeeAvatar(EmployeeVM employee)
        {
            if (employee == null) return;
            employee.AvatarPath = string.Empty;
        }

        private void ThemeManagerOnThemeChanged(object? sender, EventArgs e)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                IsLightTheme = ThemeManager.IsLightTheme;
            });
        }

        private void ToggleTheme()
        {
            ThemeManager.ToggleTheme();
        }

        private async Task SaveAsync()
        {
            var data = new DataStoreDto
            {
                Companies = Companies.Select(c => c.ToDto()).ToList(),
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
                foreach (var companyDto in data.Companies)
                {
                    var company = new CompanyVM(companyDto);
                    EnsureUniqueOrderNames(company);
                    Companies.Add(company);
                }
            }
            // Восстановить выбранную задачу и состояние таймера
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

                            // Восстановить состояние таймера, если он был запущен
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

        private void StartTimer()
        {
            if (SelectedTask == null)
                return;

            // Остановить таймер для всех других задач
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
            // Конвертировать в часы с точностью до двух знаков.
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
        #endregion

        #region Вспомогательные методы
        private void OnTimerTick()
        {
            // Пока ничего не делаем каждую секунду, можно добавить индикацию.
        }

        /// <summary>
        /// Генерирует следующий доступный ID для компании
        /// </summary>
        private int GetNextCompanyId()
        {
            return Companies.Any() ? Companies.Max(c => c.Id) + 1 : 1;
        }

        /// <summary>
        /// Генерирует следующий доступный ID для заказа
        /// </summary>
        private int GetNextOrderId()
        {
            var allOrders = Companies.SelectMany(c => c.Orders);
            return allOrders.Any() ? allOrders.Max(o => o.Id) + 1 : 1;
        }

        /// <summary>
        /// Генерирует следующий доступный ID для задачи
        /// </summary>
        private int GetNextTaskId()
        {
            var allTasks = Companies.SelectMany(c => c.Orders).SelectMany(o => o.Tasks);
            return allTasks.Any() ? allTasks.Max(t => t.Id) + 1 : 1;
        }

        /// <summary>
        /// Генерирует следующий доступный ID для сотрудника
        /// </summary>
        private int GetNextEmployeeId()
        {
            var allEmployees = Companies.SelectMany(c => c.Employees);
            return allEmployees.Any() ? allEmployees.Max(e => e.Id) + 1 : 1;
        }

        /// <summary>
        /// Генерирует следующий доступный ID для файла
        /// </summary>
        private int GetNextFileId()
        {
            var allFiles = Companies.SelectMany(c => c.Orders).SelectMany(o => o.AttachedFiles);
            return allFiles.Any() ? allFiles.Max(f => f.Id) + 1 : 1;
        }

        /// <summary>
        /// Определяет тип файла по расширению
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

        public bool IsLightTheme
        {
            get => GetValue<bool>(nameof(IsLightTheme));
            private set => SetValue(nameof(IsLightTheme), value);
        }


        private string GenerateUniqueOrderName(CompanyVM company, string? proposedName, OrderVM? ignoreOrder = null)

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



        public void EnsureUniqueOrderName(OrderVM order)

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



        private void EnsureUniqueOrderNames(CompanyVM company)

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
        /// Обновляет все статистические свойства
        /// </summary>


        #endregion

        #region Статистика
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

            RevenueTrendSeries?.Clear();

            if (monthlyRevenue.Any())
            {
                RevenueTrendSeries.Add(new LineSeries<double>
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

            RevenueTrendXAxis = new[]
            {
                new Axis
                {
                    Labels = monthlyRevenue.Select(m => m.Month.ToString("MMM yyyy", culture)).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            RevenueTrendYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };

            var statusData = orders
                .GroupBy(x => x.order.ExecutionStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderBy(g => g.Status)
                .ToList();

            OrdersByStatusSeries?.Clear();

            if (statusData.Any())
            {
                OrdersByStatusSeries.Add(new ColumnSeries<int>
                {
                    Values = statusData.Select(s => s.Count).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(30, 136, 229, 180)),
                    Stroke = new SolidColorPaint(new SKColor(30, 136, 229)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture)
                });
            }

            OrdersByStatusXAxis = new[]
            {
                new Axis
                {
                    Labels = statusData.Select(s => s.Status.GetDescription()).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            OrdersByStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };

            IEnumerable<TaskVM> tasksQuery = Companies.SelectMany(company => company.Orders).SelectMany(order => order.Tasks);
            if (startDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(task => task.StartDate >= startDate.Value);
            }

            var tasksData = tasksQuery
                .GroupBy(task => task.Status)
                .Select(g => new { Status = g.Key, Hours = g.Sum(t => t.HoursSpent) })
                .OrderBy(g => g.Status)
                .ToList();

            TasksStatusSeries?.Clear();

            if (tasksData.Any())
            {
                TasksStatusSeries.Add(new ColumnSeries<double>
                {
                    Values = tasksData.Select(t => t.Hours).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(76, 175, 80, 180)),
                    Stroke = new SolidColorPaint(new SKColor(56, 142, 60)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N1", culture)
                });
            }

            TasksStatusXAxis = new[]
            {
                new Axis
                {
                    Labels = tasksData.Select(t => t.Status.GetDescription()).ToArray(),
                    LabelsRotation = 15,
                    TextSize = 12
                }
            };
            TasksStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N1", culture),
                    TextSize = 12
                }
            };

            RevenueByCompanySeries?.Clear();

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
                    RevenueByCompanySeries.Add(new PieSeries<double>
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
                    RevenueByCompanySeries.Add(new PieSeries<double>
                    {
                        Values = new[] { 1d },
                        Name = "Нет данных",
                        DataLabelsPaint = new SolidColorPaint(new SKColor(33, 33, 33)),
                        DataLabelsSize = 12,
                        DataLabelsFormatter = _ => string.Empty
                    });
                }
            }

            CompanyRevenueSummaries?.Clear();

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

                CompanyRevenueSummaries.Add(summary);
            }

            ApplyStatisticsSort();
            CompanyRevenueView?.Refresh();
        }

        private DateTime? GetStatisticsStartDate()
        {
            var period = SelectedStatisticsPeriod?.Period ?? StatisticsPeriod.AllTime;
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

            if (!string.IsNullOrWhiteSpace(StatisticsSearchText) &&
                !summary.CompanyName.Contains(StatisticsSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var minOrders = (int)Math.Round(StatisticsMinOrders, MidpointRounding.AwayFromZero);
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
                if (SelectedStatisticsSort != null)
                {
                    CompanyRevenueView.SortDescriptions.Add(new SortDescription(SelectedStatisticsSort.PropertyName, SelectedStatisticsSort.Direction));
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
        #endregion
    }

    public record StatisticsPeriodOption(StatisticsPeriod Period, string DisplayName);

    public record StatisticsSortOption(string DisplayName, string PropertyName, ListSortDirection Direction);
}












