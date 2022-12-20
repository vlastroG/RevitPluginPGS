using MS.Commands.MEP.Enums;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    public class ChooseMechanicTypeViewModel : ViewModelBase
    {
        /// <summary>
        /// Массив названий доступных типов оборудвоания
        /// </summary>
        public string[] AvailableTypes
        {
            get
            {
                return new string[4]
                {
                    "Воздухоохладитель",
                    "Воздухонагреватель",
                    "Фильтр",
                    "Вентилятор"
                };
            }
        }

        /// <summary>
        /// Выбранное оборудование
        /// </summary>
        private string _selectedMechanicType;

        /// <summary>
        /// Выбранное оборудование
        /// </summary>
        public string SelectedMechanicType
        {
            get => _selectedMechanicType;
            set => Set(ref _selectedMechanicType, value);
        }

        /// <summary>
        /// Выбранный тип оборудования
        /// </summary>
        public EquipmentType EquipmentType
        {
            get
            {
                switch (_selectedMechanicType)
                {
                    case "Воздухоохладитель":
                        return EquipmentType.AirCooler;
                    case "Воздухонагреватель":
                        return EquipmentType.AirHeater;
                    case "Фильтр":
                        return EquipmentType.Filter;
                    case "Вентилятор":
                        return EquipmentType.Fan;
                    default:
                        return EquipmentType.Fan;
                };
            }
        }
    }
}
