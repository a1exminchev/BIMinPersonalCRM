using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    public class PathExistsConverter : IValueConverter
    {
        public static readonly PathExistsConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return false;
            try { return File.Exists(path); } catch { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

