using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    /// <summary>
    /// DTO для компоновки пар значений PGS_НаименованиеОтделки и Типоразмера отделочной стены
    /// </summary>
    public class WallTypeFinishingDto
    {
        /// <summary>
        /// Значение параметра PGS_НаименованиеОтделки
        /// </summary>
        public string FinishingName { get; }

        /// <summary>
        /// Типоразмер отделочной стены
        /// </summary>
        public WallType WallType { get; set; }

        /// <summary>
        /// Конструктор DTO
        /// </summary>
        /// <param name="FinishingName">Значение параметра PGS_НаименованиеОтделки</param>
        public WallTypeFinishingDto(string FinishingName)
        {
            this.FinishingName = FinishingName;
        }
    }
}
