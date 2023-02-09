using MS.GUI.CommandsBase;
using MS.GUI.ViewModels.Base;
using MS.GUI.Windows.AR.LintelsManager;
using MS.RevitCommands.AR.DTO;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.AR.Models.Lintels;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Модель представления окна менеджера перемычек для редактирования перемычек по экземплярам проемов
    /// </summary>
    public class OpeningsInstancesViewModel : ViewModelBase
    {
        /// <summary>
        /// Конструктор менеджера перемычек по умолчанию
        /// </summary>
        public OpeningsInstancesViewModel()
        {

        }

        /// <summary>
        /// Конструктор окна менеджера перемычек по заданной коллекции Dto проемов
        /// </summary>
        /// <param name="openings"></param>
        public OpeningsInstancesViewModel(ICollection<OpeningDto> openings)
        {
            Openings = new ObservableCollection<OpeningDto>(openings);
            _OpeningsViewSource = new CollectionViewSource
            {
                Source = openings,
                SortDescriptions =
                {
                    new SortDescription(nameof(OpeningDto.Level), ListSortDirection.Ascending),
                    new SortDescription(nameof(OpeningDto.WallMaterial), ListSortDirection.Descending),
                    new SortDescription(nameof(OpeningDto.Width), ListSortDirection.Ascending),
                    new SortDescription(nameof(OpeningDto.WallThick), ListSortDirection.Ascending),
                    new SortDescription(nameof(OpeningDto.WallHeightOverOpening), ListSortDirection.Ascending),
                    new SortDescription(nameof(OpeningDto.DistanceToLeftEnd), ListSortDirection.Ascending),
                    new SortDescription(nameof(OpeningDto.DistanceToRightEnd), ListSortDirection.Ascending),
                }
            };
            _OpeningsViewSource.Filter += OnOpeningsFilter;
        }


        /// <summary>
        /// Строка фильтрации проемов
        /// </summary>
        private protected string _OpeningsFilter;

        /// <summary>
        /// Коллекция для фильтрации во ViewModel
        /// </summary>
        private protected readonly CollectionViewSource _OpeningsViewSource;

        /// <summary>
        /// Проемы с фильтрацией
        /// </summary>
        public ICollectionView OpeningsView => _OpeningsViewSource?.View;

        /// <summary>
        /// Строка для фильтрации проемов
        /// </summary>
        public string OpeningsFilter
        {
            get => _OpeningsFilter;
            set
            {
                if (Set(ref _OpeningsFilter, value))
                {
                    _OpeningsViewSource.View.Refresh();
                }
            }
        }

        /// <summary>
        /// Все проемы, в свойстве <seealso cref="OpeningDto.LongName"/> НЕ будет содержаться значение <see cref="OpeningsFilter"/>, не будут пропускаться фильтром.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="E"></param>
        private void OnOpeningsFilter(object sender, FilterEventArgs E)
        {
            if (!(E.Item is OpeningDto opening) || string.IsNullOrWhiteSpace(OpeningsFilter)) return;

            if (!opening.LongName.Contains(OpeningsFilter))
            {
                E.Accepted = false;
            }
        }


        /// <summary>
        /// Обновлять расположение перемычек после нажатия кнопки Ок, или нет
        /// </summary>
        private bool _updateLintelsLocation;

        /// <summary>
        /// Обновлять расположение перемычек после нажатия кнопки Ок, или нет
        /// </summary>
        public bool UpdateLintelsLocation { get => _updateLintelsLocation; set => Set(ref _updateLintelsLocation, value); }

        /// <summary>
        /// Переходить к выбранному проему на 3D виде
        /// </summary>
        public bool GoToSelectedOpeningView3D { get; private protected set; }


        /// <summary>
        /// Список проемов для создания перемычек в проекте
        /// </summary>
        public ObservableCollection<OpeningDto> Openings { get; } = new ObservableCollection<OpeningDto>();

        /// <summary>
        /// Выбранный проем
        /// </summary>
        private OpeningDto _selectedOpening;

        /// <summary>
        /// Выбранный проем
        /// </summary>
        public OpeningDto SelectedOpening
        {
            get => _selectedOpening;
            set => Set(ref _selectedOpening, value);
        }


        #region EditLintelCommand

        /// <summary>
        /// Команда назначения / редактирования назначенной перемычки проема
        /// </summary>
        private ICommand _setOrEditLintelCommand;

        /// <summary>
        /// Команда назначения / редактирования назначенной перемычки проема
        /// </summary>
        public ICommand SetOrEditLintelCommand
            => _setOrEditLintelCommand = _setOrEditLintelCommand ?? new LambdaCommand(OnSetOrEditLintelCommandExecuted, CanSetOrEditLintelCommandExecute);

        /// <summary>
        /// Назначить или отредактировать перемычку можно, только если выбранный проем не null
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CanSetOrEditLintelCommandExecute(object p) => !(SelectedOpening is null);

        /// <summary>
        /// Действие команды назначения / редактирования перемычки
        /// </summary>
        /// <param name="p">Выбранный элемент списка проемов</param>
        private void OnSetOrEditLintelCommandExecuted(object p)
        {
            var lintel = (p as OpeningDto).Lintel;
            if (lintel is null)
            {
                var ui = new ChooseLintelTypeView()
                {
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };
                ui.ShowDialog();
                ChooseLintelTypeViewModel vm = ui.DataContext as ChooseLintelTypeViewModel;
                if (ui.DialogResult == true)
                {
                    switch (vm.SelectedLintelType)
                    {
                        case RevitCommands.AR.Enums.LintelType.Bar:
                            LintelBarWindow barLintelWindow = new LintelBarWindow()
                            {
                                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                            };
                            barLintelWindow.ShowDialog();
                            if (barLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (barLintelWindow.DataContext as LintelBarViewModel).GetLintel(SelectedOpening.Guid);
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Block:
                            LintelBlockWindow blockLintelWindow = new LintelBlockWindow()
                            {
                                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                            };
                            blockLintelWindow.ShowDialog();
                            if (blockLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (blockLintelWindow.DataContext as LintelBlockViewModel).GetLintel(SelectedOpening.Guid);
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Angle:
                            LintelAngleWindow angleLintelWindow = new LintelAngleWindow()
                            {
                                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                            };
                            angleLintelWindow.ShowDialog();
                            if (angleLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (angleLintelWindow.DataContext as LintelAngleViewModel).GetLintel(SelectedOpening.Guid);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                switch (lintel.LintelType)
                {
                    case RevitCommands.AR.Enums.LintelType.Bar:
                        LintelBarViewModel barVM = new LintelBarViewModel((BarLintel)lintel);
                        LintelBarWindow barLintelWindow = new LintelBarWindow()
                        {
                            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                            DataContext = barVM
                        };
                        barLintelWindow.ShowDialog();
                        if (barLintelWindow.DialogResult == true)
                        {
                            SelectedOpening.Lintel = (barLintelWindow.DataContext as LintelBarViewModel).GetLintel(SelectedOpening.Guid);
                        }
                        break;
                    case RevitCommands.AR.Enums.LintelType.Block:
                        LintelBlockViewModel blockVM = new LintelBlockViewModel((BlockLintel)lintel);
                        LintelBlockWindow blockLintelWindow = new LintelBlockWindow()
                        {
                            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                            DataContext = blockVM
                        };
                        blockLintelWindow.ShowDialog();
                        if (blockLintelWindow.DialogResult == true)
                        {
                            SelectedOpening.Lintel = (blockLintelWindow.DataContext as LintelBlockViewModel).GetLintel(SelectedOpening.Guid);
                        }
                        break;
                    case RevitCommands.AR.Enums.LintelType.Angle:
                        LintelAngleViewModel angleVM = new LintelAngleViewModel((AngleLintel)lintel);
                        LintelAngleWindow angleLintelWindow = new LintelAngleWindow()
                        {
                            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                            DataContext = angleVM
                        };
                        angleLintelWindow.ShowDialog();
                        if (angleLintelWindow.DialogResult == true)
                        {
                            SelectedOpening.Lintel = (angleLintelWindow.DataContext as LintelAngleViewModel).GetLintel(SelectedOpening.Guid);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region
        /// <summary>
        /// Команда удаления перемычки из проема
        /// </summary>
        private ICommand _deleteLintelCommand;

        /// <summary>
        /// Команда удаления перемычки из проема
        /// </summary>
        public ICommand DeleteLintelCommand
            => _deleteLintelCommand = _deleteLintelCommand ?? new LambdaCommand(OnDeleteLintelCommandExecuted, CanDeleteLintelCommandExecute);

        /// <summary>
        /// Удалить перемычку из проема можно, только если выбранный проем не null и его перемычка также не null
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CanDeleteLintelCommandExecute(object p) => !(SelectedOpening?.Lintel is null);

        /// <summary>
        /// Действие команды удаления перемычки проема
        /// </summary>
        /// <param name="p"></param>
        private void OnDeleteLintelCommandExecuted(object p)
        {
            SelectedOpening.Lintel = null;
        }
        #endregion


        #region GoToOpeningView3D command
        /// <summary>
        /// Команда перехода к 3D обрезке выбранного проема
        /// </summary>
        private ICommand _goToOpeningView3DCommand;

        /// <summary>
        /// Команда перехода к 3D обрезке выбранного проема
        /// </summary>
        public ICommand GoToOpeningView3DCommand
            => _goToOpeningView3DCommand = _goToOpeningView3DCommand ?? new LambdaCommand(OnGoToOpeningView3DExecuted, CanGoToOpeningView3DExecute);

        /// <summary>
        /// Перейти к 3D обрезке выбранного проема можно только если выбран проем
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CanGoToOpeningView3DExecute(object p) => !(SelectedOpening is null);

        /// <summary>
        /// Команда перехода к 3D обрезке выбранного проема
        /// </summary>
        /// <param name="p"></param>
        private void OnGoToOpeningView3DExecuted(object p) { GoToSelectedOpeningView3D = true; }
        #endregion


        #region UnsetGoToOpeningView3DCommand
        /// <summary>
        /// Сбросить значение свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso>
        /// </summary>
        private ICommand _unsetGoToOpeningView3DCommand;

        /// <summary>
        /// Сбросить значение свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso>
        /// </summary>
        public ICommand UnsetGoToOpeningView3DCommand
            => _unsetGoToOpeningView3DCommand = _unsetGoToOpeningView3DCommand ?? new LambdaCommand(OnUnsetGoToView3DCommandExecute, CanUnsetGoToOpeningView3DCommandExecute);

        /// <summary>
        /// Команду можно выполнить всегда
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CanUnsetGoToOpeningView3DCommandExecute(object p) => true;

        /// <summary>
        /// Сбрасывание значения свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso> на false
        /// </summary>
        /// <param name="p"></param>
        private void OnUnsetGoToView3DCommandExecute(object p) { GoToSelectedOpeningView3D = false; }
        #endregion
    }
}
