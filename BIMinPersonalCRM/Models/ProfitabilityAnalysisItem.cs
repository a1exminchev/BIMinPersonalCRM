namespace BIMinPersonalCRM.Models
{
    /// <summary>
    /// Элемент анализа прибыльности для отображения в статистике
    /// </summary>
    public class ProfitabilityAnalysisItem
    {
        /// <summary>
        /// Название компании
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Количество заказов
        /// </summary>
        public int OrdersCount { get; set; }

        /// <summary>
        /// Общая сумма по всем заказам
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Общее количество часов
        /// </summary>
        public double TotalHours { get; set; }

        /// <summary>
        /// Средняя ставка
        /// </summary>
        public double AverageRate { get; set; }

        /// <summary>
        /// Уровень прибыльности (строковое представление)
        /// </summary>
        public string ProfitabilityLevel { get; set; } = string.Empty;
    }
}

