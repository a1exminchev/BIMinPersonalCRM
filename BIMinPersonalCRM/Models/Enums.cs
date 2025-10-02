using System;
using System.ComponentModel;
using System.Reflection;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Возможные статусы отношений с компанией или сотрудником. 
    /// </summary>
    public enum RelationshipStatus
    {
        [Description("Ужасные")] Terrible,
        [Description("Плохие")] Bad,
        [Description("Средние")] Average,
        [Description("Хорошие")] Good,
        [Description("Отличные")] Excellent
    }

    /// <summary>
    ///     Статусы платежеспособности компании.
    /// </summary>
    public enum PaymentAbilityStatus
    {
        [Description("Неплатежеспособны")] Insolvent,
        [Description("Сложно")] Hard,
        [Description("Средне")] Medium,
        [Description("Хорошо")] Good,
        [Description("Прекрасно")] Excellent
    }

    /// <summary>
    ///     Возможные статусы выполнения заказа.
    /// </summary>
    public enum OrderExecutionStatus
    {
        [Description("В очереди")] Queued,
        [Description("Мой отказ")] DeclinedByMe,
        [Description("Отказ компании")] DeclinedByCompany,
        [Description("В процессе")] InProgress,
        [Description("На тестировании")] Testing,
        [Description("Ожидает оплаты")] AwaitingPayment,
        [Description("Оплачен")] Paid
    }

    /// <summary>
    ///     Возможные статусы налогооблажения заказа.
    /// </summary>
    public enum TaxStatus
    {
        [Description("Без налога")] None,
        [Description("Ожидает оплаты")] AwaitingPayment,
        [Description("Оплачено")] Paid
    }

    /// <summary>
    ///     Оценка прибыльности заказа.
    /// </summary>
    public enum ProfitabilityStatus
    {
        [Description("Очень мало")] VeryLow,
        [Description("Мало")] Low,
        [Description("Средне")] Medium,
        [Description("Много")] High,
        [Description("Очень много")] VeryHigh
    }

    /// <summary>
    ///     Статус выполнения задачи.
    /// </summary>
    public enum TaskStatus
    {
        [Description("В очереди")] Queued,
        [Description("В работе")] InProgress,
        [Description("Завершена")] Completed
    }

    /// <summary>
    ///     Вспомогательные методы для извлечения русских описаний перечислений.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Получить строковое описание (DescriptionAttribute) для значения перечисления. 
        ///     Если атрибут не задан, возвращается имя значения.
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
