using System;
using System.Collections.Generic;

namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Заказ, привязанный к конкретной компании. 
    ///     Содержит информацию о цене, типе ПО, задачах и статусах.
    /// </summary>
    public class Order
    {
        /// <summary>
        ///     Стоимость заказа ( в рублях ).
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        ///     Тип программного обеспечения (Revit, Pilot‑BIM, Navisworks и т.п.).
        /// </summary>
        public string SoftwareType { get; set; } = string.Empty;

        /// <summary>
        ///     Список путей к прикреплённым файлам ( договоры, ТЗ, счета и т.д. ).
        /// </summary>
        public List<string> AttachedFiles { get; set; } = new();

        /// <summary>
        ///     Сотрудники компании, с которыми велась работа по заказу.
        /// </summary>
        public List<Employee> CompanyEmployees { get; set; } = new();

        /// <summary>
        ///     Список задач, относящихся к заказу.
        /// </summary>
        public List<TaskItem> Tasks { get; set; } = new();

        /// <summary>
        ///     Статус выполнения заказа.
        /// </summary>
        public OrderExecutionStatus ExecutionStatus { get; set; } = OrderExecutionStatus.Queued;

        /// <summary>
        ///     Статус налога для заказа.
        /// </summary>
        public TaxStatus TaxStatus { get; set; } = TaxStatus.None;

        /// <summary>
        ///     Ожидаемый срок выполнения заказа ( в днях ).
        /// </summary>
        public int ExpectedDurationDays { get; set; }

        /// <summary>
        ///     Фактический срок выполнения ( в часах ). 
        ///     Вычисляется на основе суммарных часов всех задач или редактируется вручную.
        /// </summary>
        public double ActualDurationHours { get; set; }

        /// <summary>
        ///     Дата завершения заказа.
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        ///     Количество календарных дней от начала работы до завершения.
        /// </summary>
        public int CalendarDaysFromStart { get; set; }

        /// <summary>
        ///     Статус прибыльности заказа. 
        ///     Автоматически пересчитывается при изменении цены или часов.
        /// </summary>
        public ProfitabilityStatus ProfitabilityStatus { get; set; } = ProfitabilityStatus.Medium;

        /// <summary>
        ///     Стоимость часа для заказа. 
        ///     Рассчитывается как цена ÷ суммарные часы всех задач ( если часы > 0 ).
        /// </summary>
        public double HourlyRate
        {
            get
            {
                double totalHours = 0;
                foreach (var task in Tasks)
                {
                    totalHours += task.HoursSpent;
                }
                return totalHours > 0 ? Price / totalHours : 0;
            }
        }

        /// <summary>
        ///     Обновить статус прибыльности на основе стоимости часа. 
        ///     Можно вызывать после изменения цены или часов.
        /// </summary>
        public void UpdateProfitabilityStatus()
        {
            var rate = HourlyRate;
            // Примитивная шкала для статуса прибыльности.
            if (rate < 1000) // меньше 1000 ₽/ч
                ProfitabilityStatus = ProfitabilityStatus.VeryLow;
            else if (rate < 2000)
                ProfitabilityStatus = ProfitabilityStatus.Low;
            else if (rate < 3000)
                ProfitabilityStatus = ProfitabilityStatus.Medium;
            else if (rate < 5000)
                ProfitabilityStatus = ProfitabilityStatus.High;
            else
                ProfitabilityStatus = ProfitabilityStatus.VeryHigh;
        }
    }
}
