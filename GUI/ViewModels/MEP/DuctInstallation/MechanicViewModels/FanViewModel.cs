using MS.Commands.MEP.Mechanic.Impl;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    /// <summary>
    /// Модель представления вентилятора
    /// </summary>
    public class FanViewModel : ViewModelBase, IDataErrorInfo
    {
        public FanViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели представления вентилятора по заданному вентилятору
        /// </summary>
        /// <param name="fan"></param>
        public FanViewModel(Fan fan)
        {
            AirFlow = fan.AirFlow;
            AirPressureLoss = fan.AirPressureLoss;
            Count = fan.Count;
            EngineSpeed = fan.EngineSpeed;
            ExplosionProofType = fan.ExplosionProofType;
            FanSpeed = fan.FanSpeed;
            Mark = fan.Mark;
            RatedPower = fan.RatedPower;
            Type = fan.Type;
            Length = fan.Length;
        }


        private string _Mark;

        /// <summary>
        /// ADSK_Марка
        /// </summary>
        public string Mark { get => _Mark; set => Set(ref _Mark, value); }


        private string _Type;

        /// <summary>
        /// PGS_ТипоразмерВентилятора
        /// </summary>
        public string Type { get => _Type; set => Set(ref _Type, value); }


        private double? _AirFlow;

        /// <summary>
        /// ADSK_Расход воздуха
        /// </summary>
        public double? AirFlow { get => _AirFlow; set => Set(ref _AirFlow, value); }


        private double? _AirPressureLoss;

        /// <summary>
        /// ADSK_Потеря давления воздуха
        /// </summary>
        public double? AirPressureLoss { get => _AirPressureLoss; set => Set(ref _AirPressureLoss, value); }


        private int? _FanSpeed;

        /// <summary>
        /// ADSK_Частота вращения вентилятора
        /// </summary>
        public int? FanSpeed { get => _FanSpeed; set => Set(ref _FanSpeed, value); }


        private string _ExplosionProofType;

        /// <summary>
        /// PGS_ЭлектродвигательТипИсполненияПоВзрывозащите
        /// </summary>
        public string ExplosionProofType { get => _ExplosionProofType; set => Set(ref _ExplosionProofType, value); }


        private double? _RatedPower;

        /// <summary>
        /// ADSK_Номинальная мощность
        /// </summary>
        public double? RatedPower { get => _RatedPower; set => Set(ref _RatedPower, value); }


        private int? _EngineSpeed;

        /// <summary>
        /// ADSK_Частота вращения двигателя
        /// </summary>
        public int? EngineSpeed { get => _EngineSpeed; set => Set(ref _EngineSpeed, value); }


        private double? _Count;

        /// <summary>
        /// ADSK_Количество
        /// </summary>
        public double? Count { get => _Count; set => Set(ref _Count, value); }


        /// <summary>
        /// Длина
        /// </summary>
        private double _length;

        /// <summary>
        /// Длина
        /// </summary>
        public double Length { get => _length; set => Set(ref _length, value); }

        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Count":
                        if ((Count != null) && (Count < 0))
                        {
                            error = "Количество должно быть >= 0";
                        }
                        break;
                }
                return error;
            }
        }

        public Fan GetFan()
        {
            return GetFan(Guid.NewGuid());
        }

        public Fan GetFan(Guid guid)
        {
            return new Fan(guid, Length)
            {
                AirFlow = AirFlow,
                Count = Count,
                AirPressureLoss = AirPressureLoss,
                EngineSpeed = EngineSpeed,
                ExplosionProofType = ExplosionProofType,
                FanSpeed = FanSpeed,
                Mark = Mark,
                RatedPower = RatedPower,
                Type = Type
            };
        }
    }
}
