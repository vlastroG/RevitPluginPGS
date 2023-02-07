﻿using MS.RevitCommands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP.Mechanic
{
    /// <summary>
    /// Оборудование вентиляционной установки
    /// </summary>
    public interface IMechanic
    {
        /// <summary>
        /// Длина оборудования в миллиметрах
        /// </summary>
        double Length { get; set; }

        /// <summary>
        /// Тип оборудования
        /// </summary>
        EquipmentType EquipmentType { get; }
    }
}
