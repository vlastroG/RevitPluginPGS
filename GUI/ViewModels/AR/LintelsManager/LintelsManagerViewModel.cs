using MS.GUI.CommandsBase;
using MS.GUI.ViewModels.Base;
using MS.GUI.Windows.AR.LintelsManager;
using MS.RevitCommands.AR.DTO;
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

        }

        public LintelsManagerViewModel(ICollection<OpeningDto> openings)
        {
            Openings = new ObservableCollection<OpeningDto>(openings);
        }


        public ObservableCollection<OpeningDto> Openings { get; } = new ObservableCollection<OpeningDto>();

        private OpeningDto _selectedOpening;

        public OpeningDto SelecedOpening
        {
            get => _selectedOpening;
            set => Set(ref _selectedOpening, value);
        }


        private ICommand _setLintelCommand;

        public ICommand SetLintelCommand
            => _setLintelCommand = _setLintelCommand ?? new LambdaCommand(OnSetLintelCommandExecuted, CanSetLintelCommandExecute);

        private bool CanSetLintelCommandExecute(object p) => true; // !(_selectedOpening is null) && (_selectedOpening.Lintel is null);

        private void OnSetLintelCommandExecuted(object p)
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
                        break;
                    case RevitCommands.AR.Enums.LintelType.Block:
                        break;
                    case RevitCommands.AR.Enums.LintelType.Angle:
                        break;
                }
            }
        }
    }
}
