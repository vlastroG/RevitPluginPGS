using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Mechanic.Impl
{
    /// <summary>
    /// Фильтр
    /// </summary>
    public class Filter : Mechanic
    {
        /// <summary>
        /// Конструктор фильтра
        /// </summary>
        /// <param name="length">Длина фильтра</param>
        public Filter(double length) : base(EquipmentType.Filter, length) { }


        /// <summary>
        /// PGS_ФильтрТип
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// PGS_ФильтрКоличество
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// ADSK_Сопротивление воздушного фильтра
        /// </summary>
        public double? Windage { get; set; }

        /// <summary>
        /// ADSK_Примечание
        /// </summary>
        public string Note { get; set; }

        public override Dictionary<string, string> GetNotEmptyParameters()
        {
            throw new NotImplementedException();
        }
    }
}
