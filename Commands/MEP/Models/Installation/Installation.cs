using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Models.Installation
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Installation
    {
        /// <summary>
        /// Список болванок семейств оборудования внутри родительского семейства вентиляционной установки.
        /// Болванки представлены списками оборудования
        /// </summary>
        [JsonProperty]
        private readonly List<List<Mechanic.Mechanic>> _mechanics = new List<List<Mechanic.Mechanic>>() {
            new List<Mechanic.Mechanic>()
        };

        /// <summary>
        /// Список болванок семейств наполнения внутри родительского семейства вентиляционной установки.
        /// </summary>
        [JsonProperty]
        private readonly List<Filling> _fillings = new List<Filling>();

        /// <summary>
        /// Список УГО внутри родительского семейства вентиляционной установки
        /// </summary>
        [JsonProperty]
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
        /// ADSK_Группирование "-1"
        /// </summary>
        private string _groupingParent = "-1";

        /// <summary>
        /// Вложенное_наполнение_группирование "-2"
        /// </summary>
        private string _groupingBoldFilling = "-2";

        /// <summary>
        /// Вложенное_оборудование_группирование "-3"
        /// </summary>
        private string _groupingBoldMechanic = "-3";

        /// <summary>
        /// Впуск_Ширина
        /// </summary>
        private double _inputWidth = 600;

        /// <summary>
        /// Впуск_Высота
        /// </summary>
        private double _inputHeight = 500;

        /// <summary>
        /// Впуск_Длина
        /// </summary>
        private double _inputLength = 100;

        /// <summary>
        /// False => Впуск_Посередине, True => Впуск_Снизу
        /// </summary>
        private bool _inputLocationBottom = false;

        /// <summary>
        /// Выпуск_Ширина
        /// </summary>
        private double _outputWidth = 600;

        /// <summary>
        /// Выпуск_Высота
        /// </summary>
        private double _outputHeight = 500;

        /// <summary>
        /// Выпуск_Длина
        /// </summary>
        private double _outputLength = 100;

        /// <summary>
        /// Fase => Выпуск_Посередине, True => Выпуск_Снизу
        /// </summary>
        private bool _outputLocationBottom = false;


        /// <summary>
        /// Конструктор установки по габаритным размерам
        /// </summary>
        /// <param name="width">Ширина в мм</param>
        /// <param name="height">Высота в мм</param>
        [JsonConstructor]
        public Installation(double width, double height)
        {
            _width = width;
            _height = height;
        }


        /// <summary>
        /// Ширина установки в мм
        /// </summary>
        [JsonProperty]
        [Range(100, 2000)]
        [Description("ADSK_Размер_Ширина")]
        public double Width { get => _width; set => _width = value; }


        /// <summary>
        /// Высота установки в мм
        /// </summary>
        [JsonProperty]
        [Range(100, 2000)]
        [Description("ADSK_Размер_Высота")]
        public double Height { get => _height; set => _height = value; }


        /// <summary>
        /// Длина установки в мм
        /// </summary>
        [JsonProperty]
        [Description("ADSK_Размер_Длина")]
        public double Length
        {
            get
            {
                return _symbolics.Select(s => s.Length).Sum() + InputLength + OutputLength;
            }
        }

        /// <summary>
        /// ADSK_Группирование
        /// </summary>
        [JsonProperty]
        [Description("ADSK_Группирование")]
        public string GroupingParent { get => System + _groupingParent; }

        /// <summary>
        /// Вложенное_оборудование_группирование
        /// </summary>
        [JsonProperty]
        [Description("Вложенное_оборудование_группирование")]
        public string GroupingMechanic { get => System + _groupingBoldMechanic; }

        /// <summary>
        /// Вложенное_наполнение_группирование
        /// </summary>
        [JsonProperty]
        [Description("Вложенное_наполнение_группирование")]
        public string GroupingFilling { get => System + _groupingBoldFilling; }

        /// <summary>
        /// PGS_ТипУстановки
        /// </summary>
        [JsonProperty]
        [Description("PGS_ТипУстановки")]
        public string Type { get; set; }

        /// <summary>
        /// ADSK_Наименование краткое
        /// </summary>
        [JsonProperty]
        [Description("ADSK_Наименование краткое")]
        public string NameShort { get; set; }

        /// <summary>
        /// ADSK_Наименование
        /// </summary>
        [JsonProperty]
        [Description("ADSK_Наименование")]
        public string Name { get; set; }


        /// <summary>
        /// Префикс наименования (сокращенное название системы)
        /// </summary>
        [JsonProperty]
        public string System { get; set; }

        /// <summary>
        /// Впуск_Ширина
        /// </summary>
        [JsonProperty]
        [Description("Впуск_Ширина")]
        public double InputWidth { get => _inputWidth; set => _inputWidth = value; }

        /// <summary>
        /// Впуск_Высота
        /// </summary>
        [JsonProperty]
        [Description("Впуск_Высота")]
        public double InputHeight { get => _inputHeight; set => _inputHeight = value; }

        /// <summary>
        /// Впуск_Длина
        /// </summary>
        [JsonProperty]
        [Description("Впуск_Длина")]
        public double InputLength { get => _inputLength; set => _inputLength = value; }

        /// <summary>
        /// Впуск_Снизу
        /// </summary>
        [JsonProperty]
        [Description("Впуск_Снизу")]
        public int InputLocationBottom
        {
            get => _inputLocationBottom ? 1 : 0;
            set => _inputLocationBottom = value == 1 ? true : false;
        }

        /// <summary>
        /// Впуск_Посередине
        /// </summary>
        [JsonProperty]
        [Description("Впуск_Посередине")]
        public int InputLocationMiddle
        {
            get => !_inputLocationBottom ? 1 : 0;
            set => _inputLocationBottom = value == 0 ? true : false;
        }

        /// <summary>
        /// Выпуск_Ширина
        /// </summary>
        [JsonProperty]
        [Description("Выпуск_Ширина")]
        public double OutputWidth { get => _outputWidth; set => _outputWidth = value; }

        /// <summary>
        /// Выпуск_Высота
        /// </summary>
        [JsonProperty]
        [Description("Выпуск_Высота")]
        public double OutputHeight { get => _outputHeight; set => _outputHeight = value; }

        /// <summary>
        /// Выпуск_Длина
        /// </summary>
        [JsonProperty]
        [Description("Выпуск_Длина")]
        public double OutputLength { get => _outputLength; set => _outputLength = value; }

        /// <summary>
        /// Выпуск_Снизу
        /// </summary>
        [JsonProperty]
        [Description("Выпуск_Снизу")]
        public int OutputLocationBottom
        {
            get => _outputLocationBottom ? 1 : 0;
            set => _outputLocationBottom = value == 1 ? true : false;
        }

        /// <summary>
        /// Выпуск_Посередине
        /// </summary>
        [JsonProperty]
        [Description("Выпуск_Посередине")]
        public int OutputLocationMiddle
        {
            get => !_outputLocationBottom ? 1 : 0;
            set => _outputLocationBottom = value == 0 ? true : false;
        }

        /// <summary>
        /// Возвращает словарь названий параметров оборудования и их значений (значения заполненных свойств оборудования)
        /// </summary>
        /// <returns>Словарь заполненных параметров и их значений</returns>
        public virtual Dictionary<string, dynamic> GetParameters()
        {
            Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();

            PropertyInfo[] properties = GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                var description = ((DescriptionAttribute)property
                    .GetCustomAttribute(typeof(DescriptionAttribute)))?.Description;
                if (!(description is null))
                {
                    var value = property.GetValue(this);
                    if (!(value is null))
                    {
                        parameters.Add(description, value);
                    }
                }
            }
            return parameters;
        }

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
