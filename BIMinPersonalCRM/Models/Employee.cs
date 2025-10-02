namespace BIMinPersonalCRM.Models
{
    /// <summary>
    ///     Сотрудник компании, участвующий в выполнении заказа или контактный
    ///     представитель заказчика.
    /// </summary>
    public class Employee
    {
        /// <summary>
        ///     Полное имя сотрудника.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        ///     Должность сотрудника.
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        ///     Контактный телефон сотрудника.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        ///     Дополнительные комментарии о сотруднике.
        /// </summary>
        public string Comments { get; set; } = string.Empty;

        /// <summary>
        ///     Путь к файлу аватара сотрудника.
        /// </summary>
        public string AvatarPath { get; set; } = string.Empty;

        /// <summary>
        ///     Статус отношений с данным сотрудником.
        /// </summary>
        public RelationshipStatus RelationshipStatus { get; set; } = RelationshipStatus.Average;
    }
}
