namespace BIMinPersonalCRM.Models
{
    /// <summary>
    /// Задача, связанная с заказом.
    /// </summary>
    public class TaskItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double HoursSpent { get; set; }
        public DateTime StartDate { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Queued;
        
        /// <summary>
        /// Время начала работы с таймером (для восстановления состояния)
        /// </summary>
        public DateTime? TimerStartTime { get; set; }
        
        /// <summary>
        /// Признак того, что таймер запущен для данной задачи
        /// </summary>
        public bool IsTimerRunning { get; set; }
    }
}