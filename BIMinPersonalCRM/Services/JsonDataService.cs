using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Services
{
    /// <summary>
    ///     Implements <see cref="IDataService"/> by serializing and deserializing
    ///     the application's data to and from a JSON file on disk. The file is
    ///     created in the application's working directory if it does not exist.
    /// </summary>
    public class JsonDataService : IDataService
    {
        private readonly string _filePath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDataService"/> class.
        /// </summary>
        /// <param name="filePath">
        ///     The path to the JSON file used for persistence. If the file does
        ///     not exist it will be created when saving data.
        /// </param>
        public JsonDataService(string filePath)
        {
            _filePath = filePath;
        }

        /// <inheritdoc />
        public async Task<DataStore> LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new DataStore();
                }

                await using var stream = File.OpenRead(_filePath);
                var data = await JsonSerializer.DeserializeAsync<DataStore>(stream) ?? new DataStore();
                return data;
            }
            catch (Exception)
            {
                // If deserialization fails return an empty data store rather than
                // propagating the exception. In a real application we would log
                // the exception and perhaps show an error to the user.
                return new DataStore();
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync(DataStore data)
        {
            // Ensure the directory exists before attempting to write the file.
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(_filePath);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(stream, data, options);
        }
    }
}
