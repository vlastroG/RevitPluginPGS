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
        /// Список УГО внутри родительского семейства вентиляционной установки
        /// </summary>
        private readonly List<Symbolic.Symbolic> _symbolics = new List<Symbolic.Symbolic>();

        /// <summary>
        /// Ширина установки в мм
        /// </summary>
        private double _width;

        /// <summary>
        /// Высота установки в мм
        /// </summary>
        private double _height;

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
        public Installation(double width, double height)
        {
            _width = width;
            _height = height;
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
        public double Length
        {
            get
            {
                return _symbolics.Select(s => s.Length).Sum();
            }
        }

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
        public List<Filling> GetFillings()
        {
            return new List<Filling>(_fillings);
        }

        /// <summary>
        /// Добавляет УГО в установку
        /// </summary>
        /// <param name="symbolic"></param>
        public void AddSymbolic(Symbolic.Symbolic symbolic)
        {
            _symbolics.Add(symbolic);
        }

        /// <summary>
        /// Добавляет коллекцию УГО в установку
        /// </summary>
        /// <param name="symbolics"></param>
        public void AddSymbolic(ICollection<Symbolic.Symbolic> symbolics)
        {
            _symbolics.AddRange(symbolics);
        }

        /// <summary>
        /// Возвращает список УГО элементов одноуровневой установки слева направо
        /// </summary>
        /// <returns></returns>
        public List<Symbolic.Symbolic> GetSymbolics()
        {
            return new List<Symbolic.Symbolic>(_symbolics);
        }
    }
}
