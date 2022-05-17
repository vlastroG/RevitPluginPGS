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
        private double _mm_in_foot = 304.8;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //string par_name__MeshRowCount = "Арм.ОчереднАрмирРядовКладки";//Определить параметр для фильтрации

            Guid guid_par_mesh_rows_count = Guid.Parse("42795f38-352d-44c6-b739-4a97d0f765db");// Guid параметра Арм.ОчереднАрмирРядовКладки
            Guid guid_par_width = Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");// Guid параметра Рзм.Ширина
            Guid guid_par_height = Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");// Guid параметра Рзм.Высота

            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(guid_par_mesh_rows_count).HasValue == true && w.get_Parameter(guid_par_mesh_rows_count).AsDouble() >= 1);

            foreach (var wall in walls)
            {
                var list_openings = wall
                    .FindInserts(true, true, true, true)
                    .Select(i => doc.GetElement(i) as FamilyInstance)
                    .ToList();
                foreach (var opening in list_openings)
                {
                    var width = opening.get_Parameter(guid_par_width).AsDouble() * _mm_in_foot;
                    if (width == 0)
                    {
                        width = opening.Symbol.get_Parameter(guid_par_width).AsDouble() * _mm_in_foot;
                    }

                    var height = opening.get_Parameter(guid_par_height).AsDouble() * _mm_in_foot;
                    if (height == 0)
                    {
                        height = opening.Symbol.get_Parameter(guid_par_height).AsDouble() * _mm_in_foot;
                    }
                }

            }


            return Result.Succeeded;
        }
    }
}
