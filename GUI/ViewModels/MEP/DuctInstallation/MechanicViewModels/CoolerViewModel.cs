using MS.Commands.MEP.Enums;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    /// <summary>
    /// Модель представления данных для воздухоохладителя
    /// </summary>
    public class CoolerViewModel : ViewModelBase
    {
        /// <summary>
        /// Название типа оборудования
        /// </summary>
        public string Name { get => "Воздухоохладитель"; }


        private string _type;

        /// <summary>
        /// PGS_ВоздухоохладительТип
        /// </summary>
        public string Type { get => _type; set => Set(ref _type, value); }


        private double? _power;

        /// <summary>
        /// PGS_ВоздухоохладительМощность
        /// </summary>
        public double? Power { get => _power; set => Set(ref _power, value); }


        private double? _count;

        /// <summary>
        /// PGS_ВоздухоохладительКоличество
        /// </summary>
        public double? Count { get => _count; set => Set(ref _count, value); }


        private double? _temperatureIn;

        /// <summary>
        /// ADSK_Температура воздуха на входе в охладитель
        /// </summary>
        public double? TemperatureIn { get => _temperatureIn; set => Set(ref _temperatureIn, value); }


        private double? _temperatureOut;

        /// <summary>
        /// ADSK_Температура воздуха на выходе из охладителя
        /// </summary>
        public double? TemperatureOut { get => _temperatureOut; set => Set(ref _temperatureOut, value); }


        private double? _powerCool;

        /// <summary>
        /// ADSK_Холодильная мощность
        /// </summary>
        public double? PowerCool { get => _powerCool; set => Set(ref _powerCool, value); }


        private double? _airPressureLoss;

        /// <summary>
        /// ADSK_Потеря давления воздуха в охладителе
        /// </summary>
        public double? AirPressureLoss { get => _airPressureLoss; set => Set(ref _airPressureLoss, value); }


        /// <summary>
        /// Длина
        /// </summary>
        private double _length;

        /// <summary>
        /// Длина
        /// </summary>
        public double Length { get => _length; set => Set(ref _length, value); }
    }
}
