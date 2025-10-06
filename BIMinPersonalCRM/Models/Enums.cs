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
        [Description("Не знакомы")] NotKnown,
        [Description("Ужасные отношения")] Terrible,
        [Description("Плохие отношения")] Bad,
        [Description("Средние отношения")] Average,
        [Description("Хорошие отношения")] Good,
        [Description("Великолепные отношения")] Excellent
    }

    /// <summary>
    ///     Статусы платежеспособности компании.
    /// </summary>
    public enum PaymentAbilityStatus
    {
        [Description("Неизвестные")] NotKnown,
        [Description("Неплатежеспособные")] Insolvent,
        [Description("Сложно с деньгами")] Hard,
        [Description("Средняя платежеспособность")] Medium,
        [Description("Большая платежеспособность")] Good,
        [Description("Высочайшая платежеспособность")] Excellent
    }

    /// <summary>
    ///     Возможные статусы выполнения заказа.
    /// </summary>
    public enum OrderExecutionStatus
    {
        [Description("Мой отказ")] DeclinedByMe,
        [Description("Отказ компании")] DeclinedByCompany,
        [Description("В очереди")] Queued,
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
        [Description("Убыточно")] VeryLow,
        [Description("Малая прибыльность")] Low,
        [Description("Средняя прибыльность")] Medium,
        [Description("Большая прибыльность")] High,
        [Description("Высочайшая прибыльность")] VeryHigh
    }

    /// <summary>
    ///     Статус выполнения задачи.
    /// </summary>
    public enum TaskStatus
    {
        [Description("В ожидании")] Waiting,
        [Description("В работе")] InProgress,
        [Description("Завершена")] Completed
    }

    /// <summary>
    /// Типы файлов для заказов
    /// </summary>
    public enum FileType
    {
        [Description("Договор")] Contract,
        [Description("Техническое задание")] TechnicalSpec,
        [Description("Счет")] Invoice,
        [Description("Чек")] Receipt,
        [Description("Другое")] Other
    }

    /// <summary>
    ///     Вспомогательные методы для извлечения русских описаний перечислений.
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


