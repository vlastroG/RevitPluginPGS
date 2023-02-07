using MS.RevitCommands.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP.Models.Symbolic
{
    /// <summary>
    /// УГО элемента вентиляционной установки
    /// </summary>
    public class Symbolic : ISymbolic, IDataErrorInfo
    {
        /// <summary>
        /// Название УГО
        /// </summary>
        private string _name;

        /// <summary>
        /// Длина в мм
        /// </summary>
        private double _length;


        /// <summary>
        /// Конструктор УГО по наименованию и длине
        /// </summary>
        /// <param name="name">Наименование типоразмера УГО в родительском семействе установки</param>
        /// <param name="length">Длина УГО</param>
        [JsonConstructor]
        public Symbolic(string name, double length)
        {
            _name = name;
            _length = length;
        }

        /// <summary>
        /// Конструктор УГО по умолчанию (Фильтр, длиной 200 мм)
        /// </summary>
        public Symbolic() : this("Фильтр", 200) { }


        /// <summary>
        /// Доступные названия типов УГО
        /// </summary>
        [JsonIgnore]
        public string[] SymbolicTypes
        {
            get => new string[7]
            {
                "Вентилятор",
                "Воздухонагреватель водяной",
                "Воздухонагреватель электрический",
                "Воздухоохладитель водяной",
                "Воздухоохладитель электрический",
                "Фильтр",
                "Шумоглушитель"
            };
        }

        public string Name { get => _name; set => _name = value; }

        public double Length { get => _length; set => _length = value; }

        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Length":
                        if ((Length < 50) || (Length > 2000))
                        {
                            error = "Длина УГО должна быть от 50 до 2000 мм включительно";
                        }
                        break;
                }
                return error;
            }
        }
    }
}
