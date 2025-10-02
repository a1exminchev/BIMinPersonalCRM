namespace BIMinPersonalCRM.Models
{
    /// <summary>
    /// Прикрепленный файл к заказу
    /// </summary>
    public class FileAttachment
    {
        /// <summary>
        /// Уникальный идентификатор файла
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название файла
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Тип файла
        /// </summary>
        public FileType FileType { get; set; } = FileType.Other;

        /// <summary>
        /// Размер файла в байтах
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Дата добавления файла
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// Комментарий к файлу
        /// </summary>
        public string? Comment { get; set; }
    }
}

