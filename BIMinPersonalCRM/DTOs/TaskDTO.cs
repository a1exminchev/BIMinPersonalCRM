namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO задачи проекта для хранения в JSON.
    /// </summary>
    [Serializable]
    public class TaskDTO
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double HoursSpent { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public Models.TaskStatus Status { get; set; } = Models.TaskStatus.Waiting;
        public DateTime? TimerStartTime { get; set; }
        public bool IsTimerRunning { get; set; } = false;
    }
}

