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
    /// Перемычка из уголков
    /// </summary>
    public class AngleLintel : Lintel
    {
        /// <summary>
        /// Конструктор перемычки из уголков по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public AngleLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Angle, existLintelId) { }

        /// <summary>
        /// Опирание слева
        /// </summary>
        [Description("уголок_опирание_1")]
        public double SupportLeft { get; set; } = 250;

        /// <summary>
        /// Опирание справа
        /// </summary>
        [Description("уголок_опирание_2")]
        public double SupportRight { get; set; } = 250;

        /// <summary>
        /// Шаг полосы
        /// </summary>
        [Description("Полоса_Шаг")]
        public double StripeStep { get; set; } = 500;

        /// <summary>
        /// Основной уголок
        /// </summary>
        [Description("Внутренний уголок")]
        public string AngleMain { get; set; } = "75x5";

        /// <summary>
        /// Уголок для облицовки
        /// </summary>
        [Description("Уголок для облицовки")]
        public string AngleExterior { get; set; } = "75x5";

        /// <summary>
        /// Опорный уголок
        /// </summary>
        [Description("Опорные уголки")]
        public string AngleSupport { get; set; } = "100x10";

        /// <summary>
        /// Полоса
        /// </summary>
        [Description("Полоса")]
        public string Stripe { get; set; } = "5x50";
    }
}
