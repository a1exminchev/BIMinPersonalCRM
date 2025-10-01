using System.Collections.Generic;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Represents the root object persisted to the JSON file. It
    ///     aggregates collections of the domain models used by the application.
    /// </summary>
    public class DataStore
    {
        /// <summary>
        ///     Gets or sets the list of clients recorded in the system.
        /// </summary>
        public List<Client> Clients { get; set; } = new();

        /// <summary>
        ///     Gets or sets the list of tasks recorded in the system.
        /// </summary>
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
