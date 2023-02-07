using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.BIM.DTO
{
    /// <summary>
    /// DTO для экспорта спецификаций в CSV при помощи флага
    /// </summary>
    public class ScheduleToCSVExportDto
    {
        /// <summary>
        /// Видовое окно спецификации
        /// </summary>
        public ViewSchedule ViewSchedule { get; }

        /// <summary>
        /// Название спецификации
        /// </summary>
        public string Name { get => ViewSchedule.Name; }

        /// <summary>
        /// Экспортировать спецификацию в CSV или нет
        /// </summary>
        public bool Export { get; set; }

        /// <summary>
        /// Конструктор DTO для экспорта спецификации
        /// </summary>
        /// <param name="viewSchedule"></param>
        public ScheduleToCSVExportDto(ViewSchedule viewSchedule)
        {
            ViewSchedule = viewSchedule;
            Export = false;
        }

        /// <summary>
        /// Экспортировать в CSV спецификацию
        /// </summary>
        /// <param name="folder">Папка, в которую производится экспорт</param>
        /// <param name="options">Настройки экспорта</param>
        public void ExportToCSV(
            string folder,
            ViewScheduleExportOptions options)
        {
            if (!Export) return;
            ViewSchedule.Export(folder, ViewSchedule.Name, options);
        }
    }
}
