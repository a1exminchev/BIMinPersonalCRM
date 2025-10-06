using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.Converters
{
    /// <summary>
    /// Конвертер статуса отношений в цвет фона
    /// </summary>
    public class RelationshipStatusToBrushConverter : IValueConverter
    {
        public static readonly RelationshipStatusToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RelationshipStatus status)
            {
                return status switch
                {
                    RelationshipStatus.Terrible => new SolidColorBrush(Color.FromRgb(255, 205, 210)),
                    RelationshipStatus.Bad => new SolidColorBrush(Color.FromRgb(255, 171, 145)),
                    RelationshipStatus.Average => new SolidColorBrush(Color.FromRgb(255, 249, 196)),
                    RelationshipStatus.Good => new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                    RelationshipStatus.Excellent => new SolidColorBrush(Color.FromRgb(165, 214, 167)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер статуса платежеспособности в цвет фона
    /// </summary>
    public class PaymentAbilityToBrushConverter : IValueConverter
    {
        public static readonly PaymentAbilityToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentAbilityStatus status)
            {
                return status switch
                {
                    PaymentAbilityStatus.Insolvent => new SolidColorBrush(Color.FromRgb(255, 205, 210)),
                    PaymentAbilityStatus.Hard => new SolidColorBrush(Color.FromRgb(255, 171, 145)),
                    PaymentAbilityStatus.Medium => new SolidColorBrush(Color.FromRgb(255, 249, 196)),
                    PaymentAbilityStatus.Good => new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                    PaymentAbilityStatus.Excellent => new SolidColorBrush(Color.FromRgb(165, 214, 167)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер статуса прибыльности в цвет фона
    /// </summary>
    public class ProfitabilityToBrushConverter : IValueConverter
    {
        public static readonly ProfitabilityToBrushConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProfitabilityStatus status)
            {
                return status switch
                {
                    ProfitabilityStatus.NoOrders => new SolidColorBrush(Colors.LightGray),
                    ProfitabilityStatus.VeryLow => new SolidColorBrush(Color.FromRgb(255, 205, 210)),
                    ProfitabilityStatus.Low => new SolidColorBrush(Color.FromRgb(255, 171, 145)),
                    ProfitabilityStatus.Medium => new SolidColorBrush(Color.FromRgb(255, 249, 196)),
                    ProfitabilityStatus.High => new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                    ProfitabilityStatus.VeryHigh => new SolidColorBrush(Color.FromRgb(165, 214, 167)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

