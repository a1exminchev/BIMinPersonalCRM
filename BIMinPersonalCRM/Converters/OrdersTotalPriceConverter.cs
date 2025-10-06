using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BIMinPersonalCRM.ViewModels.Entities;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    ///     Конвертер для расчёта суммы заказов.
    /// </summary>
    public class OrdersTotalPriceConverter : IValueConverter
    {
        public static readonly OrdersTotalPriceConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<OrderVM> orders)
            {
                return orders.Sum(o => o.Price);
            }

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
