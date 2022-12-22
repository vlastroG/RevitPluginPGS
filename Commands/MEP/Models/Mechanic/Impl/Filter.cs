using MS.Commands.MEP.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [JsonConstructor]
        public Filter(Guid guid, double length) : base(EquipmentType.Filter, length, guid) { }

        /// <summary>
        /// PGS_ФильтрТип
        /// </summary>
        [Description("PGS_ФильтрТип")]
        public string Type { get; set; }

        /// <summary>
        /// PGS_ФильтрКоличество
        /// </summary>
        [Description("PGS_ФильтрКоличество")]
        public int? Count { get; set; }

        /// <summary>
        /// ADSK_Сопротивление воздушного фильтра
        /// </summary>
        [Description("ADSK_Сопротивление воздушного фильтра")]
        public double? Windage { get; set; }

        /// <summary>
        /// ADSK_Примечание
        /// </summary>
        [Description("ADSK_Примечание")]
        public string Note { get; set; }
    }
}
