using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models
{
    /// <summary>
    /// Квартира
    /// </summary>
    internal class Apartment
    {
        /// <summary>
        /// Номер квартиры
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Количество жилых комнат
        /// </summary>
        public int LivingRoomsCount { get; set; }

        /// <summary>
        /// Отпаливаемая площадь
        /// </summary>
        public double AreaHeateed { get; set; }

        /// <summary>
        /// Жилая площадь
        /// </summary>
        public double AreaLiving { get; set; }

        /// <summary>
        /// Приведенная площадь квартиры (с учетом площади неотапливаемых помещений с коэффициентом).
        /// </summary>
        public double AreaTotalCoeff { get; set; }

        /// <summary>
        /// Общая площадь квартиры без коэффициентов
        /// </summary>
        private double AreaTotal { get; set; }   
    }
}
