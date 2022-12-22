using MS.Commands.MEP.Mechanic.Impl;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    /// <summary>
    /// Модель представления фильтра
    /// </summary>
    public class FilterViewModel : ViewModelBase, IDataErrorInfo
    {
        public FilterViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели представления фильтра по заданному фильтру
        /// </summary>
        /// <param name="filter"></param>
        public FilterViewModel(Filter filter)
        {
            Count = filter.Count;
            Length = filter.Length;
            Note = filter.Note;
            Type = filter.Type;
            Windage = filter.Windage;
        }


        private string _Type;

        /// <summary>
        /// PGS_ФильтрТип
        /// </summary>
        public string Type { get => _Type; set => Set(ref _Type, value); }


        private int? _Count;

        /// <summary>
        /// PGS_ФильтрКоличество
        /// </summary>
        public int? Count { get => _Count; set => Set(ref _Count, value); }


        private double? _Windage;

        /// <summary>
        /// ADSK_Сопротивление воздушного фильтра
        /// </summary>
        public double? Windage { get => _Windage; set => Set(ref _Windage, value); }


        private string _Note;

        /// <summary>
        /// ADSK_Примечание
        /// </summary>
        public string Note { get => _Note; set => Set(ref _Note, value); }


        /// <summary>
        /// Длина
        /// </summary>
        private double _length;

        /// <summary>
        /// Длина
        /// </summary>
        public double Length { get => _length; set => Set(ref _length, value); }

        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Count":
                        if ((Count != null) && (Count < 0))
                        {
                            error = "Количество должно быть >= 0";
                        }
                        break;
                }
                return error;
            }
        }

        public Filter GetFilter()
        {
            return GetFilter(Guid.NewGuid());
        }

        public Filter GetFilter(Guid guid)
        {
            return new Filter(guid, Length)
            {
                Count = Count,
                Note = Note,
                Type = Type,
                Windage = Windage
            };
        }
    }
}
