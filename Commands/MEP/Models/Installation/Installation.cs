using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Models.Installation
{
    public class Installation
    {
        /// <summary>
        /// Список болванок семейств оборудования внутри родительского семейства вентиляционной установки.
        /// Болванки представлены списками оборудования
        /// </summary>
        private readonly List<List<Mechanic.Mechanic>> _mechanics = new List<List<Mechanic.Mechanic>>() {
            new List<Mechanic.Mechanic>()
        };

        /// <summary>
        /// Список болванок семейств наполнения внутри родительского семейства вентиляционной установки.
        /// </summary>
        private readonly List<Filling> _fillings = new List<Filling>();

        /// <summary>
        /// Максимальное количество семкейств-болванок в родительском семействе установки
        /// </summary>
        private const int _levelsCapacity = 40;

        /// <summary>
        /// Ширина установки в мм
        /// </summary>
        private double _width;

        /// <summary>
        /// Высота установки в мм
        /// </summary>
        private double _height;

        /// <summary>
        /// Длина установки в мм
        /// </summary>
        private double _length;

        /// <summary>
        /// ADSK_Группирование
        /// </summary>
        private string _groupingParent = "-1";

        /// <summary>
        /// Вложенное_наполнение_группирование
        /// </summary>
        private string _groupingBoldFilling = "-2";

        /// <summary>
        /// Вложенное_оборудование_группирование
        /// </summary>
        private string _groupingBoldMechanic = "-3";


        /// <summary>
        /// Конструктор установки по габаритным размерам
        /// </summary>
        /// <param name="width">Ширина в мм</param>
        /// <param name="height">Высота в мм</param>
        /// <param name="length">Длина в мм</param>
        public Installation(double width, double height, double length)
        {
            _width = width;
            _height = height;
            _length = length;
        }


        /// <summary>
        /// Ширина установки в мм
        /// </summary>
        [Range(100, 2000)]
        public double Width { get => _width; set => _width = value; }


        /// <summary>
        /// Высота установки в мм
        /// </summary>
        [Range(100, 2000)]
        public double Height { get => _height; set => _height = value; }


        /// <summary>
        /// Длина установки в мм
        /// </summary>
        [Range(100, 10000)]
        public double Length { get => _length; set => _length = value; }

        /// <summary>
        /// ADSK_Группирование
        /// </summary>
        public string GroupingParent { get => _groupingParent; }

        /// <summary>
        /// Вложенное_оборудование_группирование
        /// </summary>
        public string GroupingMechanic { get => _groupingBoldMechanic; }

        /// <summary>
        /// Вложенное_наполнение_группирование
        /// </summary>
        public string GroupingFilling { get => _groupingBoldFilling; }

        /// <summary>
        /// PGS_ТипУстановки
        /// </summary>
        public string Type { get; set; }


        /// <summary>
        /// ADSK_Наименование краткое
        /// </summary>
        public string NameShort { get; set; }

        /// <summary>
        /// ADSK_Наименование
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Префикс наименования (сокращенное название системы)
        /// </summary>
        public string System { get; set; }


        /// <summary>
        /// Добавляет оборудование в компоновку вентиляционной установки
        /// </summary>
        /// <param name="mechanic"></param>
        public void AddMechanic(Mechanic.Mechanic mechanic)
        {
            var mechanicType = mechanic.GetType();
            bool isAdded = false;
            for (int i = 0; !isAdded; i++)
            {
                if (!(_mechanics[i].FirstOrDefault(m => m.GetType().Equals(mechanicType)) is null))
                {
                    if (_mechanics.Count < i + 2)
                    {
                        _mechanics.Add(new List<Mechanic.Mechanic>());
                    }
                }
                else
                {
                    _mechanics[i].Add(mechanic);
                    isAdded = true;
                }
            }
        }

        /// <summary>
        /// Добавляет коллекцию оборудования в компоновку вентиляционной установки
        /// </summary>
        /// <param name="mechanics">Коллекция оборудования</param>
        public void AddMechanic(ICollection<Mechanic.Mechanic> mechanics)
        {
            foreach (var mechanic in mechanics)
            {
                AddMechanic(mechanic);
            }
        }

        /// <summary>
        /// Возвращает компоновку наполнений вентиляционной установки
        /// </summary>
        /// <returns>Коллекция наполнений установки. В каждом уровне не более 1 типа оборудования различных видов</returns>
        public List<List<Mechanic.Mechanic>> GetMechanics()
        {
            return new List<List<Mechanic.Mechanic>>(_mechanics);
        }

        /// <summary>
        /// Добавляет наполнение в установку
        /// </summary>
        /// <param name="filling">Экземпляр наполнения</param>
        public void AddFilling(Filling filling)
        {
            _fillings.Add(filling);
        }

        /// <summary>
        /// Добавляет коллекцию наполнения в установку
        /// </summary>
        /// <param name="fillings">Коллекция наполнения</param>
        public void AddFilling(ICollection<Filling> fillings)
        {
            _fillings.AddRange(fillings);
        }

        /// <summary>
        /// Возвращает коллекцию экземпляров наполнения из установки
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<Filling> GetFillings()
        {
            return _fillings;
        }
    }
}
