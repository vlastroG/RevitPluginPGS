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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Модель представления главного окна менеджера перемычек
    /// </summary>
    public class LintelsManagerViewModel : ViewModelBase
    {
        /// <summary>
        /// Конструктор менеджера перемычек по умолчанию
        /// </summary>
        public LintelsManagerViewModel()
        {
            Openings.Add(new OpeningDto(Guid.NewGuid(), 1200, 2000, 120, 500, 450, 5500, "Кирпич", "Level 1") { Mark = "ПР-1", Lintel = new BarLintel(Guid.NewGuid()) });
            Openings.Add(new OpeningDto(Guid.NewGuid(), 800, 2000, 120, 500, 350, 1300, "ГСБ", "Level 1") { Mark = "ПР-2", Lintel = new BarLintel(Guid.NewGuid()) });
            Openings.Add(new OpeningDto(Guid.NewGuid(), 1500, 2000, 200, 200, 450, 5500, "Бетон", "Level 1") { Mark = "ПР-3", Lintel = new BlockLintel(Guid.NewGuid()) });
            Openings.Add(new OpeningDto(Guid.NewGuid(), 1800, 2000, 120, 500, 450, 5500, "Кирпич", "Level 1") { Mark = "ПР-4", Lintel = new AngleLintel(Guid.NewGuid()) });
        }

        /// <summary>
        /// Конструктор окна менеджера перемычек по заданной коллекции Dto проемов
        /// </summary>
        /// <param name="openings"></param>
        public LintelsManagerViewModel(ICollection<OpeningDto> openings)
        {
            Openings = new ObservableCollection<OpeningDto>(openings);
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
                            LintelBarWindow barLintelWindow = new LintelBarWindow() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen };
                            barLintelWindow.ShowDialog();
                            if (barLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (barLintelWindow.DataContext as LintelBarViewModel).GetLintel(SelectedOpening.Guid);
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Block:
                            LintelBlockWindow blockLintelWindow = new LintelBlockWindow() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen };
                            blockLintelWindow.ShowDialog();
                            if (blockLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (blockLintelWindow.DataContext as LintelBlockViewModel).GetLintel(SelectedOpening.Guid);
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Angle:
                            LintelAngleWindow angleLintelWindow = new LintelAngleWindow() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen };
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
                            Openings.UpdateEntity(SelectedOpening);
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
                            Openings.UpdateEntity(SelectedOpening);
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
                            Openings.UpdateEntity(SelectedOpening);
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
            Openings.UpdateEntity(SelectedOpening);
        }
        #endregion
    }
}
