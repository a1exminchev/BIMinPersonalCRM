using BIMinPersonalCRM.DataObjects;
using BIMinPersonalCRM.Models;
using BIMinPersonalCRM.MVVM;

namespace BIMinPersonalCRM.ViewModels.Entities
{
    /// <summary>
    ///     Вью-модель вложенного файла.
    /// </summary>
    public class FileAttachmentVM : VMObject
    {
        private readonly FileAttachmentDTO _dto;

        public FileAttachmentVM()
        {
            _dto = new();
            DateAdded = DateTime.Now;
        }

        public FileAttachmentVM(FileAttachmentDTO dto)
        {
            _dto = dto ?? new FileAttachmentDTO();

            Id = _dto.Id;
            Name = _dto.Name;
            FilePath = _dto.FilePath;
            FileType = _dto.FileType;
            FileSize = _dto.FileSize;
            DateAdded = _dto.DateAdded;
            Comment = _dto.Comment;
        }

        public int Id
        {
            get => GetValue<int>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name));
            set => SetValue(nameof(Name), value);
        }

        public string FilePath
        {
            get => GetValue<string>(nameof(FilePath));
            set => SetValue(nameof(FilePath), value);
        }

        public FileType FileType
        {
            get => GetValue<FileType>(nameof(FileType));
            set => SetValue(nameof(FileType), value);
        }

        public long FileSize
        {
            get => GetValue<long>(nameof(FileSize));
            set => SetValue(nameof(FileSize), value);
        }

        public DateTime DateAdded
        {
            get => GetValue<DateTime>(nameof(DateAdded));
            set => SetValue(nameof(DateAdded), value);
        }

        public string Comment
        {
            get => GetValue<string>(nameof(Comment));
            set => SetValue(nameof(Comment), value);
        }

        public FileAttachmentDTO ToDto()
        {
            _dto.Id = Id;
            _dto.Name = Name;
            _dto.FilePath = FilePath;
            _dto.FileType = FileType;
            _dto.FilePath = FilePath;
            _dto.FileSize = FileSize;
            _dto.DateAdded = DateAdded;
            _dto.Comment = Comment;

            return _dto;
        }
    }
}
