using MS.Commands.MEP.Models.Installation;
using MS.Utilites;
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
        /// Стартовая директория для работы с сериализованными установками
        /// </summary>
        private static string @_serializationStartPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


        /// <summary>
        /// Сериализует объект установки в JSON в указанную директорию с названием, соответствующим названию системы.
        /// </summary>
        /// <param name="installation">Сериализуемая установка</param>
        public static string SerializeInstallation(Installation installation)
        {
            var filePath = WorkWithPath.GetFilePath(
                ref @_serializationStartPath,
                "Json файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt",
                "Перейдите в папку и напишите название файла без расширения",
                string.Empty);

            string jsonString = JsonConvert.SerializeObject(installation);
            File.WriteAllText(filePath, jsonString);
            return filePath;
        }


        /// <summary>
        /// Десериализует установку из указанного файла
        /// </summary>
        /// <param name="filePath">JSON файл установки</param>
        /// <returns>Десериализованный объект установки</returns>
        public static Installation DeserializeInstallation()
        {
            var filePath = $@"{WorkWithPath.GetFilePath(
                ref @_serializationStartPath,
                "Json файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt",
                "Выберите Json файл с вентиляционной установкой",
                string.Empty)}";

            string jsonString = File.ReadAllText(@filePath);
            return JsonConvert.DeserializeObject<Installation>(jsonString);
        }
    }
}
