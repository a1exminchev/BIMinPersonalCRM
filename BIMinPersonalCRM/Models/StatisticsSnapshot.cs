using System;
using System.Collections.Generic;

namespace BIMinPersonalCRM.Models
{
    public record MonthlyRevenuePoint(DateTime Month, double Revenue);

    public record OrderStatusPoint(OrderExecutionStatus Status, int Count);

    public record TaskStatusPoint(TaskStatus Status, double Hours);

    public record NamedValue(string Name, double Value);

    public record StatisticsSnapshot(
        IReadOnlyList<MonthlyRevenuePoint> MonthlyRevenue,
        IReadOnlyList<OrderStatusPoint> OrdersByStatus,
        IReadOnlyList<TaskStatusPoint> TasksByStatus,
        IReadOnlyList<NamedValue> RevenueByCompany,
        IReadOnlyList<CompanyRevenueSummary> CompanySummaries);
}
