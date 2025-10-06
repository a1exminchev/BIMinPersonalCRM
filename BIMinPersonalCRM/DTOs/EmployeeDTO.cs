using System;
using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO сотрудника для хранения в JSON.
    /// </summary>
    [Serializable]
    public class EmployeeDTO
    {
        public int Id { get; set; } = -1;
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
        public RelationshipStatus RelationshipStatus { get; set; } = RelationshipStatus.Average;
    }
}
