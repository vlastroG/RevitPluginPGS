using MS.RevitCommands.AR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.Models.Lintels
{
    /// <summary>
    /// Перемычка из арматурных стержней
    /// </summary>
    public class BarLintel : Lintel
    {
        /// <summary>
        /// Конструктор перемычки по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public BarLintel(Guid guid) : base(guid, LintelType.Bar) { }

        /// <summary>
        /// Диаметр арматурных стержней в мм
        /// </summary>
        public double BarsDiameter { get; set; }

        /// <summary>
        /// Опирание слева в мм
        /// </summary>
        public double SupportLeft { get; set; }

        /// <summary>
        /// Опирание справа в мм
        /// </summary>
        public double SupportRight { get; set; }

        /// <summary>
        /// Шаг стержней в мм
        /// </summary>
        public double BarsStep { get; set; }
    }
}
