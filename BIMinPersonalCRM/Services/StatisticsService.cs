using System;
using System.Collections.Generic;
using System.Linq;
using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Services
{
    public class StatisticsService : IStatisticsService
    {
        public StatisticsSnapshot BuildSnapshot(IEnumerable<CompanyDTO> companies, DateTime? startDate)
        {
            var companyList = companies?.ToList() ?? new List<CompanyDTO>();

            var filteredOrders = companyList
                .SelectMany(company => company.Orders ?? new List<OrderDTO>(), (company, order) => new { company, order })
                .Where(x => !startDate.HasValue || x.order.CreatedDate.Date >= startDate.Value.Date)
                .ToList();

            var monthlyRevenue = filteredOrders
                .GroupBy(x => new DateTime(x.order.CreatedDate.Year, x.order.CreatedDate.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new MonthlyRevenuePoint(g.Key, g.Sum(x => x.order.Price)))
                .ToList();

            var ordersByStatus = filteredOrders
                .GroupBy(x => x.order.ExecutionStatus)
                .OrderBy(g => g.Key)
                .Select(g => new OrderStatusPoint(g.Key, g.Count()))
                .ToList();

            var tasksQuery = companyList
                .SelectMany(company => company.Orders ?? new List<OrderDTO>())
                .SelectMany(order => order.Tasks ?? new List<TaskDTO>());

            if (startDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(task => task.StartDate >= startDate.Value.Date);
            }

            var tasksByStatus = tasksQuery
                .GroupBy(task => task.Status)
                .OrderBy(g => g.Key)
                .Select(g => new TaskStatusPoint(g.Key, g.Sum(t => t.HoursSpent)))
                .ToList();

            var revenueByCompanyRaw = filteredOrders
                .GroupBy(x => x.company.Name)
                .Select(g => new NamedValue(g.Key, g.Sum(item => item.order.Price)))
                .OrderByDescending(item => item.Value)
                .ToList();

            var revenueByCompany = PrepareTopCompanies(revenueByCompanyRaw);

            var summaries = BuildCompanySummaries(companyList, startDate);

            return new StatisticsSnapshot(monthlyRevenue, ordersByStatus, tasksByStatus, revenueByCompany, summaries);
        }

        private static List<NamedValue> PrepareTopCompanies(IReadOnlyList<NamedValue> revenueByCompany)
        {
            if (revenueByCompany == null || revenueByCompany.Count == 0)
            {
                return new List<NamedValue>();
            }

            var topCompanies = revenueByCompany.Take(7).ToList();
            var otherRevenue = revenueByCompany.Skip(7).Sum(item => item.Value);

            if (otherRevenue > 0)
            {
                topCompanies.Add(new NamedValue("Прочие", otherRevenue));
            }

            return topCompanies;
        }

        private static List<CompanyRevenueSummary> BuildCompanySummaries(IEnumerable<CompanyDTO> companies, DateTime? startDate)
        {
            var result = new List<CompanyRevenueSummary>();

            foreach (var company in companies ?? Enumerable.Empty<CompanyDTO>())
            {
                var orders = (company.Orders ?? new List<OrderDTO>())
                    .Where(order => !startDate.HasValue || order.CreatedDate.Date >= startDate.Value.Date)
                    .ToList();

                var totalRevenue = orders.Sum(o => o.Price);
                var totalHours = orders.SelectMany(o => o.Tasks ?? new List<TaskDTO>()).Sum(t => t.HoursSpent);

                result.Add(new CompanyRevenueSummary
                {
                    CompanyName = company.Name,
                    OrdersCount = orders.Count,
                    ActiveOrdersCount = orders.Count(o => o.ExecutionStatus is OrderExecutionStatus.InProgress or OrderExecutionStatus.Testing or OrderExecutionStatus.AwaitingPayment),
                    CompletedOrdersCount = orders.Count(o => o.ExecutionStatus == OrderExecutionStatus.Paid),
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0,
                    TotalHours = totalHours,
                    AverageRate = totalHours > 0 ? totalRevenue / totalHours : 0,
                    Profitability = CalculateAverageProfitability(company),
                    Relationship = company.RelationshipStatus.GetDescription(),
                    PaymentAbility = company.PaymentAbilityStatus.GetDescription(),
                    LastOrderDate = orders.Any() ? orders.Max(o => (DateTime?)o.CreatedDate) : null
                });
            }

            return result;
        }

        private static ProfitabilityStatus CalculateAverageProfitability(CompanyDTO company)
        {
            var orders = company.Orders;
            if (orders == null || orders.Count == 0)
            {
                return ProfitabilityStatus.NoOrders;
            }

            var average = orders.Average(order => (int)order.ProfitabilityStatus);
            return (ProfitabilityStatus)Math.Round(average);
        }
    }
}
