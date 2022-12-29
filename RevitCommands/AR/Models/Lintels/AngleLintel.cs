﻿using MS.RevitCommands.AR.Enums;
using System;
using System.Collections.Generic;
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
        public double SupportLeft { get; set; }

        /// <summary>
        /// Опирание справа
        /// </summary>
        public double SupportRight { get; set; }

        /// <summary>
        /// Шаг полосы
        /// </summary>
        public double StripeStep { get; set; }

        /// <summary>
        /// Основной уголок
        /// </summary>
        public string AngleMain { get; set; }

        /// <summary>
        /// Уголок для облицовки
        /// </summary>
        public string AngleExterior { get; set; }

        /// <summary>
        /// Опорный уголок
        /// </summary>
        public string AngleSupport { get; set; }

        /// <summary>
        /// Полоса
        /// </summary>
        public string Stripe { get; set; }
    }
}
