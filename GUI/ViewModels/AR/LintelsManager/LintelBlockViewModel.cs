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
    public class LintelBlockViewModel : ViewModelBase, ILintelCreator
    {
        public LintelBlockViewModel()
        {

        }

        public LintelBlockViewModel(BlockLintel blockLintel)
        {

        }



        public Lintel GetLintel()
        {
            return new BlockLintel(Guid.NewGuid());
        }
    }
}
