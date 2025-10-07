using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private readonly IStatisticsService _statisticsService;
        private readonly System.Timers.Timer _timer;
        private DateTime? _timerStartTime;
        #endregion

        #region Вычисляемые публичные свойства

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
        public string TimerStatus
        {
            get => GetValue<string>(nameof(TimerStatus));
            set => SetValue(nameof(TimerStatus), value);
        }

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
            get
            {
                var value = GetValue<ObservableCollection<ISeries>>(nameof(RevenueTrendSeries));
                if (value == null)
                {
                    value = new ObservableCollection<ISeries>();
                    SetValue(nameof(RevenueTrendSeries), value);
                }

                return value;
            }
            set => SetValue(nameof(RevenueTrendSeries), value);
        }

        public ObservableCollection<ISeries> OrdersByStatusSeries
        {
            get
            {
                var value = GetValue<ObservableCollection<ISeries>>(nameof(OrdersByStatusSeries));
                if (value == null)
                {
                    value = new ObservableCollection<ISeries>();
                    SetValue(nameof(OrdersByStatusSeries), value);
                }

                return value;
            }
            set => SetValue(nameof(OrdersByStatusSeries), value);
        }

        public ObservableCollection<ISeries> TasksStatusSeries
        {
            get
            {
                var value = GetValue<ObservableCollection<ISeries>>(nameof(TasksStatusSeries));
                if (value == null)
                {
                    value = new ObservableCollection<ISeries>();
                    SetValue(nameof(TasksStatusSeries), value);
                }

                return value;
            }
            set => SetValue(nameof(TasksStatusSeries), value);
        }

        public ObservableCollection<ISeries> RevenueByCompanySeries
        {
            get
            {
                var value = GetValue<ObservableCollection<ISeries>>(nameof(RevenueByCompanySeries));
                if (value == null)
                {
                    value = new ObservableCollection<ISeries>();
                    SetValue(nameof(RevenueByCompanySeries), value);
                }

                return value;
            }
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
            get
            {
                var value = GetValue<ObservableCollection<CompanyRevenueSummary>>(nameof(CompanyRevenueSummaries));
                if (value == null)
                {
                    value = new ObservableCollection<CompanyRevenueSummary>();
                    SetValue(nameof(CompanyRevenueSummaries), value);
                }

                return value;
            }
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
        /// Текст поиска задач
        /// </summary>
        public string TaskSearchText
        {
            get => GetValue<string>(nameof(TaskSearchText));
            set
            {
                SetValue(nameof(TaskSearchText), value);
                OnPropertyChanged(nameof(FilteredTasks));
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
                                                     o.SoftwareType.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                     o.Comments.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                     o.CompanyName.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase));
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
        /// Фильтрованные задачи
        /// </summary>
        public ObservableCollection<TaskVM> FilteredTasks
        {
            get
            {
                var allTasks = Companies.SelectMany(c => c.Orders).SelectMany(o => o.Tasks);

                if (!string.IsNullOrWhiteSpace(TaskSearchText))
                {
                    allTasks = allTasks.Where(t => t.Name.Contains(TaskSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                   t.CompanyName.Contains(TaskSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                   t.Description.Contains(TaskSearchText, StringComparison.OrdinalIgnoreCase) ||
                                                   t.OrderName.Contains(TaskSearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (SelectedCompanyFilterForTasks != null)
                {
                    allTasks = allTasks.Where(o => o.CompanyName == SelectedCompanyFilterForTasks.Name);
                }

                if (SelectedTaskStatusFilter.HasValue)
                {
                    allTasks = allTasks.Where(t => t.Status == SelectedTaskStatusFilter.Value);
                }

                return new(allTasks.OrderByDescending(o => o.StartDate));
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
        /// Фильтры для статусов задач
        /// </summary>
        public List<Models.TaskStatus?> TaskStatusFilter
        {
            get
            {
                var list = new List<Models.TaskStatus?> { null };
                list.AddRange(Enum.GetValues<Models.TaskStatus>().Cast<Models.TaskStatus?>());
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
        /// Выбранный фильтр компании для задач
        /// </summary>
        public CompanyVM? SelectedCompanyFilterForTasks
        {
            get => GetValue<CompanyVM?>(nameof(SelectedCompanyFilterForTasks));
            set
            {
                SetValue(nameof(SelectedCompanyFilterForTasks), value);
                OnPropertyChanged(nameof(FilteredTasks));
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

        /// <summary>
        /// Выбранный фильтр статуса задачи
        /// </summary>
        public Models.TaskStatus? SelectedTaskStatusFilter
        {
            get => GetValue<Models.TaskStatus?>(nameof(SelectedTaskStatusFilter));
            set
            {
                SetValue(nameof(SelectedTaskStatusFilter), value);
                OnPropertyChanged(nameof(FilteredTasks));
            }
        }
        #endregion

        #endregion

        #region Публичные свойства

        /// <summary>
        ///     Текущий выбранный индекс вкладки.
        /// </summary>
        public int SelectedTabIndex
        {
            get => GetValue<int>(nameof(SelectedTabIndex));
            set => SetValue(nameof(SelectedTabIndex), value);
        }

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
        ///     Выбранная задача.
        /// </summary>
        public TaskVM SelectedTask
        {
            get => GetValue<TaskVM>(nameof(SelectedTask));
            set => SetValue(nameof(SelectedTask), value);
        }

        /// <summary>
        ///     Возвращает признак, что таймер запущен.
        /// </summary>
        //public bool IsTimerRunning => _timer.Enabled && SelectedTask?.IsTimerRunning == true;

        public bool IsTimerRunning
        {
            get => GetValue<bool>(nameof(IsTimerRunning));
            set
            {
                SetValue(nameof(IsTimerRunning), value);
                if (!value)
                {
                    TimerStatus = "Остановлен";
                    CurrentTimerElapsed = TimeSpan.Zero;
                    OnPropertyChanged(nameof(CurrentTaskRunningHours));
                    OnPropertyChanged(nameof(CurrentTaskRunningTimeDisplay));
                }
                UpdateTimerCommands();
            }
        }

        public TimeSpan CurrentTimerElapsed
        {
            get => GetValue<TimeSpan>(nameof(CurrentTimerElapsed));
            private set => SetValue(nameof(CurrentTimerElapsed), value);
        }

        public double CurrentTaskRunningHours
        {
            get
            {
                if (CurrentTask == null)
                {
                    return 0;
                }

                var hours = CurrentTask.HoursSpent;
                if (IsTimerRunning && _timerStartTime.HasValue)
                {
                    hours += (DateTime.Now - _timerStartTime.Value).TotalHours;
                }

                return hours;
            }
        }

        /// <summary>
        ///     Форматированное отображение времени работы над текущей задачей.
        /// </summary>
        public string CurrentTaskRunningTimeDisplay
        {
            get
            {
                var time = TimeSpan.FromHours(CurrentTaskRunningHours);
                return $"{(int)time.TotalHours}ч {time.Minutes:D2}мин";
            }
        }

        /// <summary>
        /// ????????? ?????????
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

        /// <summary>
        ///     Текущая задача.
        /// </summary>

        public TaskVM CurrentTask
        {
            get => GetValue<TaskVM>(nameof(CurrentTask));
            set
            {
                if (ReferenceEquals(CurrentTask, value))
                {
                    return;
                }

                SetValue(nameof(CurrentTask), value);
                OnPropertyChanged(nameof(CurrentTaskCollection));
                SyncTimerStateFromCurrentTask();
            }
        }

        /// <summary>
        ///     Текущие задачи из одной задачи для DataGrid ItemsSource.
        /// </summary> 
        public ObservableCollection<TaskVM> CurrentTaskCollection => CurrentTask == null ? new() : new () { CurrentTask };
        #endregion

        #region Команды

        public DelegateCommand AddCompanyCommand { get; }
        public DelegateCommand AddOrderCommand { get; }
        public DelegateCommand<OrderVM> RemoveOrderCommand { get; }
        public DelegateCommand AddTaskCommand { get; }
        public DelegateCommand<TaskVM> RemoveTaskCommand { get; }
        public DelegateCommand AddEmployeeCommand { get; }
        public DelegateCommand<EmployeeVM> RemoveEmployeeCommand { get; }
        public DelegateCommand AddFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> RemoveFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> OpenFileCommand { get; }
        public DelegateCommand<FileAttachmentVM> RepathFileCommand { get; }
        public DelegateCommand ToggleThemeCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand LoadCommand { get; }
        public DelegateCommand<OrderVM> ShowOrderFilesCommand { get; }
        public DelegateCommand<TaskVM> ShowTaskOriginCommand { get; }
        public DelegateCommand StartTimerCommand { get; }
        public DelegateCommand StopTimerCommand { get; }
        public DelegateCommand<TaskVM> SetCurrentTaskCommand { get; }
        public DelegateCommand RemoveCurrentTaskCommand { get; }
        public DelegateCommand DeleteCompanyCommand { get; }
        public DelegateCommand SelectLogoCommand { get; }
        public DelegateCommand RemoveLogoCommand { get; }
        public DelegateCommand<EmployeeVM> SelectEmployeeAvatarCommand { get; }
        public DelegateCommand<EmployeeVM> RemoveEmployeeAvatarCommand { get; }
        public DelegateCommand RefreshStatisticsCommand { get; }

        #endregion

        /// <summary>
        ///     Инициализирует новую модель представления и загружает данные.
        /// </summary>
        public MainVM()
        {
            _dataService = new JsonDataService(AppSettings.DataFile);
            _statisticsService = new StatisticsService();
            if (!System.IO.Directory.Exists(AppSettings.CompanyLogosFolder))
            {
                System.IO.Directory.CreateDirectory(AppSettings.CompanyLogosFolder);
            }
            if (!System.IO.Directory.Exists(AppSettings.EmployeeAvatarsFolder))
            {
                System.IO.Directory.CreateDirectory(AppSettings.EmployeeAvatarsFolder);
            }

            Companies = new();
            TimerStatus = "Остановлен";
            CurrentTimerElapsed = TimeSpan.Zero;

            if (Application.Current != null)
            {
                Application.Current.Exit += OnApplicationExit;
            }

            SelectedOrderStatusFilter = null;

            IsLightTheme = ThemeManager.IsLightTheme;
            ThemeManager.ThemeChanged += ThemeManagerOnThemeChanged;

            

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

            // Инициализация команд
            AddCompanyCommand = new(AddCompany);
            AddOrderCommand = new(AddOrder, _ => SelectedCompany != null);
            RemoveOrderCommand = new(RemoveOrder, parameter => SelectedCompany != null && parameter is OrderVM);
            ShowOrderFilesCommand = new(ShowOrderFiles, parameter => parameter is OrderVM);
            ShowTaskOriginCommand = new(ShowTaskOrigin, parameter => parameter is TaskVM);
            AddTaskCommand = new(AddTask, _ => SelectedOrder != null);
            RemoveTaskCommand = new(RemoveTask, parameter => SelectedOrder != null && parameter is TaskVM);
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
            LoadCommand = new(async () => await LoadAsync());
            StartTimerCommand = new(StartTimer, _ => CurrentTask != null && !IsTimerRunning);
            StopTimerCommand = new(StopTimer, _ => CurrentTask != null && IsTimerRunning);
            SetCurrentTaskCommand = new(SetCurrentTask);
            RemoveCurrentTaskCommand = new(RemoveCurrentTask);
            RefreshStatisticsCommand = new(RefreshStatistics);

            UpdateTimerCommands();
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
                ExpectedDurationHours = 10,
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

        private void ShowOrderFiles(OrderVM order)
        {
            if (order == null) return;

            var company = Companies.FirstOrDefault(c => c.Orders.Contains(order) || c.Orders.Any(o => o.Id == order.Id));
            if (company == null) return;

            var targetOrder = company.Orders.FirstOrDefault(o => ReferenceEquals(o, order) || o.Id == order.Id);
            if (targetOrder == null) return;

            if (!ReferenceEquals(SelectedCompany, company))
            {
                SelectedCompany = company;
            }

            SelectedOrder = targetOrder;
            SelectedTabIndex = 0;
        }

        private void ShowTaskOrigin(TaskVM task)
        {
            if (task == null) return;

            var targetCompany = Companies.FirstOrDefault(c => c.Orders.FirstOrDefault(x => x.Tasks.FirstOrDefault(t => t.Id == task.Id) != null) != null);
            if (targetCompany == null) return;

            var targetOrder = targetCompany.Orders.FirstOrDefault(x => x.Tasks.FirstOrDefault(t => t.Id == task.Id) != null);
            if (targetOrder == null) return;

            if (!ReferenceEquals(SelectedCompany, targetCompany))
            {
                SelectedCompany = targetCompany;
            }

            SelectedOrder = targetOrder;
            SelectedTask = task;
            SelectedTabIndex = 0;
        }

        private void AddTask()
        {
            var task = new TaskVM
            {
                Id = GetNextTaskId(),
                Name = "Новая задача",
                StartDate = DateTime.Today,
                CompanyName = SelectedCompany.Name,
                OrderName = SelectedOrder.Name
            };
            SelectedOrder.Tasks.Add(task);
            AllTasks.Add(task);
            SelectedTask = task;
            //OnPropertyChanged(nameof(TotalHoursSpent));
            //OnPropertyChanged(nameof(AverageHourlyRate));
            //UpdateAllStatistics();
        }

        private void RemoveTask(TaskVM task)
        {
            if (SelectedTask == null || task == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить задачу '{task.Name}'?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (SelectedOrder.Tasks.Remove(task) && ReferenceEquals(SelectedTask, task))
                {
                    SelectedTask = null;
                }
            }
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

            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить сотрудника '{employee.FullName}'?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (SelectedCompany.Employees.Remove(employee) && ReferenceEquals(SelectedEmployee, employee))
                {
                    SelectedEmployee = null;
                }
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

            var result = System.Windows.MessageBox.Show(
                $"Вы действительно хотите удалить файл '{SelectedFile.Name}'?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (SelectedOrder.AttachedFiles.Remove(file) && ReferenceEquals(SelectedFile, file))
                {
                    SelectedFile = null;
                }
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
                CurrentTaskId = CurrentTask?.Id
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
                            CurrentTask = task;
                            break;
                        }
                    }

                    if (CurrentTask != null)
                    {
                        break;
                    }
                }
            }
            else
            {
                CurrentTask = null;
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
            if (CurrentTask == null)
                return;

            foreach (var company in Companies)
            {
                foreach (var order in company.Orders)
                {
                    foreach (var task in order.Tasks)
                    {
                        if (task != CurrentTask && task.IsTimerRunning)
                        {
                            task.IsTimerRunning = false;
                            task.TimerStartTime = null;
                        }
                    }
                }
            }

            _timerStartTime = DateTime.Now;
            CurrentTask.TimerStartTime = _timerStartTime;
            CurrentTask.IsTimerRunning = true;
            CurrentTimerElapsed = TimeSpan.Zero;
            _timer.Start();
            IsTimerRunning = true;
            UpdateRunningTimerVisuals(TimeSpan.Zero);
            ScheduleSave();
        }


        private void StopTimer()
        {
            if (CurrentTask == null || !_timerStartTime.HasValue)
                return;

            _timer.Stop();
            var elapsed = DateTime.Now - _timerStartTime.Value;
            CurrentTask.HoursSpent += Math.Round(elapsed.TotalHours, 2);
            CurrentTask.IsTimerRunning = false;
            CurrentTask.TimerStartTime = null;
            _timerStartTime = null;
            OnPropertyChanged(nameof(Tasks));
            OnPropertyChanged(nameof(TotalHoursSpent));
            OnPropertyChanged(nameof(AverageHourlyRate));
            SelectedOrder?.UpdateProfitabilityStatus();
            OnPropertyChanged(nameof(Orders));
            OnPropertyChanged(nameof(SelectedOrder));
            OnPropertyChanged(nameof(Companies));
            OnPropertyChanged(nameof(TotalMoneyEarned));
            IsTimerRunning = false;
            UpdateAllStatistics();
            ScheduleSave();
        }

        private void SetCurrentTask(TaskVM newCurrentTask)
        {
            if (newCurrentTask == null)
            {
                return;
            }

            SelectedTask = newCurrentTask;
            CurrentTask = newCurrentTask;
            ScheduleSave();
        }


        private void RemoveCurrentTask()
        {
            if (IsTimerRunning)
            {
                StopTimer();
            }

            CurrentTask = null;
            ScheduleSave();
        }

        private void UpdateRunningTimerVisuals(TimeSpan elapsed)
        {
            CurrentTimerElapsed = elapsed;
            TimerStatus = $"Запущен ({FormatElapsed(elapsed)})";
            OnPropertyChanged(nameof(CurrentTaskRunningHours));
            OnPropertyChanged(nameof(CurrentTaskRunningTimeDisplay));
        }

        private void SyncTimerStateFromCurrentTask()
        {
            if (CurrentTask?.IsTimerRunning == true && CurrentTask.TimerStartTime.HasValue)
            {
                _timerStartTime = CurrentTask.TimerStartTime;
                IsTimerRunning = true;
                UpdateRunningTimerVisuals(DateTime.Now - _timerStartTime.Value);
                _timer.Start();
            }
            else
            {
                _timer.Stop();
                _timerStartTime = null;
                if (IsTimerRunning)
                {
                    IsTimerRunning = false;
                }
                else
                {
                    TimerStatus = "Остановлен";
                    CurrentTimerElapsed = TimeSpan.Zero;
                    OnPropertyChanged(nameof(CurrentTaskRunningHours));
                    OnPropertyChanged(nameof(CurrentTaskRunningTimeDisplay));
                }
            }

            UpdateTimerCommands();
        }

        private void UpdateTimerCommands()
        {
            StartTimerCommand?.RaiseCanExecuteChanged();
            StopTimerCommand?.RaiseCanExecuteChanged();
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            return elapsed.TotalHours >= 1
                ? elapsed.ToString(@"hh\:mm\:ss")
                : elapsed.ToString(@"mm\:ss");
        }

        private void ScheduleSave()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await SaveAsync().ConfigureAwait(false);
                }
                catch
                {
                    // ignore persistence errors during background saves
                }
            });
        }
        #endregion

        #region Вспомогательные методы

        private void OnTimerTick()
        {
            if (!IsTimerRunning || CurrentTask == null || !_timerStartTime.HasValue)
            {
                return;
            }

            var elapsed = DateTime.Now - _timerStartTime.Value;
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                UpdateRunningTimerVisuals(elapsed);
            });
        }

        /// <summary>
        /// Генерирует следующий доступный ID для компании
        /// </summary>
        
        private void OnApplicationExit(object? sender, ExitEventArgs e)
        {
            try
            {
                _timer.Stop();
                SaveAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // ignore errors on shutdown
            }
        }

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
            var snapshot = _statisticsService.BuildSnapshot(Companies.Select(c => c.ToDto()), startDate);

            UpdateRevenueTrendSeries(snapshot.MonthlyRevenue, culture);
            UpdateOrdersByStatusSeries(snapshot.OrdersByStatus, culture);
            UpdateTasksStatusSeries(snapshot.TasksByStatus, culture);
            UpdateRevenueByCompanySeries(snapshot.RevenueByCompany, culture);
            UpdateCompanyRevenueSummaries(snapshot.CompanySummaries);

            ApplyStatisticsSort();
            CompanyRevenueView?.Refresh();
        }

        private void RefreshStatistics()
        {
            RefreshStatisticsDashboard();
        }

        private void UpdateRevenueTrendSeries(IReadOnlyList<MonthlyRevenuePoint> data, CultureInfo culture)
        {
            var series = RevenueTrendSeries;
            series.Clear();

            if (data != null && data.Count > 0)
            {
                series.Add(new LineSeries<double>
                {
                    Values = data.Select(point => point.Revenue).ToArray(),
                    Fill = null,
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                    GeometryStroke = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                    GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255), 1),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture)
                });

                RevenueTrendXAxis = new[]
                {
                    new Axis
                    {
                        Labels = data.Select(point => point.Month.ToString("MMM yyyy", culture)).ToArray(),
                        LabelsRotation = 15,
                        TextSize = 12
                    }
                };
            }
            else
            {
                RevenueTrendXAxis = Array.Empty<Axis>();
            }

            RevenueTrendYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };
        }

        private void UpdateOrdersByStatusSeries(IReadOnlyList<OrderStatusPoint> data, CultureInfo culture)
        {
            var series = OrdersByStatusSeries;
            series.Clear();

            if (data != null && data.Count > 0)
            {
                series.Add(new ColumnSeries<int>
                {
                    Values = data.Select(point => point.Count).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(30, 136, 229, 180)),
                    Stroke = new SolidColorPaint(new SKColor(30, 136, 229)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture)
                });

                OrdersByStatusXAxis = new[]
                {
                    new Axis
                    {
                        Labels = data.Select(point => point.Status.GetDescription()).ToArray(),
                        LabelsRotation = 15,
                        TextSize = 12
                    }
                };
            }
            else
            {
                OrdersByStatusXAxis = Array.Empty<Axis>();
            }

            OrdersByStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N0", culture),
                    TextSize = 12
                }
            };
        }

        private void UpdateTasksStatusSeries(IReadOnlyList<TaskStatusPoint> data, CultureInfo culture)
        {
            var series = TasksStatusSeries;
            series.Clear();

            if (data != null && data.Count > 0)
            {
                series.Add(new ColumnSeries<double>
                {
                    Values = data.Select(point => point.Hours).ToArray(),
                    Fill = new SolidColorPaint(new SKColor(76, 175, 80, 180)),
                    Stroke = new SolidColorPaint(new SKColor(56, 142, 60)),
                    DataLabelsPaint = new SolidColorPaint(new SKColor(55, 71, 79)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N1", culture)
                });

                TasksStatusXAxis = new[]
                {
                    new Axis
                    {
                        Labels = data.Select(point => point.Status.GetDescription()).ToArray(),
                        LabelsRotation = 15,
                        TextSize = 12
                    }
                };
            }
            else
            {
                TasksStatusXAxis = Array.Empty<Axis>();
            }

            TasksStatusYAxis = new[]
            {
                new Axis
                {
                    Labeler = value => value.ToString("N1", culture),
                    TextSize = 12
                }
            };
        }

        private void UpdateRevenueByCompanySeries(IReadOnlyList<NamedValue> data, CultureInfo culture)
        {
            var series = RevenueByCompanySeries;
            series.Clear();

            if (data == null || data.Count == 0)
            {
                return;
            }

            var meaningfulValues = data.Where(item => item.Value > 0).ToList();

            foreach (var item in meaningfulValues)
            {
                series.Add(new PieSeries<double>
                {
                    Values = new[] { item.Value },
                    Name = item.Name,
                    DataLabelsPaint = new SolidColorPaint(new SKColor(33, 33, 33)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0", culture),
                    Pushout = data.Count > 1 ? 4 : 0
                });
            }

            if (!meaningfulValues.Any())
            {
                series.Add(new PieSeries<double>
                {
                    Values = new[] { 1d },
                    Name = "Нет данных",
                    DataLabelsPaint = new SolidColorPaint(new SKColor(33, 33, 33)),
                    DataLabelsSize = 12,
                    DataLabelsFormatter = _ => string.Empty
                });
            }
        }

        private void UpdateCompanyRevenueSummaries(IReadOnlyList<CompanyRevenueSummary> summaries)
        {
            var collection = CompanyRevenueSummaries;
            collection.Clear();

            if (summaries == null)
            {
                return;
            }

            foreach (var summary in summaries)
            {
                collection.Add(summary);
            }
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













