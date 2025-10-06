using System;
using System.Globalization;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Converts a byte value to megabytes for display purposes.
    /// </summary>
    public class BytesToMegabytesConverter : IValueConverter
    {
        private const double BytesInMegabyte = 1024d * 1024d;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return 0d;
            }

            var bytes = value switch
            {
                long longValue => longValue,
                int intValue => intValue,
                double doubleValue => doubleValue,
                float floatValue => floatValue,
                decimal decimalValue => (double)decimalValue,
                string stringValue when double.TryParse(stringValue, NumberStyles.Any, culture, out var parsed) => parsed,
                _ => 0d
            };

            return bytes / BytesInMegabyte;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return 0L;
            }

            double megabytes = value switch
            {
                double doubleValue => doubleValue,
                float floatValue => floatValue,
                long longValue => longValue,
                int intValue => intValue,
                decimal decimalValue => (double)decimalValue,
                string stringValue when double.TryParse(stringValue, NumberStyles.Any, culture, out var parsed) => parsed,
                _ => 0d
            };

            var bytes = megabytes * BytesInMegabyte;

            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                return (long)Math.Round(bytes);
            }

            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return (int)Math.Round(bytes);
            }

            return bytes;
        }
    }
}
