using System.IO;
using BIMinPersonalCRM.DataObjects;
using Newtonsoft.Json;

namespace BIMinPersonalCRM.Services
{
    /// <summary>
    ///     Реализация IDataService, сохраняющая данные в JSON-файл.
    /// </summary>
    public class JsonDataService : IDataService
    {
        private readonly string _filePath;

        public JsonDataService(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<DataStoreDto> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new DataStoreDto();
                }

                var json = await File.ReadAllTextAsync(_filePath);
                var data = JsonConvert.DeserializeObject<DataStoreDto>(json) ?? new DataStoreDto();
                return data;
            }
            catch (Exception)
            {
                return new DataStoreDto();
            }
        }

        public async Task SaveAsync(DataStoreDto data)
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
