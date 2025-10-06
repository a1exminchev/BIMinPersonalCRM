using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.MVVM;

namespace BIMinPersonalCRM.ViewModels.Entities
{
    /// <summary>
    ///     Вью-модель задачи заказа.
    /// </summary>
    public class TaskVM : VMObject
    {
        private readonly TaskDTO _dto;

        public TaskVM()
        {
            HoursSpent = 0;
            StartDate = DateTime.Now;
            Status = Models.TaskStatus.Waiting;
        }

        public TaskVM(TaskDTO dto)
        {
            _dto = dto ?? new TaskDTO();

            Id = _dto.Id;
            Name = _dto.Name;
            Description = _dto.Description;
            HoursSpent = _dto.HoursSpent;
            StartDate = _dto.StartDate;
            Status = _dto.Status;
            TimerStartTime = _dto.TimerStartTime;
            IsTimerRunning = _dto.IsTimerRunning;
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

        public string Description
        {
            get => GetValue<string>(nameof(Description));
            set => SetValue(nameof(Description), value);
        }

        public double HoursSpent
        {
            get => GetValue<double>(nameof(HoursSpent));
            set => SetValue(nameof(HoursSpent), value);
        }

        public DateTime StartDate
        {
            get => GetValue<DateTime>(nameof(StartDate));
            set => SetValue(nameof(StartDate), value);
        }

        public Models.TaskStatus Status
        {
            get => GetValue<Models.TaskStatus>(nameof(Status));
            set => SetValue(nameof(Status), value);
        }

        public DateTime? TimerStartTime
        {
            get => GetValue<DateTime?>(nameof(TimerStartTime));
            set => SetValue(nameof(TimerStartTime), value);
        }

        public bool IsTimerRunning
        {
            get => GetValue<bool>(nameof(IsTimerRunning));
            set => SetValue(nameof(IsTimerRunning), value);
        }

        public TaskDTO ToDto()
        {
            _dto.Id = Id;
            _dto.Name = Name;
            _dto.Description = Description;
            _dto.HoursSpent = HoursSpent;
            _dto.StartDate = StartDate;
            _dto.Status = Status;
            _dto.TimerStartTime = TimerStartTime;
            _dto.IsTimerRunning = IsTimerRunning;

            return _dto;
        }
    }
}

