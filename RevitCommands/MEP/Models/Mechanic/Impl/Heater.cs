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
    /// Воздухонагреватель
    /// </summary>
    public class Heater : Mechanic
    {
        /// <summary>
        /// Конструктор воздухонагревателя по длине
        /// </summary>
        /// <param name="length">Длина воздухонагревателя в миллиметрах</param>
        public Heater(double length) : base(EquipmentType.AirHeater, length) { }

        [JsonConstructor]
        public Heater(Guid guid, double length) : base(EquipmentType.AirHeater, length, guid) { }

        /// <summary>
        /// PGS_ВоздухонагревательТип
        /// </summary>
        [Description("PGS_ВоздухонагревательТип")]
        public string Type { get; set; }

        /// <summary>
        /// PGS_ВоздухонагревательМощность
        /// </summary>
        [Description("PGS_ВоздухонагревательМощность")]
        public double? Power { get; set; }

        /// <summary>
        /// PGS_ВоздухонагревательКоличество
        /// </summary>
        [Description("PGS_ВоздухонагревательКоличество")]
        public double? Count { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на входе в нагреватель
        /// </summary>
        [Description("ADSK_Температура воздуха на входе в нагреватель")]
        public double? TemperatureIn { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на выходе из нагревателя
        /// </summary>
        [Description("ADSK_Температура воздуха на выходе из нагревателя")]
        public double? TemperatureOut { get; set; }

        /// <summary>
        /// ADSK_Тепловая мощность
        /// </summary>
        [Description("ADSK_Тепловая мощность")]
        public double? PowerHeat { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха в нагревателе
        /// </summary>
        [Description("ADSK_Потеря давления воздуха в нагревателе")]
        public double? AirPressureLoss { get; set; }
    }
}
