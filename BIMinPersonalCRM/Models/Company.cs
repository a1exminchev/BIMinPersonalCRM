using System.Collections.Generic;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Компания‑клиент, для которой выполняются заказы.
    /// </summary>
    public class Company
    {
        /// <summary>
        ///     Наименование компании.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Контактный телефон компании.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        ///     Веб‑сайт компании.
        /// </summary>
        public string Website { get; set; } = string.Empty;

        /// <summary>
        ///     Путь к логотипу компании.
        /// </summary>
        public string LogoPath { get; set; } = string.Empty;

        /// <summary>
        ///     Цвет карточки компании в шестнадцатеричном формате ( например, "#FFEECC" ).
        /// </summary>
        public string CardColor { get; set; } = "#FFFFFF";

        /// <summary>
        ///     Сотрудники, связанные с этой компанией.
        /// </summary>
        public List<Employee> Employees { get; set; } = new();

        /// <summary>
        ///     Статус отношений с компанией.
        /// </summary>
        public RelationshipStatus RelationshipStatus { get; set; } = RelationshipStatus.Average;

        /// <summary>
        ///     Статус платежеспособности компании.
        /// </summary>
        public PaymentAbilityStatus PaymentAbilityStatus { get; set; } = PaymentAbilityStatus.Medium;

        /// <summary>
        ///     Заказы, выполнявшиеся для компании.
        /// </summary>
        public List<Order> Orders { get; set; } = new();

        /// <summary>
        ///     Средний статус прибыльности по всем заказам.
        ///     Вычисляется динамически при запросе. 
        /// </summary>
        public ProfitabilityStatus AverageProfitability
        {
            get
            {
                if (Orders == null || Orders.Count == 0)
                    return ProfitabilityStatus.Medium;
                double sum = 0;
                foreach (var order in Orders)
                {
                    sum += (int)order.ProfitabilityStatus;
                }
                var avg = sum / Orders.Count;
                return (ProfitabilityStatus)System.Math.Round(avg);
            }
        }
    }
}
