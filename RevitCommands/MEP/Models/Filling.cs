using MS.RevitCommands.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP.Models
{
    public class Filling : IDataErrorInfo
    {
        /// <summary>
        /// ADSK_Наименование
        /// </summary>
        private string _name;

        /// <summary>
        /// ADSK_Количество
        /// </summary>
        private double _count;


        /// <summary>
        /// Конструктор наполнения по наименованию и количеству
        /// </summary>
        /// <param name="name">ADSK_Наименование</param>
        /// <param name="count">ADSK_Количество</param>
        [JsonConstructor]
        public Filling(string name, double count)
        {
            _name = name;
            _count = count;
        }

        /// <summary>
        /// Конструктор наполнения по наименованию
        /// </summary>
        /// <param name="name">ADSK_Наименование</param>
        public Filling(string name) : this(name, 0) { }


        /// <summary>
        /// ADSK_Наименование
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// ADSK_Количество
        /// </summary>
        public double Count
        {
            get => _count;
            set => _count = value;
        }

        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrWhiteSpace(Name))
                        {
                            error = "Наименование не должно быть пустым или состоять только из пробелов.";
                        }
                        break;
                    case "Count":
                        if (Count < 0)
                        {
                            error = "Количество должно быть >= 0";
                        }
                        break;
                }
                return error;
            }
        }
    }
}
