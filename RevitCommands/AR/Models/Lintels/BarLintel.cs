using MS.RevitCommands.AR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public BarLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Bar, existLintelId) { }

        /// <summary>
        /// Диаметр арматурных стержней в мм
        /// </summary>
        [Description("ADSK_Размер_Диаметр")]
        public double BarsDiameter { get; set; } = 12;

        /// <summary>
        /// Опирание слева в мм
        /// </summary>
        [Description("Опирание слева")]
        public double SupportLeft { get; set; } = 250;

        /// <summary>
        /// Опирание справа в мм
        /// </summary>
        [Description("Опирание справа")]
        public double SupportRight { get; set; } = 250;

        /// <summary>
        /// Шаг стержней в мм
        /// </summary>
        [Description("Шаг стержней")]
        public double BarsStep { get; set; } = 60;
    }
}
