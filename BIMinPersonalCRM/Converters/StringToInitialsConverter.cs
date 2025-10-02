using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Конвертер строки в инициалы
    /// </summary>
    public class StringToInitialsConverter : IValueConverter
    {
        public static readonly StringToInitialsConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    return $"{words[0][0]}{words[1][0]}".ToUpper();
                }
                else if (words.Length == 1 && words[0].Length >= 2)
                {
                    return words[0].Substring(0, 2).ToUpper();
                }
                else if (words.Length == 1 && words[0].Length == 1)
                {
                    return words[0].ToUpper();
                }
            }
            return "??";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

