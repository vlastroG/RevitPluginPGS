using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP.Enums
{
    public static class EquipmentTypeExtension
    {
        /// <summary>
        /// Преобразует тип перечисления оборудования в строку
        /// </summary>
        /// <param name="equipmentType"></param>
        /// <returns></returns>
        public static string GetName(EquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case EquipmentType.AirCooler:
                    return "Воздухоохладитель-";
                case EquipmentType.AirHeater:
                    return "Воздухонагреватель";
                case EquipmentType.Filter:
                    return "Фильтр------------";
                case EquipmentType.Fan:
                    return "Вентилятор--------";
                default:
                    return string.Empty;
            }
        }
    }
}
