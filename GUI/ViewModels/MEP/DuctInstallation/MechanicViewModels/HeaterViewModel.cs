using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    /// <summary>
    /// Модель представления воздухонагревателя
    /// </summary>
    public class HeaterViewModel : ViewModelBase
    {
        public string _Type;
        /// <summary>
        /// PGS_ВоздухонагревательТип
        /// </summary>
        public string Type { get => _Type; set => Set(ref _Type, value); }


        public double? _Power;
        /// <summary>
        /// PGS_ВоздухонагревательМощность
        /// </summary>
        public double? Power { get => _Power; set => Set(ref _Power, value); }


        public double? _Count;
        /// <summary>
        /// PGS_ВоздухонагревательКоличество
        /// </summary>
        public double? Count { get => _Count; set => Set(ref _Count, value); }


        public double? _TemperatureIn;
        /// <summary>
        /// ADSK_Температура воздуха на входе в нагреватель
        /// </summary>
        public double? TemperatureIn { get => _TemperatureIn; set => Set(ref _TemperatureIn, value); }


        public double? _TemperatureOut;
        /// <summary>
        /// ADSK_Температура воздуха на выходе из нагревателя
        /// </summary>
        public double? TemperatureOut { get => _TemperatureOut; set => Set(ref _TemperatureOut, value); }


        public double? _PowerHeat;
        /// <summary>
        /// ADSK_Тепловая мощность
        /// </summary>
        public double? PowerHeat { get => _PowerHeat; set => Set(ref _PowerHeat, value); }


        public double? _AirPressureLoss;
        /// <summary>
        /// ADSK_Потеря давления воздуха в нагревателе
        /// </summary>
        public double? AirPressureLoss { get => _AirPressureLoss; set => Set(ref _AirPressureLoss, value); }
    }
}
