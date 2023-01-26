using MS.Utilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Logging
{
    public static class Logger
    {
        private static readonly string _logPath
            = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PGS-BIM\Logs\";

        /// <summary>
        /// Записывает массив строк в текстовый лог файл с заданным названием, прибавляя дату
        /// </summary>
        /// <param name="logName">Название (префикс) лог файла</param>
        /// <param name="logMessage">Сообщение для логгирования</param>
        /// <param name="openLog">True, если открывать лог файл, False, если нет</param>
        public static void WriteLog(string logName, string[] logMessage, bool openLog)
        {
            Directory.CreateDirectory(_logPath);
            var logFile = Path.Combine(_logPath, logName + "_" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".txt");
            File.WriteAllLines(logFile, logMessage);
            if (openLog)
            {
                PathMethods.OpenWithDefaultProgram(logFile);
            }
        }

        /// <summary>
        /// Записывает массив строк в текстовый лог файл с заданным названием, прибавляя дату
        /// </summary>
        /// <param name="logName">Название (префикс) лог файла</param>
        /// <param name="logMessage">Сообщение для логгирования</param>
        /// <param name="openLog">True, если открывать лог файл, False, если нет</param>
        public static void WriteLog(string logName, string logMessage, bool openLog)
        {
            WriteLog(logName, new string[] { logMessage }, openLog);
        }
    }
}
