using System.Collections.Generic;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    /// Represents the root object persisted to the JSON file.
    /// Aggregates collections of domain models used by the application.
    /// </summary>
    public class DataStore
    {
        /// <summary>
        /// Gets or sets the list of companies recorded in the system.
        /// </summary>
        public List<Company> Companies { get; set; } = new();

        /// <summary>
        /// Gets or sets the ID of the currently selected task for tracking time.
        /// </summary>
        public int? CurrentTaskId { get; set; }
    }
}
