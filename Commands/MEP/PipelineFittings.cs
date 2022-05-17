using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PipelineFittings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter = new FilteredElementCollector(doc);

            var pipe_accessories = filter
                .OfCategory(BuiltInCategory.OST_PipeAccessory)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as FamilyInstance);


            foreach (var pipe_acc in pipe_accessories)
            {
                var t = pipe_acc;
            }

            return Result.Succeeded;
        }
    }
}
