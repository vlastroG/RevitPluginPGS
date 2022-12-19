using MS.Commands.MEP.Mechanic;
using MS.Commands.MEP.Models;
using MS.Commands.MEP.Models.Symbolic;
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
        /// Тип установки
        /// </summary>
        private string _type;

        /// <summary>
        /// Тип установки
        /// </summary>
        public string Type
        {
            get => _type;
            set => Set(ref _type, value);
        }


        /// <summary>
        /// Впуск высота
        /// </summary>
        private double? _inputHeight;

        /// <summary>
        /// Впуск высота
        /// </summary>
        public double? InputHeight
        {
            get => _inputHeight;
            set => Set(ref _inputHeight, value);
        }

        /// <summary>
        /// Впуск ширина
        /// </summary>
        private double? _inputWidth;

        /// <summary>
        /// Впуск ширина
        /// </summary>
        public double? InputWidth
        {
            get => _inputWidth;
            set => Set(ref _inputWidth, value);
        }

        /// <summary>
        /// Впуск высота
        /// </summary>
        private double? _inputLength;

        /// <summary>
        /// Впуск высота
        /// </summary>
        public double? InputLength
        {
            get => _inputLength;
            set => Set(ref _inputLength, value);
        }

        /// <summary>
        /// Выпуск высота
        /// </summary>
        private double? _outputHeight;

        /// <summary>
        /// Выпуск высота
        /// </summary>
        public double? OutputHeight
        {
            get => _outputHeight;
            set => Set(ref _outputHeight, value);
        }

        /// <summary>
        /// Выпуск ширина
        /// </summary>
        private double? _outputWidth;

        /// <summary>
        /// Выпуск ширина
        /// </summary>
        public double? OutputWidth
        {
            get => _outputWidth;
            set => Set(ref _outputWidth, value);
        }

        /// <summary>
        /// Выпуск длина
        /// </summary>
        private double? _outputLength;

        /// <summary>
        /// Выпуск длина
        /// </summary>
        public double? OutputLength
        {
            get => _outputLength;
            set => Set(ref _outputLength, value);
        }


        /// <summary>
        /// Впуск снизу
        /// </summary>
        private bool _inputLocationBottom;

        /// <summary>
        /// Впуск снизу
        /// </summary>
        public bool InputLocationBottom
        {
            get => _inputLocationBottom;
            set => Set(ref _inputLocationBottom, value);
        }

        /// <summary>
        /// Выпуск снизу
        /// </summary>
        private bool _outputLocationBottom;

        /// <summary>
        /// Выпуск снизу
        /// </summary>
        public bool OutputLocationBottom
        {
            get => _outputLocationBottom;
            set => Set(ref _outputLocationBottom, value);
        }


        /// <summary>
        /// Коллекция элементов оборудования в установке
        /// </summary>
        public ObservableCollection<Mechanic> Equipment { get; } = new ObservableCollection<Mechanic>();


        /// <summary>
        /// Коллекция элементов наполнения в установке
        /// </summary>
        public ObservableCollection<Filling> Filling { get; } = new ObservableCollection<Filling>();


        /// <summary>
        /// Коллекция элементов УГО в установке
        /// </summary>
        public ObservableCollection<Symbolic> Symbolics { get; } = new ObservableCollection<Symbolic>();


        public DuctEquipmentConstructorViewModel()
        {

        }
    }
}
