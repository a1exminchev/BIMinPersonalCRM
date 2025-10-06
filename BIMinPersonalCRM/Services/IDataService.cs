using BIMinPersonalCRM.DataObjects;

namespace BIMinPersonalCRM.Services
{
    /// <summary>
    ///     Абстракция слоя сохранения данных.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        ///     Загружает сохранённые данные или возвращает пустой набор.
        /// </summary>
        Task<DataStoreDto> LoadAsync();

        /// <summary>
        ///     Сохраняет переданные данные асинхронно.
        /// </summary>
        Task SaveAsync(DataStoreDto data);
    }
}
