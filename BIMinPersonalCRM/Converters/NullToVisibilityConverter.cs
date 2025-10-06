using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Конвертер для скрытия элементов при null значениях
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public static readonly NullToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value == null;
            if (parameter is "Inverse") result = !result;
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

