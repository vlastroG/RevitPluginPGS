using MS.RevitCommands.MEP.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP.Mechanic.Impl
{
    /// <summary>
    /// Воздухоохладитель
    /// </summary>
    public class Cooler : Mechanic
    {
        /// <summary>
        /// Конструктор воздухоохладителя
        /// </summary>
        /// <param name="length">Длина воздухоохладителя в мм</param>
        public Cooler(double length) : base(EquipmentType.AirCooler, length) { }

        [JsonConstructor]
        public Cooler(Guid guid, double length) : base(EquipmentType.AirCooler, length, guid) { }


        /// <summary>
        /// PGS_ВоздухоохладительТип
        /// </summary>
        [Description("PGS_ВоздухоохладительТип")]
        public string Type { get; set; }

        /// <summary>
        /// PGS_ВоздухоохладительМощность
        /// </summary>
        [Description("PGS_ВоздухоохладительМощность")]
        public double? Power { get; set; }

        /// <summary>
        /// PGS_ВоздухоохладительКоличество
        /// </summary>
        [Description("PGS_ВоздухоохладительКоличество")]
        public double? Count { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на входе в охладитель
        /// </summary>
        [Description("ADSK_Температура воздуха на входе в охладитель")]
        public double? TemperatureIn { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на выходе из охладителя
        /// </summary>
        [Description("ADSK_Температура воздуха на выходе из охладителя")]
        public double? TemperatureOut { get; set; }

        /// <summary>
        /// ADSK_Холодильная мощность
        /// </summary>
        [Description("ADSK_Холодильная мощность")]
        public double? PowerCool { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха в охладителе
        /// </summary>
        [Description("ADSK_Потеря давления воздуха в охладителе")]
        public double? AirPressureLoss { get; set; }
    }
}
