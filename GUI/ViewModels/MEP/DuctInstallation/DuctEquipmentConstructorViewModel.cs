using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    public class DuctEquipmentConstructorViewModel : ViewModelBase
    {
        /// <summary>
        /// Полное название установки
        /// </summary>
        private string _nameLong;

        /// <summary>
        /// Полное название установки
        /// </summary>
        public string NameLong
        {
            get => _nameLong;
            set => Set(ref _nameLong, value);
        }

        /// <summary>
        /// Сокращенное наименование установки
        /// </summary>
        private string _nameShort;

        /// <summary>
        /// Сокращенное наименование установки
        /// </summary>
        public string NameShort
        {
            get => _nameShort;
            set => Set(ref _nameShort, value);
        }

        /// <summary>
        /// Наименование системы
        /// </summary>
        private string _systemName;

        /// <summary>
        /// Наименование системы
        /// </summary>
        public string SystemName
        {
            get => _systemName;
            set => Set(ref _systemName, value);
        }

        /// <summary>
        /// Коллекция элементов оборудования в установке
        /// </summary>
        public ObservableCollection<EquipmentViewModel> Equipment { get; } = new ObservableCollection<EquipmentViewModel>();


        public DuctEquipmentConstructorViewModel()
        {

        }
    }
}
