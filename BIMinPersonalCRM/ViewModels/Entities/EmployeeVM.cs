using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.MVVM;

namespace BIMinPersonalCRM.ViewModels.Entities
{
    /// <summary>
    ///     Вью-модель сотрудника.
    /// </summary>
    public class EmployeeVM : VMObject
    {
        private readonly EmployeeDTO _dto;

        public EmployeeVM()
        {
            _dto = new();
            RelationshipStatus = RelationshipStatus.NotKnown;
        }

        public EmployeeVM(EmployeeDTO dto)
        {
            _dto = dto ?? new EmployeeDTO();

            Id = _dto.Id;
            FullName = _dto.FullName;
            Position = _dto.Position;
            Phone = _dto.Phone;
            Comments = _dto.Comments;
            AvatarPath = _dto.AvatarPath;
            RelationshipStatus = _dto.RelationshipStatus;
        }

        public int Id
        {
            get => GetValue<int>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string FullName
        {
            get => GetValue<string>(nameof(FullName));
            set => SetValue(nameof(FullName), value);
        }

        public string Position
        {
            get => GetValue<string>(nameof(Position));
            set => SetValue(nameof(Position), value);
        }

        public string Phone
        {
            get => GetValue<string>(nameof(Phone));
            set => SetValue(nameof(Phone), value);
        }

        public string Comments
        {
            get => GetValue<string>(nameof(Comments));
            set => SetValue(nameof(Comments), value);
        }

        public string AvatarPath
        {
            get => GetValue<string>(nameof(AvatarPath));
            set => SetValue(nameof(AvatarPath), value);
        }

        public RelationshipStatus RelationshipStatus
        {
            get => GetValue<RelationshipStatus>(nameof(RelationshipStatus));
            set => SetValue(nameof(RelationshipStatus), value);
        }

        public EmployeeDTO ToDto()
        {
            _dto.Id = Id;
            _dto.FullName = FullName;
            _dto.Position = Position;
            _dto.Phone = Phone;
            _dto.Comments = Comments;
            _dto.AvatarPath = AvatarPath;
            _dto.RelationshipStatus = RelationshipStatus;

            return _dto;
        }
    }
}
