using BIMinPersonalCRM.Models;

namespace BIMinPersonalCRM.DataObjects
{
    /// <summary>
    ///     DTO вложенного файла для сохранения в JSON.
    /// </summary>
    [Serializable]
    public class FileAttachmentDTO
    {
        public int Id { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public FileType FileType { get; set; } = FileType.Other;
        public long FileSize { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string Comment { get; set; } = string.Empty;
    }
}
