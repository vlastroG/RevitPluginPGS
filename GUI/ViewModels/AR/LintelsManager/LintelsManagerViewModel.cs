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
    public class LintelsManagerViewModel : ViewModelBase
    {
        public LintelsManagerViewModel()
        {
            Openings.Add(new OpeningDto(1200, 120, 500, 450, 5500, "Кирпич"));
        }

        public LintelsManagerViewModel(ICollection<OpeningDto> openings)
        {
            Openings = new ObservableCollection<OpeningDto>(openings);
        }


        public ObservableCollection<OpeningDto> Openings { get; } = new ObservableCollection<OpeningDto>();

        private OpeningDto _selectedOpening;

        public OpeningDto SelectedOpening
        {
            get => _selectedOpening;
            set => Set(ref _selectedOpening, value);
        }


        private ICommand _setOrEditLintelCommand;

        public ICommand SetOrEditLintelCommand
            => _setOrEditLintelCommand = _setOrEditLintelCommand ?? new LambdaCommand(OnSetOrEditLintelCommandExecuted, CanSetOrEditLintelCommandExecute);

        private bool CanSetOrEditLintelCommandExecute(object p) => !(_selectedOpening is null);

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
                                SelectedOpening.Lintel = (barLintelWindow.DataContext as LintelBarViewModel).GetLintel();
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Block:
                            LintelBlockWindow blockLintelWindow = new LintelBlockWindow() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen };
                            blockLintelWindow.ShowDialog();
                            if (blockLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (blockLintelWindow.DataContext as LintelBlockViewModel).GetLintel();
                            }
                            break;
                        case RevitCommands.AR.Enums.LintelType.Angle:
                            LintelAngleWindow angleLintelWindow = new LintelAngleWindow() { WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen };
                            angleLintelWindow.ShowDialog();
                            if (angleLintelWindow.DialogResult == true)
                            {
                                SelectedOpening.Lintel = (angleLintelWindow.DataContext as LintelAngleViewModel).GetLintel();
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
                            SelectedOpening.Lintel = (barLintelWindow.DataContext as LintelBarViewModel).GetLintel();
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
                            SelectedOpening.Lintel = (blockLintelWindow.DataContext as LintelBlockViewModel).GetLintel();
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
                            SelectedOpening.Lintel = (angleLintelWindow.DataContext as LintelAngleViewModel).GetLintel();
                        }
                        break;
                    default:
                        break;
                }
            }
        }


    }
}
