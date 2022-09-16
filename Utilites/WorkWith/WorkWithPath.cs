using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace MS.Utilites
{
    public static class WorkWithPath
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Создать временную папку. Если папка существует,
        /// то она предварительно удалится рекурсивно, а затем создастся заново (пустая).
        /// </summary>
        /// <param name="dirPath">Путь к папке</param>
        /// <returns>Временная папка</returns>
        public static DirectoryInfo CreateTempDir(string @dirPath)
        {
            if (Directory.Exists(@dirPath))
            {
                Directory.Delete(@dirPath, true);
            }
            DirectoryInfo temporaryFolder = Directory.CreateDirectory(@dirPath);
            return temporaryFolder;
        }

        /// <summary>
        /// Выводит диалоговое окно для выбора файла заданного расширения и получения полного пути к нему.
        /// В случае отмены или выбора пользователем неправильного расширения возвращается пустая строка.
        /// </summary>
        /// <param name="StartPath">Стартовая директория</param>
        /// <param name="FileFilter">Фильтр для файлов</param>
        /// <param name="Tittle">Заголовок окна</param>
        /// <param name="MustEndsWith">Расширение, которое должно быть у выбранного файла</param>
        /// <returns></returns>
        public static string GetPath(ref string StartPath, string FileFilter, string Tittle, string MustEndsWith)
        {
            string path = String.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = StartPath,
                Filter = FileFilter,
                Multiselect = false,
                RestoreDirectory = true,
                Title = Tittle
            };
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
                FileInfo file = new FileInfo(path);
                StartPath = file.DirectoryName;
            }
            else
            {
                return String.Empty;
            }
            if (!path.EndsWith(MustEndsWith))
            {
                MessageBox.Show("Неверный формат файла!", "Ошибка");
                return String.Empty;
            }
            return path;
        }
    }
}
