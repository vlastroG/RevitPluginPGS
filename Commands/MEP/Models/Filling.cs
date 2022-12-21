using MS.Commands.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Models
{
    public class Filling
    {
        /// <summary>
        /// ADSK_Наименование
        /// </summary>
        private string _name;

        /// <summary>
        /// ADSK_Количетсво
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
        /// ADSK_Количетсво
        /// </summary>
        public double Count
        {
            get => _count;
            set => _count = value;
        }
    }
}
