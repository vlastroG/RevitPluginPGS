using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    /// <summary>
    /// Модель представления фильтра
    /// </summary>
    public class FilterViewModel : ViewModelBase
    {
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
    }
}
