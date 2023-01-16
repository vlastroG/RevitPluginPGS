using MS.GUI.CommandsBase;
using MS.GUI.Windows.AR.LintelsManager;
using MS.RevitCommands.AR.DTO;
using MS.RevitCommands.AR.DTO.LintelsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    public class SimilarOpeningsViewModel : OpeningsInstancesViewModel
    {
        public SimilarOpeningsViewModel()
        {

        }

        public bool EditSelectedSimilarOpening { get; private set; }


        public SimilarOpeningsViewModel(ICollection<SimilarOpeningsDto> openings)
            : base(openings.Select(similarOpening => similarOpening as OpeningDto).ToList()) { }


        #region EditOpenings Command
        private ICommand _editOpenings;

        public ICommand EditOpenings
            => _editOpenings = _editOpenings ?? new LambdaCommand(OnEditOpeningsCommandExecuted, CanEditOpeningsCommandExecute);

        private bool CanEditOpeningsCommandExecute(object p) => !(SelectedOpening is null);

        private void OnEditOpeningsCommandExecuted(object p) { EditSelectedSimilarOpening = true; }
        #endregion


        #region UnsetEditSelectedSimilarOpeningCommand
        /// <summary>
        /// Сбросить значение свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso>
        /// </summary>
        private ICommand _unsetEditSelectedSimilarOpeningCommand;

        /// <summary>
        /// Сбросить значение свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso>
        /// </summary>
        public ICommand UnsetEditSelectedSimilarOpeningCommand
            => _unsetEditSelectedSimilarOpeningCommand
            = _unsetEditSelectedSimilarOpeningCommand
            ?? new LambdaCommand(OnUnsetEditSelectedSimilarOpeningCommandExecute, CanUnsetEditSelectedSimilarOpeningCommandExecute);

        /// <summary>
        /// Команду можно выполнить всегда
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool CanUnsetEditSelectedSimilarOpeningCommandExecute(object p) => true;

        /// <summary>
        /// Сбрасывание значения свойства <seealso cref="GoToSelectedOpeningView3D">GoToSelectedOpeningView3D</seealso> на false
        /// </summary>
        /// <param name="p"></param>
        private void OnUnsetEditSelectedSimilarOpeningCommandExecute(object p) { EditSelectedSimilarOpening = false; }
        #endregion
    }
}
