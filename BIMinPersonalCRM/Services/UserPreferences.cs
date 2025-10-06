using System.IO;
using System.Text.Json;

namespace BIMinPersonalCRM.Services
{
    internal static class UserPreferences
    {
        private class PreferenceData
        {
            public bool? IsLightTheme { get; set; }
        }

        private static readonly object SyncRoot = new();
        private static PreferenceData _data = new();
        private static bool _initialized;

        private static string FilePath => AppSettings.PreferencesFile;

        private static void EnsureLoaded()
        {
            if (_initialized) return;

            lock (SyncRoot)
            {
                if (_initialized) return;
                try
                {
                    if (File.Exists(FilePath))
                    {
                        var json = File.ReadAllText(FilePath);
                        _data = JsonSerializer.Deserialize<PreferenceData>(json) ?? new PreferenceData();
                    }
                }
                catch
                {
                    _data = new PreferenceData();
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        public static bool? GetThemePreference()
        {
            EnsureLoaded();
            return _data.IsLightTheme;
        }

        public static void SetThemePreference(bool isLight)
        {
            EnsureLoaded();
            _data.IsLightTheme = isLight;
            Save();
        }

        private static void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Ignore persistence errors
            }
        }
    }
}
