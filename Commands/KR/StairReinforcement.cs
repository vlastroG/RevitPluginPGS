using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StairReinforcement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter = new SelectionFilterElementsOfCategory<BuiltInCategory>();

            List<Element> selectedElements = null;
            try
            {
                //selectedElements = uidoc.Selection.PickElementsByRectangle(filter, "Выберите помещения.").ToList();
                selectedElements = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Выберите лестницы.").Select(e => doc.GetElement(e.ElementId)).ToList();
            }
            catch (OperationCanceledException e)
            {
                return Result.Cancelled;
            }

            string test = "test";

            return Result.Succeeded;
        }
    }
}
