using System;
using System.Globalization;
using System.Windows.Data;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Конвертер для отображения русского описания перечислений
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public static readonly EnumDescriptionConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return enumValue.GetDescription();
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

