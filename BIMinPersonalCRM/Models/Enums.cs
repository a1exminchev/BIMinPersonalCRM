using System;
using System.ComponentModel;
using System.Reflection;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Ð’Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ñ‹Ðµ ÑÑ‚Ð°Ñ‚ÑƒÑÑ‹ Ð¾Ñ‚Ð½Ð¾ÑˆÐµÐ½Ð¸Ð¹ Ñ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸ÐµÐ¹ Ð¸Ð»Ð¸ ÑÐ¾Ñ‚Ñ€ÑƒÐ´Ð½Ð¸ÐºÐ¾Ð¼. 
    /// </summary>
    public enum RelationshipStatus
    {
        [Description("Ð£Ð¶Ð°ÑÐ½Ñ‹Ðµ")] Terrible,
        [Description("ÐŸÐ»Ð¾Ñ…Ð¸Ðµ")] Bad,
        [Description("Ð¡Ñ€ÐµÐ´Ð½Ð¸Ðµ")] Average,
        [Description("Ð¥Ð¾Ñ€Ð¾ÑˆÐ¸Ðµ")] Good,
        [Description("ÐžÑ‚Ð»Ð¸Ñ‡Ð½Ñ‹Ðµ")] Excellent
    }

    /// <summary>
    ///     Ð¡Ñ‚Ð°Ñ‚ÑƒÑÑ‹ Ð¿Ð»Ð°Ñ‚ÐµÐ¶ÐµÑÐ¿Ð¾ÑÐ¾Ð±Ð½Ð¾ÑÑ‚Ð¸ ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸.
    /// </summary>
    public enum PaymentAbilityStatus
    {
        [Description("ÐÐµÐ¿Ð»Ð°Ñ‚ÐµÐ¶ÐµÑÐ¿Ð¾ÑÐ¾Ð±Ð½Ñ‹")] Insolvent,
        [Description("Ð¡Ð»Ð¾Ð¶Ð½Ð¾")] Hard,
        [Description("Ð¡Ñ€ÐµÐ´Ð½Ðµ")] Medium,
        [Description("Ð¥Ð¾Ñ€Ð¾ÑˆÐ¾")] Good,
        [Description("ÐŸÑ€ÐµÐºÑ€Ð°ÑÐ½Ð¾")] Excellent
    }

    /// <summary>
    ///     Ð’Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ñ‹Ðµ ÑÑ‚Ð°Ñ‚ÑƒÑÑ‹ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ Ð·Ð°ÐºÐ°Ð·Ð°.
    /// </summary>
    public enum OrderExecutionStatus
    {
        [Description("Ð’ Ð¾Ñ‡ÐµÑ€ÐµÐ´Ð¸")] Queued,
        [Description("ÐœÐ¾Ð¹ Ð¾Ñ‚ÐºÐ°Ð·")] DeclinedByMe,
        [Description("ÐžÑ‚ÐºÐ°Ð· ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ð¸")] DeclinedByCompany,
        [Description("Ð’ Ð¿Ñ€Ð¾Ñ†ÐµÑÑÐµ")] InProgress,
        [Description("ÐÐ° Ñ‚ÐµÑÑ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ð¸")] Testing,
        [Description("ÐžÐ¶Ð¸Ð´Ð°ÐµÑ‚ Ð¾Ð¿Ð»Ð°Ñ‚Ñ‹")] AwaitingPayment,
        [Description("ÐžÐ¿Ð»Ð°Ñ‡ÐµÐ½")] Paid
    }

    /// <summary>
    ///     Ð’Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ñ‹Ðµ ÑÑ‚Ð°Ñ‚ÑƒÑÑ‹ Ð½Ð°Ð»Ð¾Ð³Ð¾Ð¾Ð±Ð»Ð°Ð¶ÐµÐ½Ð¸Ñ Ð·Ð°ÐºÐ°Ð·Ð°.
    /// </summary>
    public enum TaxStatus
    {
        [Description("Ð‘ÐµÐ· Ð½Ð°Ð»Ð¾Ð³Ð°")] None,
        [Description("ÐžÐ¶Ð¸Ð´Ð°ÐµÑ‚ Ð¾Ð¿Ð»Ð°Ñ‚Ñ‹")] AwaitingPayment,
        [Description("ÐžÐ¿Ð»Ð°Ñ‡ÐµÐ½Ð¾")] Paid
    }

    /// <summary>
    ///     ÐžÑ†ÐµÐ½ÐºÐ° Ð¿Ñ€Ð¸Ð±Ñ‹Ð»ÑŒÐ½Ð¾ÑÑ‚Ð¸ Ð·Ð°ÐºÐ°Ð·Ð°.
    /// </summary>
    public enum ProfitabilityStatus
    {
        [Description("ÐžÑ‡ÐµÐ½ÑŒ Ð¼Ð°Ð»Ð¾")] VeryLow,
        [Description("ÐœÐ°Ð»Ð¾")] Low,
        [Description("Ð¡Ñ€ÐµÐ´Ð½Ðµ")] Medium,
        [Description("ÐœÐ½Ð¾Ð³Ð¾")] High,
        [Description("ÐžÑ‡ÐµÐ½ÑŒ Ð¼Ð½Ð¾Ð³Ð¾")] VeryHigh
    }

    /// <summary>
    ///     Ð¡Ñ‚Ð°Ñ‚ÑƒÑ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ Ð·Ð°Ð´Ð°Ñ‡Ð¸.
    /// </summary>
    public enum TaskStatus
    {
        [Description("Ð’ Ð¾Ñ‡ÐµÑ€ÐµÐ´Ð¸")] Queued,
        [Description("Ð’ Ñ€Ð°Ð±Ð¾Ñ‚Ðµ")] InProgress,
        [Description("Ð—Ð°Ð²ÐµÑ€ÑˆÐµÐ½Ð°")] Completed
    }

    /// <summary>
    /// Ð¢Ð¸Ð¿Ñ‹ Ñ„Ð°Ð¹Ð»Ð¾Ð² Ð´Ð»Ñ Ð·Ð°ÐºÐ°Ð·Ð¾Ð²
    /// </summary>
    public enum FileType
    {
        [Description("Ð”Ð¾Ð³Ð¾Ð²Ð¾Ñ€")] Contract,
        [Description("Ð¢ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¾Ðµ Ð·Ð°Ð´Ð°Ð½Ð¸Ðµ")] TechnicalSpec,
        [Description("Ð¡Ñ‡ÐµÑ‚")] Invoice,
        [Description("Ð§ÐµÐº")] Receipt,
        [Description("Ð”Ñ€ÑƒÐ³Ð¾Ðµ")] Other
    }

    /// <summary>
    ///     Ð’ÑÐ¿Ð¾Ð¼Ð¾Ð³Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ð¼ÐµÑ‚Ð¾Ð´Ñ‹ Ð´Ð»Ñ Ð¸Ð·Ð²Ð»ÐµÑ‡ÐµÐ½Ð¸Ñ Ñ€ÑƒÑÑÐºÐ¸Ñ… Ð¾Ð¿Ð¸ÑÐ°Ð½Ð¸Ð¹ Ð¿ÐµÑ€ÐµÑ‡Ð¸ÑÐ»ÐµÐ½Ð¸Ð¹.
    /// </summary>
        /// <summary>
    ///     Периоды для отбора данных статистики.
    /// </summary>
    public enum StatisticsPeriod
    {
        [Description("Последние 30 дней")] Last30Days,
        [Description("Последние 90 дней")] Last90Days,
        [Description("С начала года")] YearToDate,
        [Description("За все время")] AllTime
    }
public static class EnumExtensions
    {
        /// <summary>
        ///     ÐŸÐ¾Ð»ÑƒÑ‡Ð¸Ñ‚ÑŒ ÑÑ‚Ñ€Ð¾ÐºÐ¾Ð²Ð¾Ðµ Ð¾Ð¿Ð¸ÑÐ°Ð½Ð¸Ðµ (DescriptionAttribute) Ð´Ð»Ñ Ð·Ð½Ð°Ñ‡ÐµÐ½Ð¸Ñ Ð¿ÐµÑ€ÐµÑ‡Ð¸ÑÐ»ÐµÐ½Ð¸Ñ. 
        ///     Ð•ÑÐ»Ð¸ Ð°Ñ‚Ñ€Ð¸Ð±ÑƒÑ‚ Ð½Ðµ Ð·Ð°Ð´Ð°Ð½, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ÑÑ Ð¸Ð¼Ñ Ð·Ð½Ð°Ñ‡ÐµÐ½Ð¸Ñ.
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field != null)
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null)
                {
                    return attr.Description;
                }
            }
            return value.ToString();
        }
    }
}


