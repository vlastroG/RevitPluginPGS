using System;
using System.IO;
using System.Reflection;

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
    }
}
