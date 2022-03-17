using System;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace FileManager
{
    class ButtonsAndCommands
    {
        /// <summary>
        /// Кнопки управления программой
        /// </summary>
        /// <param name="key">Нажатая кнопка</param>
        /// <param name="fileManager">Информация о файловом менеджере</param>
        /// <returns></returns>
        public static FileManagerVO ControlButtons(ConsoleKey key, FileManagerVO fileManager)
        {
            var settings = Properties.Settings.Default;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    {
                        if (fileManager.CurrentIndex > -1) fileManager.CurrentIndex--;
                    }
                    break;
                case ConsoleKey.DownArrow:
                    {
                        if (fileManager.CurrentIndex < fileManager.MaxIndex) fileManager.CurrentIndex++;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    {
                        if (settings.Page >= settings.Page) settings.Page++;
                        fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    {
                        if (settings.Page > 1) settings.Page--;
                        fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                    }
                    break;
                case ConsoleKey.Enter:
                    {
                        fileManager.DirectoryInfo = new DirectoryInfo(settings.Path);
                        fileManager.FileSystemInfo = fileManager.DirectoryInfo.GetFileSystemInfos();

                        if (fileManager.CurrentIndex != -1 && Directory.Exists(fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName))
                        {
                            settings.Path = fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName;
                            fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                            settings.Page = 1;
                            break;
                        }
                        if (fileManager.CurrentIndex != -1 && File.Exists(fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName))
                        {
                            Process.Start(new ProcessStartInfo() { FileName = fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName, UseShellExecute = true });
                            break;
                        }
                        if (fileManager.CurrentIndex == -1)
                        {
                            var previousDirectory = Path.GetDirectoryName(settings.Path);
                            if (previousDirectory != null) settings.Path = previousDirectory;
                            fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                            settings.Page = 1;
                        }
                    }
                    break;
                case ConsoleKey.Delete:
                    {
                        fileManager.DirectoryInfo = new DirectoryInfo(settings.Path);
                        fileManager.FileSystemInfo = fileManager.DirectoryInfo.GetFileSystemInfos();
                        var file = fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName;
                        if (fileManager.CurrentIndex != -1 && File.Exists(file))
                        {
                            //File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                            fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                            settings.Page = 1;
                        }
                        if (fileManager.CurrentIndex != -1 && Directory.Exists(file))
                        {
                            try
                            {
                                Directory.Delete(file);
                                fileManager = FilesAndDirectories.RecalculatInfoOfPage(fileManager);
                                settings.Page = 1;
                            }
                            catch
                            {
                                fileManager.Message = $"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Ошибка удаления: Папка не пустая, сначала удалите содержимое папки!";
                                HandlerException.Log(fileManager.Message);
                            }
                        }
                    }
                    break;
                case ConsoleKey.C:
                    {
                        fileManager.DirectoryInfo = new DirectoryInfo(settings.Path);
                        fileManager.FileSystemInfo = fileManager.DirectoryInfo.GetFileSystemInfos();
                        fileManager.PathCopy = fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].FullName;
                        fileManager.FileCopy = fileManager.FileSystemInfo[fileManager.CurrentIndex + (settings.PageSize * (settings.Page - 1))].Name;
                    }
                    break;
                case ConsoleKey.V:
                    {
                        var pathPaste = settings.Path + "\\" + fileManager.FileCopy;
                        if (File.Exists(fileManager.PathCopy))
                        {
                            File.Copy(fileManager.PathCopy, pathPaste, true);
                            //File.SetAttributes(fileManager.PathCopy, FileAttributes.Normal);
                            fileManager.MaxIndex = FilesAndDirectories.GetMaxIndex();
                        }

                        if (Directory.Exists(fileManager.PathCopy))
                        {
                            Directory.CreateDirectory(pathPaste);
                            FilesAndDirectories.copyDirectory(fileManager.PathCopy, pathPaste);
                            fileManager.MaxIndex = FilesAndDirectories.GetMaxIndex();
                        }
                    }
                    break;
                case ConsoleKey.Tab:
                    {
                        fileManager = ProcessingCommands(fileManager);
                    }
                    break;
            }
            Properties.Settings.Default.Save();
            return fileManager;
        }

        /// <summary>
        /// Обработка команд
        /// </summary>
        /// <param name="fileManager">Информация о файловом менеджере</param>
        /// <returns></returns>
        public static FileManagerVO ProcessingCommands(FileManagerVO fileManager)
        {
            fileManager.Message = "";
            var drives = Environment.GetLogicalDrives();
            var settings = Properties.Settings.Default;
            Console.CursorVisible = true;
            string[] cmd = { };
            Console.Write("\nВведите команду: ");
            var command = Console.ReadLine().ToLower();

            if (command.Contains("cd"))
            {
                cmd = command.Split(' ');
                command = cmd[0];
            }

            switch (command)
            {
                case "doc":
                    {
                        var path = AppDomain.CurrentDomain.BaseDirectory;
                        path = Path.GetDirectoryName(path);
                        path = Path.GetDirectoryName(path);
                        path = Path.GetDirectoryName(path) + @"\Documentation.md";
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = path,
                            UseShellExecute = true
                        });
                    }
                    break;
                case "log":
                    {
                        Process.Start(new ProcessStartInfo()
                        {
                            FileName = Directory.GetCurrentDirectory() + @"\log.txt",
                            UseShellExecute = true
                        });
                    }
                    break;
                case "cd":
                    {
                        if (cmd.Length == 2)
                        {
                            var tom = $@"{cmd[1].ToUpper()}:\";
                            if (drives.Contains(tom.Substring(0, 3)))
                            {
                                settings.Path = tom;
                            }
                            else
                            {
                                fileManager.Message = $"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Диск '{cmd[1]}' не найден!";
                                HandlerException.Log(fileManager.Message);
                            }
                        }
                        else
                        {
                            fileManager.Message = $"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Команда '{string.Join(" ", cmd)}' не распознана!";
                            HandlerException.Log(fileManager.Message);
                        }
                    }
                    break;
                default:
                    {
                        fileManager.Message = $"{DateTime.Now.ToString("dd.MM.yy HH:mm")} Команда '{command}' не распознана!";
                        HandlerException.Log(fileManager.Message);
                    }
                    break;
            }
            Properties.Settings.Default.Save();
            return fileManager;
        }
    }
}
