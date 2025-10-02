using System;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    /// Represents aggregated revenue statistics for a single company.
    /// </summary>
    public class CompanyRevenueSummary
    {
        public string CompanyName { get; set; } = string.Empty;
        public int OrdersCount { get; set; }
        public int ActiveOrdersCount { get; set; }
        public int CompletedOrdersCount { get; set; }
        public double TotalRevenue { get; set; }
        public double AverageOrderValue { get; set; }
        public double TotalHours { get; set; }
        public double AverageRate { get; set; }
        public ProfitabilityStatus Profitability { get; set; }
        public string Relationship { get; set; } = string.Empty;
        public string PaymentAbility { get; set; } = string.Empty;
        public DateTime? LastOrderDate { get; set; }
    }
}
