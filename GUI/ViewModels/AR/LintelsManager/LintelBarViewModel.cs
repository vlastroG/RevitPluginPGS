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
    public class LintelBarViewModel : ViewModelBase, ILintelCreator
    {
        public LintelBarViewModel()
        {

        }

        public LintelBarViewModel(BarLintel barLintel)
        {

        }


        public Lintel GetLintel()
        {
            throw new NotImplementedException();
        }
    }
}
