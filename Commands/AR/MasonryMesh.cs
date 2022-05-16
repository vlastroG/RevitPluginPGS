using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MasonryMesh : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Wall wall = doc.GetElement("3095440") as Wall;

            List<Element> list_doors_windows = (List<Element>)wall.FindInserts(true, false, false, false).Select(i => doc.GetElement(i));

            var door = list_doors_windows[0];

            return Result.Succeeded;
        }
    }
}
