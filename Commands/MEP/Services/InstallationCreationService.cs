using MS.Commands.MEP.Models.Installation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Services
{
    /// <summary>
    /// Сервис для работы с установкой
    /// </summary>
    public static class InstallationCreationService
    {
        /// <summary>
        /// Сериализует объект установки в JSON в указанную директорию с названием, соответствующим названию системы.
        /// </summary>
        /// <param name="installation">Сериализуемая установка</param>
        /// <param name="dirPath">Путь к директории</param>
        public static string SerializeInstallation(Installation installation, string @dirPath)
        {
            string path = @dirPath + $@"\{installation.System}.json";
            string jsonString = JsonConvert.SerializeObject(installation);
            File.WriteAllText(path, jsonString);
            return path;
        }

        /// <summary>
        /// Десериализует установку из указанного файла
        /// </summary>
        /// <param name="filePath">JSON файл установки</param>
        /// <returns>Десериализованный объект установки</returns>
        public static Installation DeserializeInstallation(string @filePath)
        {
            string jsonString = File.ReadAllText(@filePath);
            return JsonConvert.DeserializeObject<Installation>(jsonString);
        }
    }
}
