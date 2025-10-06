using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO заказа для хранения в JSON.
    /// </summary>
    [Serializable]
    public class OrderDTO
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string SoftwareType { get; set; } = string.Empty;
        public List<FileAttachmentDTO> AttachedFiles { get; set; } = new();
        //public List<EmployeeDTO> CompanyEmployees { get; set; } = new();
        public List<TaskDTO> Tasks { get; set; } = new();
        public OrderExecutionStatus ExecutionStatus { get; set; } = OrderExecutionStatus.Queued;
        public TaxStatus TaxStatus { get; set; } = TaxStatus.None;
        public int ExpectedDurationDays { get; set; } = 0;
        public double ActualDurationHours { get; set; } = 0;
        public DateTime? CompletionDate { get; set; }
        public int CalendarDaysFromStart { get; set; } = 0;
        public ProfitabilityStatus ProfitabilityStatus { get; set; } = ProfitabilityStatus.Medium;
        public string CompanyName { get; set; }
    }
}
