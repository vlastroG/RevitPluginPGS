using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Mechanic.Impl
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

        /// <summary>
        /// PGS_ВоздухоохладительТип
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// PGS_ВоздухоохладительМощность
        /// </summary>
        public double? Power { get; set; }

        /// <summary>
        /// PGS_ВоздухоохладительКоличество
        /// </summary>
        public double? Count { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на входе в охладитель
        /// </summary>
        public double? TemperatureIn { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на выходе из охладителя
        /// </summary>
        public double? TemperatureOut { get; set; }

        /// <summary>
        /// ADSK_Холодильная мощность 
        /// </summary>
        public double? PowerCool { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха в охладителе
        /// </summary>
        public double? AirPressureLoss { get; set; }

        public override Dictionary<string, string> GetNotEmptyParameters()
        {
            throw new NotImplementedException();
        }
    }
}
