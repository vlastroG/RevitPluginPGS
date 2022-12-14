using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Mechanic.Impl
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


        /// <summary>
        /// PGS_ВоздухонагревательТип
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// PGS_ВоздухонагревательМощность
        /// </summary>
        public double? Power { get; set; }

        /// <summary>
        /// PGS_ВоздухонагревательКоличество
        /// </summary>
        public double? Count { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на входе в нагреватель
        /// </summary>
        public double? TemperatureIn { get; set; }

        /// <summary>
        /// ADSK_Температура воздуха на выходе из нагревателя
        /// </summary>
        public double? TemperatureOut { get; set; }

        /// <summary>
        /// ADSK_Тепловая мощность 
        /// </summary>
        public double? PowerHeat { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха в нагревателе
        /// </summary>
        public double? AirPressureLoss { get; set; }

        public override Dictionary<string, dynamic> GetNotEmptyParameters()
        {
            throw new NotImplementedException();
        }
    }
}
