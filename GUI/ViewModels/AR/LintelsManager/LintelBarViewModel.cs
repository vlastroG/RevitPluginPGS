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
    /// Модель представления окна для редактирования свойств перемычки из арматурных стержней
    /// </summary>
    public class LintelBarViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели представления окна редактирования свойств перемычки из арматурных стержней по умолчанию
        /// </summary>
        public LintelBarViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели представления окна редактирования свойств заданной перемычки из арматурных стержней
        /// </summary>
        /// <param name="barLintel"></param>
        public LintelBarViewModel(BarLintel barLintel)
        {

        }


        public Lintel GetLintel(Guid guid)
        {
            return new BarLintel(guid);
        }
    }
}
