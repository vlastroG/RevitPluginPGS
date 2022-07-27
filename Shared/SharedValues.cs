using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Shared
{
    public static class SharedValues
    {
        /// <summary>
        /// Значение параметра "Описание" в типе семейств перемычек - "Перемычка"
        /// </summary>
        public static readonly string LintelDescription = "Перемычка";

        /// <summary>
        /// Префикс марки перемычки "ПР-"
        /// </summary>
        public static readonly string LintelMarkPrefix = "ПР-";

        /// <summary>
        /// Коэффициент для перевода квадратных футов в квадратные метры
        /// </summary>
        public static readonly double SqFeetToMeters = 0.3048 * 0.3048;

        /// <summary>
        /// Коэффициент для перевода футов в миллиметры
        /// </summary>
        public static readonly double FootToMillimeters = 304.8;
    }
}
