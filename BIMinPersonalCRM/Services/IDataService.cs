using System.Threading.Tasks;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Services
{
    /// <summary>
    ///     Abstracts the persistence layer used by the application. This
    ///     interface can be implemented using different storage mechanisms
    ///     (e.g. JSON files, databases). The view models interact with this
    ///     service rather than referencing a specific implementation directly.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        ///     Loads the persisted data asynchronously. If no data exists
        ///     returns a new <see cref="DataStore"/> with empty collections.
        /// </summary>
        Task<DataStore> LoadAsync();

        /// <summary>
        ///     Persists the given <see cref="DataStore"/> asynchronously.
        /// </summary>
        Task SaveAsync(DataStore data);
    }
}
