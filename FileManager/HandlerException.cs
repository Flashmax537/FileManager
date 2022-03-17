using System.IO;

namespace FileManager
{
    class HandlerException
    {
        /// <summary>
        /// Запись текста ошибки в лог
        /// </summary>
        /// <param name="message">Текст ошибки</param>
        public static void Log(string message)
        {
            File.AppendAllText("log.txt", message + "\n");
        }
    }
}
