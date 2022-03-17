using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FileManager
{
    class FilesAndDirectories
    {
        /// <summary>
        /// Получить информацию о файлах на странице
        /// </summary>
        /// <param name="page">Номер текущей страницы</param>
        /// <param name="pageSize">Количество элементов на странице</param>
        /// <param name="path">Путь</param>
        /// <returns></returns>
        public static List<FileSystemInfo> GetFilesInfoOnPage(int page, int pageSize, string path)
        {
            int skipFiles = pageSize * page;
            int maxFilesToShow = skipFiles + pageSize;
            var filesOnPage = new List<FileSystemInfo>();

            for (int i = skipFiles; i < maxFilesToShow; i++)
            {
                var files = GetFilesInDirectories(path);

                if (files.Length <= i)
                {
                    break;
                }
                filesOnPage.Add(files[i]);
            }
            return filesOnPage;
        }

        /// <summary>
        /// Отрисовка файлов и подкаталогов
        /// </summary>
        /// <param name="currentIndex">Индекс выбраного элемента</param>
        /// <param name="files">Список файлов</param>
        /// <param name="path">Путь</param>
        /// <param name="pageSize">Количество элементов на странице</param>
        /// <param name="windowSize">Размер окна</param>
        public static void PrintFilesOrDirectories(int currentIndex, List<FileSystemInfo> files, string path, int pageSize, int windowSize)
        {
            Console.Clear();
            FileManager.PrintUpperBound(windowSize);
            FileManager.PrintText(windowSize, path);

            if (currentIndex == -1)
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                FileManager.PrintText(windowSize, "[...]");
                Console.ResetColor();
            }
            else
            {
                FileManager.PrintText(windowSize, "[...]");
            }

            for (int i = 0; i < files.Count(); i++)
            {
                if (currentIndex == i)
                {
                    PrintFileOrDirectory(files[i].Name, true, windowSize);
                    continue;
                }
                PrintFileOrDirectory(files[i].Name, false, windowSize);
            }
            for (int i = files.Count(); i < pageSize; i++)
            {
                FileManager.PrintText(windowSize, "");
            }
        }

        /// <summary>
        /// Отрисовка файла и подкаталога
        /// </summary>
        /// <param name="fileName">Имя Файла</param>
        /// <param name="isSelected">Выбран ли текущий элемент</param>
        /// <param name="windowSize">Размер окна</param>
        public static void PrintFileOrDirectory(string fileName, bool isSelected, int windowSize)
        {
            if (isSelected) Console.BackgroundColor = ConsoleColor.Blue;
            FileManager.PrintText(windowSize, fileName);
            Console.ResetColor();
        }

        /// <summary>
        /// Получение файлов и подкаталогов текущего каталога
        /// </summary>
        /// <param name="path">Путь текущего каталога</param>
        /// <returns></returns>
        public static FileSystemInfo[] GetFilesInDirectories(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            return directoryInfo.GetFileSystemInfos();
        }

        /// <summary>
        /// Копирование содержимого каталога в другой каталог
        /// </summary>
        /// <param name="sourceDirectoryName">Каталог, который необходимо скопировать</param>
        /// <param name="destinationDirectoryName">Местоположение, в которое необходимо скопировать содержимое каталога</param>
        public static void copyDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            if (!Directory.Exists(destinationDirectoryName))
            {
                Directory.CreateDirectory(destinationDirectoryName);
            }

            DirectoryInfo dirInfo = new DirectoryInfo(sourceDirectoryName);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.CopyTo(Path.Combine(destinationDirectoryName, tempfile.Name));
            }

            DirectoryInfo[] directories = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in directories)
            {
                copyDirectory(Path.Combine(sourceDirectoryName, tempdir.Name), Path.Combine(destinationDirectoryName, tempdir.Name));
            }
        }

        /// <summary>
        /// Вовод окна информации о файле
        /// </summary>
        /// <param name="directoryInfo">Информация о папке</param>
        /// <param name="fileSystemInfo">Информация о файле</param>
        /// <param name="path">Путь</param>
        /// <param name="windowSize">Размер окна</param>
        /// <param name="currentIndex">Индекс выбраного элемента</param>
        /// <param name="pageSize">Количество элементов на странице</param>
        /// <param name="page">Номер текущей страницы</param>
        public static void PrintInfo(DirectoryInfo directoryInfo, FileSystemInfo[] fileSystemInfo, string path,
            int windowSize, int currentIndex, int pageSize, int page)
        {
            FileManager.PrintUpperBound(windowSize);
            directoryInfo = new DirectoryInfo(path);
            fileSystemInfo = directoryInfo.GetFileSystemInfos();
            var file = fileSystemInfo[currentIndex + (pageSize * (page - 1))];

            FileManager.PrintText(windowSize, $"Информация о файле: {file.Name}");
            if (File.Exists(file.FullName))
            {
                var fileInfo = new FileInfo(file.FullName);
                FileAttributes attributes = File.GetAttributes(file.FullName);
                FileManager.PrintText(windowSize, $"Тип: Файл");
                FileManager.PrintText(windowSize, $"Размер: {fileInfo.Length} байт");
                FileManager.PrintText(windowSize, $"Только для чтения: {ToYesNoString((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)}");
                FileManager.PrintText(windowSize, $"Скрытый: {ToYesNoString((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)}");
                FileManager.PrintText(windowSize, $"Сестемный: {ToYesNoString((attributes & FileAttributes.System) == FileAttributes.System)}");
            }
            if (Directory.Exists(file.FullName))
            {
                directoryInfo = new DirectoryInfo(file.FullName);
                FileAttributes attributes = directoryInfo.Attributes;
                FileManager.PrintText(windowSize, $"Тип: Папка с файлами");
                try
                {
                    long sizeInBytes = Directory.EnumerateFiles($"{file.FullName}", "*", SearchOption.AllDirectories).Sum(fileInfo => new FileInfo(fileInfo).Length);
                    FileManager.PrintText(windowSize, $"Размер: {sizeInBytes} байт");
                }
                catch
                {
                    FileManager.PrintText(windowSize, "Размер: -");
                }
                FileManager.PrintText(windowSize, $"Только для чтения: {ToYesNoString((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)}");
                FileManager.PrintText(windowSize, $"Скрытый: {ToYesNoString((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)}");
                FileManager.PrintText(windowSize, $"Сестемный: {ToYesNoString((attributes & FileAttributes.System) == FileAttributes.System)}");
            }
            FileManager.PrintLowerBound(windowSize);
        }

        /// <summary>
        /// Получить номер последней страницы
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPage()
        {
            var settings = Properties.Settings.Default;
            return (int)Math.Ceiling((decimal)GetFilesInDirectories(settings.Path).Length / settings.PageSize);
        }

        /// <summary>
        /// Получит индекс последнего элемента на странице
        /// </summary>
        /// <returns></returns>
        public static int GetMaxIndex()
        {
            var settings = Properties.Settings.Default;
            return GetFilesInfoOnPage(settings.Page - 1, settings.PageSize, settings.Path).Count() - 1;
        }

        /// <summary>
        /// Пересчет информации о странице
        /// </summary>
        /// <param name="fileManager">Информация о файловом менеджере</param>
        /// <returns></returns>
        public static FileManagerVO RecalculatInfoOfPage(FileManagerVO fileManager)
        {
            var settings = Properties.Settings.Default;
            fileManager.CurrentIndex = 0;
            fileManager.MaxPage = GetMaxPage();
            fileManager.MaxIndex = GetMaxIndex();
            return fileManager;
        }

        /// <summary>
        /// Преобразование <see cref="bool" /> в "Да" или "Нет" при <c>true</c> или <c>false</c>
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public static string ToYesNoString(bool value)
        {
            return value ? "Да" : "Нет";
        }
    }
}
