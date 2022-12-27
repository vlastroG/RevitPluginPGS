using MS.RevitCommands.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    public interface ILintelCreator
    {
        Lintel GetLintel();
    }
}
