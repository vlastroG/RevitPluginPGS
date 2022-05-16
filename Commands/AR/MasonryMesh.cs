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
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            string par_name__MeshRowCount = "Арм.ОчереднАрмирРядовКладки";//Определить параметр для фильтрации

            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter.OfCategory(BuiltInCategory.OST_Walls).WhereElementIsNotElementType().ToElements().Select(e => e as Wall).Where(w => w.WallType.GetCompoundStructure().LayerCount == 1).Where(w => w.LookupParameter(par_name__MeshRowCount).AsDouble() >= 1);


            Wall wall = walls.FirstOrDefault();

            var list_doors_windows = wall.FindInserts(true, true, true, true).Select(i => doc.GetElement(i)).ToList();

            var door = list_doors_windows[0];

            return Result.Succeeded;
        }
    }
}
