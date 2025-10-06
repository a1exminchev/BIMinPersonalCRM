using System;
using System.Collections.Generic;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO с данными приложения для сохранения в JSON.
    /// </summary>
    [Serializable]
    public class DataStoreDto
    {
        public List<CompanyDTO> Companies { get; set; } = new();
        public int? CurrentTaskId { get; set; }
    }
}
