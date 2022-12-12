using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        /// <summary>
        /// ADSK_Марка
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// PGS_ТипоразмерВентилятора
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ADSK_Расход воздуха
        /// </summary>
        public double AirFlow { get; set; }

        /// <summary>
        /// ADSK_Потеря давления воздуха
        /// </summary>
        public double AirPressureLoss { get; set; }

        /// <summary>
        /// ADSK_Частота вращения вентилятора
        /// </summary>
        public int FanSpeed { get; set; }

        /// <summary>
        /// PGS_ЭлектродвигательТипИсполненияПоВзрывозащите
        /// </summary>
        public string ExplosionProofType { get; set; }

        /// <summary>
        /// ADSK_Номинальная мощность
        /// </summary>
        public double RatedPower { get; set; }

        /// <summary>
        /// ADSK_Частота вращения двигателя
        /// </summary>
        public int EngineSpeed { get; set; }
    }
}
