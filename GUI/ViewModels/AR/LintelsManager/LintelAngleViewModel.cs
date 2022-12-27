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
    public class LintelAngleViewModel : ViewModelBase, ILintelCreator
    {
        public LintelAngleViewModel()
        {

        }

        public LintelAngleViewModel(AngleLintel angleLintel)
        {

        }


        public Lintel GetLintel()
        {
            return new AngleLintel(Guid.NewGuid());
        }
    }
}
