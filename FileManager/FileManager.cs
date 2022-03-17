using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace FileManager
{
    class FileManager
    {
        #region Параметры отображения окна консоли

        #region Библиотеки для работы с размером окна
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /* Устанавливаем окно по его указателю в нужное место */
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, UInt32 wFlags);

        /* Получаем крайние точки окна */
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        #endregion

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_SIZE = 0xF000;//resize

        /// <summary>
        /// Параметры отображения окна консоли
        /// </summary>
        private static void ConsoleSettings()
        {
            Console.Title = "Файловый менеджер";
            Console.SetWindowSize(120, 50);
            /* Получили указатель на нашу консоль */
            var hWnd = GetConsoleWindow();
            var wndRect = new RECT();
            /* Получили ее размеры */
            GetWindowRect(hWnd, out wndRect);
            var cWidth = wndRect.Right - wndRect.Left;
            var cHeight = wndRect.Bottom - wndRect.Top;
            /* Флаг - означает что при установке позиции окна размер не менялся */
            const UInt32 SWP_NOSIZE = 0x0001;
            /* Окна выше остальных */
            var HWND_TOPMOST = 0;
            var Width = ((int)SystemParameters.PrimaryScreenWidth);
            var Height = ((int)SystemParameters.PrimaryScreenHeight);
            /* Установка окна в нужное место */
            SetWindowPos(hWnd, HWND_TOPMOST, Width / 2 - cWidth / 2, Height / 2 - cHeight / 2, 0, 0, SWP_NOSIZE);

            IntPtr sysMenu = GetSystemMenu(hWnd, false);

            if (hWnd != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND); //resize
            }
        }
        #endregion

        /// <summary>
        ///  Точка входа в программу
        /// </summary>
        static void Main()
        {
            var fileManager = new FileManagerVO();
            var settings = Properties.Settings.Default; // Настройки
            var drives = Environment.GetLogicalDrives(); // Список доступных имен логических дисков
            ConsoleSettings();
            fileManager.CurrentIndex = 0;
            fileManager.MaxPage = FilesAndDirectories.GetMaxPage();
            fileManager.MaxIndex = FilesAndDirectories.GetMaxIndex();

            var drive = settings.Path.Substring(0, 3); // Имя текущего логического диска
            if (!drives.Contains(drive)) settings.Path = drives.First(); // Если текущего логического диска нет, то выбираем первый из списока доступных имен логических дисков

            while (true)
            {
                try
                {
                    Console.CursorVisible = false; // Делаем курсор невидимым, для красивого отображения окон
                    FilesAndDirectories.PrintFilesOrDirectories(fileManager.CurrentIndex, 
                        FilesAndDirectories.GetFilesInfoOnPage(settings.Page - 1, settings.PageSize, settings.Path), 
                        settings.Path, settings.PageSize, settings.WindowSize);

                    PrintText(settings.WindowSize, "");
                    PrintText(settings.WindowSize, $"Страница: {settings.Page} из {fileManager.MaxPage}");
                    PrintLowerBound(settings.WindowSize);

                    var directoryInfo = new DirectoryInfo(settings.Path);
                    var fileSystemInfo = directoryInfo.GetFileSystemInfos();
                    if (fileManager.CurrentIndex > -1 && fileSystemInfo.Length != 0)
                    {
                        FilesAndDirectories.PrintInfo(directoryInfo, fileSystemInfo, settings.Path, settings.WindowSize, fileManager.CurrentIndex, settings.PageSize, settings.Page);
                    }

                    PrintCommandsInfo((settings.WindowSize / 2) + 1);
                    PrintMessages(settings.WindowSize, fileManager.Message, fileManager.PathCopy);
                    ConsoleKeyInfo info = Console.ReadKey(); // Ожидание нажатия кнопки
                    fileManager = ButtonsAndCommands.ControlButtons(info.Key, fileManager); // Обновление информации о файловом менеджере
                    Properties.Settings.Default.Save(); // Сохранение настроек
                }
                catch (Exception ex)
                {
                    fileManager.Message = $"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Ошибка: Что-то пошло не так";
                    HandlerException.Log($"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Ошибка: {ex.Message}");
                    var previousDirectory = Path.GetDirectoryName(settings.Path);
                    if (previousDirectory != null) settings.Path = previousDirectory;
                    fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                    settings.Page = 1;
                }
            }
        }

        /// <summary>
        /// Вывод окон помощи управления программой
        /// </summary>
        /// <param name="windowSize">Размер окна</param>
        public static void PrintCommandsInfo(int windowSize)
        {
            Console.Write("┌");
            Console.Write(new string('─', -windowSize + 1));
            Console.Write("┐");
            PrintUpperBound(windowSize);

            Console.Write($"│ {{0, {windowSize}}}│", "Кнопки управления:");
            PrintText(windowSize, "Команды:");
            Console.Write($"│ {{0, {windowSize}}}│", "\"↑\" - следующий элемент");
            PrintText(windowSize, "\"doc\" - открыть документацию");
            Console.Write($"│ {{0, {windowSize}}}│", "\"↓\" - предыдущий элемент");
            PrintText(windowSize, "\"log\"");
            Console.Write($"│ {{0, {windowSize}}}│", "\"→\" - следующая сраница");
            PrintText(windowSize, "\"cd 'Имя логического диска'\" - перейти на другой диск");
            Console.Write($"│ {{0, {windowSize}}}│", "\"←\" - предыдущая сраница");
            PrintText(windowSize, "(Например: 'cd D' для прехода на диск 'D')");
            Console.Write($"│ {{0, {windowSize}}}│", "\"C\" - копирование файлов, каталогов");
            PrintText(windowSize, "");
            Console.Write($"│ {{0, {windowSize}}}│", "\"V\" - вставка файлов, каталогов");
            PrintText(windowSize, "");
            Console.Write($"│ {{0, {windowSize}}}│", "\"Delete\" - удаление файлов, каталогов");
            PrintText(windowSize, "");
            Console.Write($"│ {{0, {windowSize}}}│", "\"Tab\" - ввод команд");
            PrintText(windowSize, "");
            Console.Write($"│ {{0, {windowSize}}}│", "\"Enter\" - открытие файлов, каталогов");
            PrintText(windowSize, "");
            Console.Write($"│ {{0, {windowSize}}}│", "(Для возврата в предыдущую папку нажмите \"[...]\")");
            PrintText(windowSize, "");

            Console.Write("└");
            Console.Write(new string('─', -windowSize + 1));
            Console.Write("┘");
            PrintLowerBound(windowSize);
        }

        /// <summary>
        /// Вывод окна сообщений
        /// </summary>
        /// <param name="windowSize">Размер окна</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="pathCopy">Путь скопированого файл</param>
        public static void PrintMessages(int windowSize, string message, string pathCopy)
        {
            PrintUpperBound(windowSize);
            PrintText(windowSize, "Окно сообщений: ");
            PrintText(windowSize, $"Скопирован файл: {pathCopy}");
            PrintText(windowSize, $"Сообщение: {message}");
            PrintLowerBound(windowSize);
        }

        /// <summary>
        /// Отрисовка нижний границы окна
        /// </summary>
        /// <param name="windowSize">Размер окна</param>
        public static void PrintLowerBound(int windowSize)
        {
            Console.Write("└");
            Console.Write(new string('─', -windowSize + 1));
            Console.WriteLine("┘");
        }

        /// <summary>
        /// Отрисовка верхней границы окна
        /// </summary>
        /// <param name="windowSize">Размер окна</param>
        public static void PrintUpperBound(int windowSize)
        {
            Console.Write("┌");
            Console.Write(new string('─', -windowSize + 1));
            Console.WriteLine("┐");
        }

        /// <summary>
        /// Отрисовка текста внутри окна
        /// </summary>
        /// <param name="windowSize">Размер окна</param>
        /// <param name="text">Текст</param>
        public static void PrintText(int windowSize, string text)
        {
            text = text.Length > -windowSize - 3 ? text.Substring(0, -windowSize - 3) + "..." : text;
            Console.WriteLine($"│ {{0, {windowSize}}}│", text);
        }
    }
}
