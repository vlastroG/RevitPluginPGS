using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MS.Commands.MEP.Mechanic
{
    /// <summary>
    /// Оборудование в составе вентиляционной установки
    /// </summary>
    public abstract class Mechanic : IMechanic
    {
        /// <summary>
        /// Длина оборудования в миллиметрах
        /// </summary>
        [Range(50, 3000)]
        private protected double _length;

        /// <summary>
        /// Длина оборудования в миллиметрах
        /// </summary>
        [Range(50, 3000)]
        public double Length { get => _length; set { _length = value; } }


        /// <summary>
        /// Тип оборудования
        /// </summary>
        private protected readonly EquipmentType _equipmentType;

        /// <summary>
        /// Тип оборудования
        /// </summary>
        public EquipmentType EquipmentType => _equipmentType;


        /// <summary>
        /// ADSK_Группирование
        /// </summary>
        private protected string _grouping;

        /// <summary>
        /// ADSK_Группирование
        /// </summary>
        public string Grouping { get => _grouping; set => _grouping = value; }


        /// <summary>
        /// Конструктор оборудования вентиляционной установки
        /// </summary>
        /// <param name="equipmentType">Тип оборудования</param>
        /// <param name="length">Длина оборудования в мм</param>
        public Mechanic(EquipmentType equipmentType, double length)
        {
            _equipmentType = equipmentType;
            _length = length;
        }
    }
}
