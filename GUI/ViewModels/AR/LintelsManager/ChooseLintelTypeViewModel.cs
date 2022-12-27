using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// ViewModel для выбора типа перемычки
    /// </summary>
    public class ChooseLintelTypeViewModel : ViewModelBase
    {
        /// <summary>
        /// Выбранный тип перемычки
        /// </summary>
        private LintelType _selectedLintelType;

        /// <summary>
        /// Выбранный тип перемычки
        /// </summary>
        public LintelType SelectedLintelType { get => _selectedLintelType; set => Set(ref _selectedLintelType, value); }

        /// <summary>
        /// Типы перемычек, доступные для выбора
        /// </summary>
        public IEnumerable<LintelType> LintelTypes => Enum.GetValues(typeof(LintelType)).Cast<LintelType>();
    }
}
