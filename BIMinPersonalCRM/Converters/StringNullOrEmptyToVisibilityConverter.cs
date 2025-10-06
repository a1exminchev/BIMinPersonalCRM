using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public static readonly StringNullOrEmptyToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEmpty = value == null || string.IsNullOrWhiteSpace(value as string);
            var inverse = string.Equals(parameter as string, "Inverse", StringComparison.OrdinalIgnoreCase);
            var visible = inverse ? !isEmpty : isEmpty;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

