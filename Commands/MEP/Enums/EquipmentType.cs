using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Enums
{
    /// <summary>
    /// Типы оборудования в вентиляционной установке
    /// </summary>
    public enum EquipmentType
    {
        /// <summary>
        /// Вентилятор
        /// </summary>
        Fan,
        /// <summary>
        /// Аоздухоохладитель
        /// </summary>
        AirCooler,
        /// <summary>
        /// Воздухонагреватель
        /// </summary>
        AirHeater,
        /// <summary>
        /// Фильтр
        /// </summary>
        Filter
    } 
}
