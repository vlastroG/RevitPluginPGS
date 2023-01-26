using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.GUI.KR;
using MS.GUI.ViewModels.KR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ChangeSettingsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var settingsKRui = new SettingsKRview();
            settingsKRui.ShowDialog();
            return Result.Succeeded;
        }
    }
}
