using System.IO;
using System.Windows;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    public class PathExistsToVisibilityConverter : IValueConverter
    {
        public static readonly PathExistsToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return Visibility.Collapsed;
            try
            {
                if (File.Exists(path))
                {
                    return Visibility.Visible;
                }
            }
            catch { return Visibility.Collapsed; }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is Visibility)
            {
                if ((Visibility)value == Visibility.Visible)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return value;
        }
    }
}

