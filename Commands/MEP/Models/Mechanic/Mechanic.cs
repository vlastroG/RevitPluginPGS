using MS.Commands.MEP.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.ComponentModel;

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
        /// Конструктор оборудования вентиляционной установки
        /// </summary>
        /// <param name="equipmentType">Тип оборудования</param>
        /// <param name="length">Длина оборудования в мм</param>
        public Mechanic(EquipmentType equipmentType, double length)
        {
            _equipmentType = equipmentType;
            _length = length;
        }

        /// <summary>
        /// Возвращает словарь названий параметров оборудования и их значений (значения заполненных свойств оборудования)
        /// </summary>
        /// <returns>Словарь заполненных параметров и их значений</returns>
        public virtual Dictionary<string, dynamic> GetNotEmptyParameters()
        {
            Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var value = property.GetValue(this);
                if (!(value is null))
                {
                    var description = ((DescriptionAttribute)property
                        .GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
                    if (!(description is null))
                    {
                        parameters.Add(description, value);
                    }
                }
            }

            return parameters;
        }
    }
}
