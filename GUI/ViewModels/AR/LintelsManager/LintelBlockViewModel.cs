using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.AR.Models.Lintels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Модель представления окна для редактирования свойств перемычки из бетонных брусков
    /// </summary>
    public class LintelBlockViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели представления окна для редактирования свойств перемычки из бетонных брусков по умолчанию
        /// </summary>
        public LintelBlockViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели представления окна для редактирования свойств заданной перемычки из бетонных брусков
        /// </summary>
        public LintelBlockViewModel(BlockLintel blockLintel)
        {

        }



        public Lintel GetLintel(Guid guid)
        {
            return new BlockLintel(guid);
        }
    }
}
