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
        /// Название ключевого параметра помещения для назначения отделки стен и потолка - "Стиль комнат"
        /// </summary>
        public static readonly string RoomsFinishingWallsAndCeiling = "Стиль комнат";

        /// <summary>
        /// Название ключевого параметра помещения для назначения отделки пола - "Тип отделки пола"
        /// </summary>
        public static readonly string RoomsFinishingFloor = "Тип отделки пола";

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
