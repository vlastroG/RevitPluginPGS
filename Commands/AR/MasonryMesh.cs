using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

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

            //string par_name__MeshRowCount = "Арм.ОчереднАрмирРядовКладки";//Определить параметр для фильтрации

            Guid guid = Guid.Parse("42795f38-352d-44c6-b739-4a97d0f765db");// Guid параметра Арм.ОчереднАрмирРядовКладки
            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(guid).HasValue == true && w.get_Parameter(guid).AsDouble() >= 1);

            foreach (var wall in walls)
            {
                var list_openings = wall
                    .FindInserts(true, true, true, true)
                    .Select(i => doc.GetElement(i))
                    .ToList();

                var door = list_openings[0];
            }


            return Result.Succeeded;
        }
    }
}
