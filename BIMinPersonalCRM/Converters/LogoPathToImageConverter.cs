using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace BIMinPersonalCRM.Converters
{
    public class LogoPathToImageConverter : IValueConverter
    {
        public static readonly LogoPathToImageConverter Instance = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var path = value as string;
                if (string.IsNullOrWhiteSpace(path)) return null;

                string absolutePath = path;

                if (!Path.IsPathRooted(absolutePath))
                {
                    var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "BIMinPersonalCRM");
                    absolutePath = Path.Combine(baseDir, path);
                }

                if (!File.Exists(absolutePath)) return null;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // избегаем блокировки файла
                image.UriSource = new Uri(absolutePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}


