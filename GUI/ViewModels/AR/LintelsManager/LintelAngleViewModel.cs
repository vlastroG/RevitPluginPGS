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
    /// ViewModel для окна редактирования свойств перемычки из уголков
    /// </summary>
    public class LintelAngleViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели окна редактирования свойств перемычки по умолчанию
        /// </summary>
        public LintelAngleViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели окна редактирования свойств перемычки из уголков
        /// </summary>
        /// <param name="angleLintel">Заданная перемычка из уголков</param>
        public LintelAngleViewModel(AngleLintel angleLintel)
        {

        }


        public Lintel GetLintel(Guid guid)
        {
            return new AngleLintel(guid);
        }
    }
}
