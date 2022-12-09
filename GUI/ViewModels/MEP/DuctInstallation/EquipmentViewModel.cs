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
    /// Модель представления данных для оборудования внутри вентиляционной установки
    /// </summary>
    public class EquipmentViewModel : ViewModelBase
    {
        /// <summary>
        /// Тип оборудования
        /// </summary>
        private EquipmentType _type;

        /// <summary>
        /// Тип оборудования
        /// </summary>
        public EquipmentType Type
        {
            get => _type;
            set => Set(ref _type, value);
        }

        /// <summary>
        /// Длина оборудования
        /// </summary>
        private double _length;

        /// <summary>
        /// Длина оборудования
        /// </summary>
        public double Length
        {
            get => _length;
            set => Set(ref _length, value);
        }
    }
}
