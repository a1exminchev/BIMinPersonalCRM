using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO компании для хранения в JSON.
    /// </summary>
    [Serializable]
    public class CompanyDTO
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;
        public string CardColor { get; set; } = "#FFFFFF";
        public RelationshipStatus RelationshipStatus { get; set; } = RelationshipStatus.Average;
        public PaymentAbilityStatus PaymentAbilityStatus { get; set; } = PaymentAbilityStatus.Medium;
        public List<EmployeeDTO> Employees { get; set; } = new();
        public List<OrderDTO> Orders { get; set; } = new();
    }
}
