using System.Collections.ObjectModel;
using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.MVVM;

namespace BIMinPersonalCRM.ViewModels.Entities
{
    /// <summary>
    /// Вью-модель заказа.
    /// </summary>
    public class OrderVM : VMObject
    {
        private readonly OrderDTO _dto;

        public OrderVM()
        {
            _dto = new();
            AttachedFiles = new();
            Tasks = new();
            Price = 0;
            CreatedDate = DateTime.Now;
            CalendarDaysFromStart = 0;
        }

        public OrderVM(OrderDTO dto)
        {
            _dto = dto ?? new OrderDTO();

            Id = _dto.Id;
            Name = _dto.Name;
            Price = _dto.Price;
            CreatedDate = _dto.CreatedDate;
            SoftwareType = _dto.SoftwareType;
            AttachedFiles = new(dto.AttachedFiles.Select(f => new FileAttachmentVM(f)));
            Tasks = new(dto.Tasks.Select(e => new TaskVM(e)));
            ExecutionStatus = _dto.ExecutionStatus;
            TaxStatus = _dto.TaxStatus;
            ExpectedDurationDays = _dto.ExpectedDurationDays;
            ActualDurationHours = _dto.ActualDurationHours;
            CompletionDate = _dto.CompletionDate;
            CalendarDaysFromStart = _dto.CalendarDaysFromStart;
            ProfitabilityStatus = _dto.ProfitabilityStatus;
            CompanyName = _dto.CompanyName;
        }

        public int Id
        {
            get => GetValue<int>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name));
            set => SetValue(nameof(Name), value);
        }

        public double Price
        {
            get => GetValue<double>(nameof(Price));
            set => SetValue(nameof(Price), value);
        }

        public DateTime CreatedDate
        {
            get => GetValue<DateTime>(nameof(CreatedDate));
            set => SetValue(nameof(CreatedDate), value);
        }

        public string SoftwareType
        {
            get => GetValue<string>(nameof(SoftwareType));
            set => SetValue(nameof(SoftwareType), value);
        }

        public ObservableCollection<FileAttachmentVM> AttachedFiles
        {
            get => GetValue<ObservableCollection<FileAttachmentVM>>(nameof(AttachedFiles));
            set => SetValue(nameof(AttachedFiles), value);
        }

        public ObservableCollection<TaskVM> Tasks
        {
            get => GetValue<ObservableCollection<TaskVM>>(nameof(Tasks));
            set => SetValue(nameof(Tasks), value);
        }

        public OrderExecutionStatus ExecutionStatus
        {
            get => GetValue<OrderExecutionStatus>(nameof(ExecutionStatus));
            set => SetValue(nameof(ExecutionStatus), value);
        }

        public TaxStatus TaxStatus
        {
            get => GetValue<TaxStatus>(nameof(TaxStatus));
            set => SetValue(nameof(TaxStatus), value);
        }

        public int ExpectedDurationDays
        {
            get => GetValue<int>(nameof(ExpectedDurationDays));
            set => SetValue(nameof(ExpectedDurationDays), value);
        }

        public double ActualDurationHours
        {
            get => GetValue<double>(nameof(ActualDurationHours));
            set => SetValue(nameof(ActualDurationHours), value);
        }

        public DateTime? CompletionDate
        {
            get => GetValue<DateTime?>(nameof(CompletionDate));
            set => SetValue(nameof(CompletionDate), value);
        }

        public int CalendarDaysFromStart
        {
            get => GetValue<int>(nameof(CalendarDaysFromStart));
            set => SetValue(nameof(CalendarDaysFromStart), value);
        }

        public ProfitabilityStatus ProfitabilityStatus
        {
            get => GetValue<ProfitabilityStatus>(nameof(ProfitabilityStatus));
            set => SetValue(nameof(ProfitabilityStatus), value);
        }

        public string CompanyName
        {
            get => GetValue<string>(nameof(CompanyName));
            set => SetValue(nameof(CompanyName), value);
        }

        public double HourlyRate
        {
            get
            {
                var totalHours = Tasks.Sum(task => task.HoursSpent);
                return totalHours > 0 ? Price / totalHours : 0;
            }
        }

        public void UpdateProfitabilityStatus()
        {
            var rate = HourlyRate;
            if (rate < 1000)
            {
                ProfitabilityStatus = ProfitabilityStatus.VeryLow;
            }
            else if (rate < 2000)
            {
                ProfitabilityStatus = ProfitabilityStatus.Low;
            }
            else if (rate < 3000)
            {
                ProfitabilityStatus = ProfitabilityStatus.Medium;
            }
            else if (rate < 5000)
            {
                ProfitabilityStatus = ProfitabilityStatus.High;
            }
            else
            {
                ProfitabilityStatus = ProfitabilityStatus.VeryHigh;
            }
        }

        public OrderDTO ToDto()
        {
            _dto.Id = Id;
            _dto.Name = Name;
            _dto.Price = Price;
            _dto.CreatedDate = CreatedDate;
            _dto.SoftwareType = SoftwareType;
            _dto.ActualDurationHours = ActualDurationHours;
            _dto.AttachedFiles = AttachedFiles.Select(f => f.ToDto()).ToList();
            _dto.Tasks = Tasks.Select(t => t.ToDto()).ToList();
            _dto.ExecutionStatus = ExecutionStatus;
            _dto.TaxStatus = TaxStatus;
            _dto.ExpectedDurationDays = ExpectedDurationDays;
            _dto.ActualDurationHours = ActualDurationHours;
            _dto.CompletionDate = CompletionDate;
            _dto.CalendarDaysFromStart = CalendarDaysFromStart;
            _dto.ProfitabilityStatus = ProfitabilityStatus;
            _dto.CompanyName = CompanyName;
            return _dto;
        }
    }
}
