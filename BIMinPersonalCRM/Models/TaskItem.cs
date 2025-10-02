using System;

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
        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
    }
}
