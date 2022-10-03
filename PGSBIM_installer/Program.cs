using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace PGSBIM_installer
{
    internal class Program
    {
        private static string _pluginDir = @"\\dsm\rvt\!Ресурсы\!PGS-BIM_Plugin\PGS-BIM";

        //private static string _destDir = @"%ProgramData%\Autodesk\Revit\Addins\2022";

        private static string _destDir = @"C:\ProgramData\Autodesk\Revit\Addins\2022";

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"--> Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"--> Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"--> Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"--> Renamed:");
            Console.WriteLine($"-->     Old: {e.OldFullPath}");
            Console.WriteLine($"-->     New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"--> Message: {ex.Message}");
                Console.WriteLine("--> Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
                Console.ReadKey(true);
            }
        }


        static void Main(string[] args)
        {
            bool destDirExist = Directory.Exists(_destDir);
            while (!destDirExist)
            {
                Console.WriteLine(@"Перейдите в проводнике в папку по пути '%ProgramData%\Autodesk\Revit\Addins\2022'" +
                                    "\nСкопируйте полный путь, отображающийся в проводнике и вставьте ниже:");
                Console.Write("-->");
                _destDir = Console.ReadLine().TrimStart();
                Console.WriteLine();
                destDirExist = Directory.Exists(_destDir);
            }
            Console.WriteLine("Установка плагина PGS-BIM...\n");
            using (var watcher = new FileSystemWatcher(_destDir))
            {
                watcher.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
                bool exceptionThrown = false;
                try
                {
                    CopyFilesRecursively(_pluginDir, _destDir);
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                    Console.WriteLine();
                    Console.WriteLine("Ошибка: закройте Revit.");
                    Console.WriteLine("Нажмите Enter для выхода из установщика");
                    Console.ReadKey(true);
                }
                if (!exceptionThrown)
                {
                    Task.Delay(500).Wait();
                    Console.WriteLine();
                    Console.WriteLine("Готово, нажмите Enter");
                    Console.ReadKey(true);
                }
            }
        }
    }
}
