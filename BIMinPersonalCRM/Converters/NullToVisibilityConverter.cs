using System;
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
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

