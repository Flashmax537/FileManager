using System.IO;

namespace FileManager
{
    /// <summary>
    /// Информация о файловом менеджере
    /// </summary>
    class FileManagerVO
    {
        /// <summary>
        /// Путь скопированого файла
        /// </summary>
        public string PathCopy { get; set; }

        /// <summary>
        /// Имя скопированого файла
        /// </summary>
        public string FileCopy { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Индекс выбраного элемента
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        /// Индекс последнего элемента на странице
        /// </summary>
        public int MaxIndex { get; set; }

        /// <summary>
        /// Номер последней страницы
        /// </summary>
        public int MaxPage { get; set; }

        /// <summary>
        /// Информация о папке
        /// </summary>
        public DirectoryInfo DirectoryInfo { get; set; }

        /// <summary>
        /// Информация о файле
        /// </summary>
        public FileSystemInfo[] FileSystemInfo { get; set; }
    }
}
