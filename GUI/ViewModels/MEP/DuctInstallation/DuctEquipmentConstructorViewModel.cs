using MS.Commands.MEP.Mechanic;
using MS.Commands.MEP.Mechanic.Impl;
using MS.Commands.MEP.Models;
using MS.Commands.MEP.Models.Symbolic;
using MS.GUI.CommandsBase;
using MS.GUI.ViewModels.Base;
using MS.GUI.Windows.MEP;
using MS.GUI.Windows.MEP.InstallationConstructor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MS.GUI.ViewModels.MEP.DuctInstallation
{
    public class DuctEquipmentConstructorViewModel : ViewModelBase
    {
        public DuctEquipmentConstructorViewModel()
        {
            Mechanics.Add(new Fan(150));
            Mechanics.Add(new Filter(450));

            Symbolics.Add(new Symbolic());
            Symbolics.Add(new Symbolic());
            Symbolics.Add(new Symbolic());

            Fillings.Add(new Filling("Наполнение 1", 2));
            Fillings.Add(new Filling("Наполнение 2", 1));
            Fillings.Add(new Filling("Наполнение 3", 3));
        }


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
        public ObservableCollection<Mechanic> Mechanics { get; } = new ObservableCollection<Mechanic>();

        /// <summary>
        /// Выбранное оборудование
        /// </summary>
        private Mechanic _selectedMechanic;

        /// <summary>
        /// Выбранное оборудование
        /// </summary>
        public Mechanic SelectedMechanic
        {
            get => _selectedMechanic;
            set
            {
                if (value is null)
                {
                    Set(ref _selectedMechanic, Mechanics.FirstOrDefault());
                }
                else
                {
                    Set(ref _selectedMechanic, value);
                }
            }
        }


        /// <summary>
        /// Коллекция элементов наполнения в установке
        /// </summary>
        public ObservableCollection<Filling> Fillings { get; } = new ObservableCollection<Filling>();

        /// <summary>
        /// Выбранное наполнение
        /// </summary>
        private Filling _selectedFilling;

        /// <summary>
        /// Выбранное наполнение
        /// </summary>
        public Filling SelectedFilling
        {
            get => _selectedFilling;
            set
            {
                if (value is null)
                {
                    Set(ref _selectedFilling, Fillings.FirstOrDefault());
                }
                else
                {
                    Set(ref _selectedFilling, value);
                }
            }
        }


        /// <summary>
        /// Коллекция элементов УГО в установке
        /// </summary>
        public ObservableCollection<Symbolic> Symbolics { get; } = new ObservableCollection<Symbolic>();

        /// <summary>
        /// Выбранное УГО
        /// </summary>
        private Symbolic _selectedSymbolic;

        /// <summary>
        /// Выбранное УГО
        /// </summary>
        public Symbolic SelectedSymbolic
        {
            get => _selectedSymbolic;
            set
            {
                if (value is null)
                {
                    Set(ref _selectedSymbolic, Symbolics.FirstOrDefault());
                }
                else
                {
                    Set(ref _selectedSymbolic, value);
                }
            }
        }

        #region Commands for Mechanic

        #region Create Mechanic command

        /// <summary>
        /// Команда добавления оборудования
        /// </summary>
        private ICommand _createNewMechanicCommand;

        /// <summary>
        /// Команда добавления оборудования
        /// </summary>
        public ICommand CreateNewMechanicCommand
            => _createNewMechanicCommand = _createNewMechanicCommand ?? new LambdaCommand(OnCreateNewMechanicCommandExecuted, CanCreateNewMechanicCommandExecute);

        /// <summary>
        /// Оборудование можно добавить всегда
        /// </summary>
        /// <param name="p">Передаваемый объект</param>
        /// <returns></returns>
        private static bool CanCreateNewMechanicCommandExecute(object p) => true;

        /// <summary>
        /// Действие при добавлении нового оборудования
        /// </summary>
        /// <param name="p"></param>
        private void OnCreateNewMechanicCommandExecuted(object p)
        {
            var ui = new ChooseMechanicTypeView()
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            ui.ShowDialog();
            ChooseMechanicTypeViewModel vm = ui.DataContext as ChooseMechanicTypeViewModel;
            if (ui.DialogResult == true && !string.IsNullOrEmpty(vm.SelectedMechanicType))
            {
                switch (vm.EquipmentType)
                {
                    case Commands.MEP.Enums.EquipmentType.Fan:
                        break;
                    case Commands.MEP.Enums.EquipmentType.AirCooler:
                        break;
                    case Commands.MEP.Enums.EquipmentType.AirHeater:
                        break;
                    case Commands.MEP.Enums.EquipmentType.Filter:
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Edit Mechanic command
        /// <summary>
        /// Команда редактирования оборудования
        /// </summary>
        private ICommand _editMechanicCommand;

        /// <summary>
        /// Команда редактирования оборудования
        /// </summary>
        public ICommand EditMechanicCommand
            => _editMechanicCommand = _editMechanicCommand ?? new LambdaCommand(OnEditMechanicCommandExecuted, CanEditMechanicCommandExecute);

        /// <summary>
        /// Проверка возможности выполнения команды редактирования оборудования
        /// </summary>
        /// <param name="p">Объект для редактирования</param>
        /// <returns></returns>
        private static bool CanEditMechanicCommandExecute(object p) => p is Mechanic;

        /// <summary>
        /// Действие, при редактировании оборудования
        /// </summary>
        /// <param name="p">Объект для редактирвоания</param>
        private void OnEditMechanicCommandExecuted(object p)
        {
            //write
            MessageBox.Show("Оборудование отредактировано", "Заголовок");
        }
        #endregion

        #region Delete command
        /// <summary>
        /// Команда удаления оборудования
        /// </summary>
        private ICommand _deleteMechanicCommand;

        /// <summary>
        /// Команда удаления оборудования
        /// </summary>
        public ICommand DeleteMechanicCommand
            => _deleteMechanicCommand = _deleteMechanicCommand ?? new LambdaCommand(OnDeleteMechanicCommandExecuted, CanDeleteMechanicCommandExecute);

        /// <summary>
        /// Элемент можно удалить из списка оборудования всегда
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool CanDeleteMechanicCommandExecute(object p) => true;

        /// <summary>
        /// Действие при удалении оборудования
        /// </summary>
        /// <param name="p"></param>
        private void OnDeleteMechanicCommandExecuted(object p)
        {
            //write
            Mechanic mechanic = (Mechanic)p;
            Mechanics.Remove(mechanic);
        }
        #endregion

        #endregion

        #region Commands for Filling

        #region Create Filling command

        /// <summary>
        /// Команда добавления наполнения
        /// </summary>
        private ICommand _createNewFillingCommand;

        /// <summary>
        /// Команда добавления наполнения
        /// </summary>
        public ICommand CreateNewFillingCommand
            => _createNewFillingCommand = _createNewFillingCommand ?? new LambdaCommand(OnCreateNewFillingCommandExecuted, CanCreateNewFillingCommandExecute);

        /// <summary>
        /// Наполнение можно добавить всегда
        /// </summary>
        /// <param name="p">Передаваемый объект</param>
        /// <returns></returns>
        private static bool CanCreateNewFillingCommandExecute(object p) => true;

        /// <summary>
        /// Действие при добавлении нового наполнения
        /// </summary>
        /// <param name="p"></param>
        private void OnCreateNewFillingCommandExecuted(object p)
        {
            //write
            Fillings.Add(new Filling("new name"));
        }

        #endregion

        #region Delete Filling command

        /// <summary>
        /// Команда удаления наполнения
        /// </summary>
        private ICommand _deleteFillingCommand;

        /// <summary>
        /// Команда удаления наполнения
        /// </summary>
        public ICommand DeleteFillingCommand
            => _deleteFillingCommand = _deleteFillingCommand ?? new LambdaCommand(OnDeleteFillingCommandExecuted, CanDeleteFillingCommandExecute);

        /// <summary>
        /// Элемент можно удалить из списка наполнения всегда
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool CanDeleteFillingCommandExecute(object p) => true;

        /// <summary>
        /// Действие при удалении наполнения
        /// </summary>
        /// <param name="p"></param>
        private void OnDeleteFillingCommandExecuted(object p)
        {
            //write
            Filling mechanic = (Filling)p;
            Fillings.Remove(mechanic);
        }

        #endregion

        #endregion

        #region Commands for Symbolic

        #region Create Symbolic command

        /// <summary>
        /// Команда добавления УГО
        /// </summary>
        private ICommand _createNewSymbolicCommand;

        /// <summary>
        /// Команда добавления УГО
        /// </summary>
        public ICommand CreateNewSymbolicCommand
            => _createNewSymbolicCommand = _createNewSymbolicCommand ?? new LambdaCommand(OnCreateNewSymbolicCommandExecuted, CanCreateNewSymbolicCommandExecute);

        /// <summary>
        /// Оборудование можно добавить, если передаваемый объект - УГО
        /// </summary>
        /// <param name="p">Передаваемый объект</param>
        /// <returns></returns>
        private static bool CanCreateNewSymbolicCommandExecute(object p) => true;

        /// <summary>
        /// Действие при добавлении нового УГО
        /// </summary>
        /// <param name="p"></param>
        private void OnCreateNewSymbolicCommandExecuted(object p)
        {
            //write
            Symbolics.Add(new Symbolic());
        }

        #endregion

        #region Delete Symbolic command

        /// <summary>
        /// Команда удаления УГО
        /// </summary>
        private ICommand _deleteSymbolicCommand;

        /// <summary>
        /// Команда удаления УГО
        /// </summary>
        public ICommand DeleteSymbolicCommand
            => _deleteSymbolicCommand = _deleteSymbolicCommand ?? new LambdaCommand(OnDeleteSymbolicCommandExecuted, CanDeleteSymbolicCommandExecute);

        /// <summary>
        /// Элемент можно удалить из списка УГО всегда
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private static bool CanDeleteSymbolicCommandExecute(object p) => true;

        /// <summary>
        /// Действие при удалении УГО
        /// </summary>
        /// <param name="p"></param>
        private void OnDeleteSymbolicCommandExecuted(object p)
        {
            //write
            Symbolic mechanic = (Symbolic)p;
            Symbolics.Remove(mechanic);
        }

        #endregion

        #region Convert to Mechanic and Filling command

        private ICommand _convertToMechanicAndFilling;

        public ICommand ConvertToMechanicAndFillingCommand
            => _convertToMechanicAndFilling = _convertToMechanicAndFilling ?? new LambdaCommand(OnConvertToMechanicAndFillingCommandExecuted, CanConvertToMechanicAndFillingCommandExecute);

        private bool CanConvertToMechanicAndFillingCommandExecute(object p)
        {
            return (Fillings.Count + Mechanics.Count) == 0;
        }

        private void OnConvertToMechanicAndFillingCommandExecuted(object p)
        {
            //
        }

        #endregion

        #endregion
    }
}
