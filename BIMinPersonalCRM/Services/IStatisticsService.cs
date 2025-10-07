using System;
using System.Collections.Generic;
using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Services
{
    public interface IStatisticsService
    {
        StatisticsSnapshot BuildSnapshot(IEnumerable<CompanyDTO> companies, DateTime? startDate);
    }
}
