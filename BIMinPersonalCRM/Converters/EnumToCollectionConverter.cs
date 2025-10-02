using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Конвертер для преобразования перечислений в коллекции с русскими названиями
    /// </summary>
    public class EnumToCollectionConverter : IValueConverter
    {
        public static readonly EnumToCollectionConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type enumType && enumType.IsEnum)
            {
                return Enum.GetValues(enumType)
                    .Cast<Enum>()
                    .Select(e => new EnumDisplayItem { Value = e, Description = e.GetDescription() })
                    .ToList();
            }
            return new List<EnumDisplayItem>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumDisplayItem item)
                return item.Value;
            return value;
        }
    }

    /// <summary>
    /// Класс для отображения элементов перечисления с описанием
    /// </summary>
    public class EnumDisplayItem
    {
        public Enum Value { get; set; } = null!;
        public string Description { get; set; } = string.Empty;

        public override string ToString() => Description;
    }
}

