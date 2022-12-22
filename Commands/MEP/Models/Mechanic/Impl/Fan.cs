using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MS.Utilites.Reflection;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MS.Commands.MEP.Mechanic.Impl
{
    /// <summary>
    /// Вентилятор
    /// </summary>
    public class Fan : Mechanic
    {
        /// <summary>
        /// Конструктор вентилятора по длине
        /// </summary>
        /// <param name="length">Длина вентилятора в мм</param>
        public Fan(double length) : base(EquipmentType.Fan, length) { }

        [JsonConstructor]
        public Fan(Guid guid, double length) : base(EquipmentType.Fan, length, guid) { }

        /// <summary>
        /// ADSK_Марка
        /// </summary>
        [Description("ADSK_Марка")]
        public string Mark { get; set; }

        /// <summary>
        /// PGS_ТипоразмерВентилятора
        /// </summary>
        [Description("PGS_ТипоразмерВентилятора")]
        public string Type { get; set; }

        /// <summary>
        /// ADSK_Расход воздуха
        /// </summary>
        [Description("ADSK_Расход воздуха")]
        public double? AirFlow { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха
        /// </summary>
        [Description("ADSK_Потеря давления воздуха")]
        public double? AirPressureLoss { get; set; }

        /// <summary>
        /// ADSK_Частота вращения вентилятора
        /// </summary>
        [Description("ADSK_Частота вращения вентилятора")]
        public int? FanSpeed { get; set; }

        /// <summary>
        /// PGS_ЭлектродвигательТипИсполненияПоВзрывозащите
        /// </summary>
        [Description("PGS_ЭлектродвигательТипИсполненияПоВзрывозащите")]
        public string ExplosionProofType { get; set; }

        /// <summary>
        /// ADSK_Номинальная мощность
        /// </summary>
        [Description("ADSK_Номинальная мощность")]
        public double? RatedPower { get; set; }

        /// <summary>
        /// ADSK_Частота вращения двигателя
        /// </summary>
        [Description("ADSK_Частота вращения двигателя")]
        public int? EngineSpeed { get; set; }

        /// <summary>
        /// ADSK_Количество
        /// </summary>
        [Description("ADSK_Количество")]
        public double? Count { get; set; }
    }
}
