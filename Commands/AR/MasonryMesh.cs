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
        /// <summary>
        /// Коэффициент для перевода футов в миллиметры
        /// </summary>
        private double _mm_in_foot = 304.8;

        /// <summary>
        /// Подсчет кладочной сетки в стенах
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //string par_name__MeshRowCount = "Арм.ОчереднАрмирРядовКладки";//Определить параметр для фильтрации

            Guid guid_par_mesh_rows_count = Guid.Parse("42795f38-352d-44c6-b739-4a97d0f765db");// Guid параметра Арм.ОчереднАрмирРядовКладки
            Guid guid_par_width = Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");// Guid параметра Рзм.Ширина
            Guid guid_par_height = Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");// Guid параметра Рзм.Высота
            //Guid guid_par_mesh_length = Guid.Parse("b396979f-903a-49fd-a897-2c9f256d978d");// Guid параметра ДлинаКладочнойСетки

            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(guid_par_mesh_rows_count).HasValue == true
                         && w.get_Parameter(guid_par_mesh_rows_count).AsDouble() >= 1);

            double mesh_rows_in_wall = 0;
            double mesh_length_in_openings = 0;
            double wall_height = 0;
            double wall_length = 0;
            double mesh_length_total = 0;
            foreach (var wall in walls)
            {
                mesh_rows_in_wall = wall.get_Parameter(guid_par_mesh_rows_count).AsDouble();
                wall_height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble() * _mm_in_foot;
                wall_length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * _mm_in_foot;

                var list_openings = wall
                    .FindInserts(true, true, true, true)
                    .Select(i => doc.GetElement(i) as FamilyInstance)
                    .ToList();

                double opening_width = 0;
                double opening_height = 0;
                double opening_wall_height_percent = 0;
                double mesh_rows_in_opening = 0;
                double mesh_length_in_opening = 0;
                foreach (var opening in list_openings)
                {
                    opening_width = opening.get_Parameter(guid_par_width).AsDouble() * _mm_in_foot;
                    if (opening_width == 0)
                    {
                        opening_width = opening.Symbol.get_Parameter(guid_par_width).AsDouble() * _mm_in_foot;
                    }

                    opening_height = opening.get_Parameter(guid_par_height).AsDouble() * _mm_in_foot;
                    if (opening_height == 0)
                    {
                        opening_height = opening.Symbol.get_Parameter(guid_par_height).AsDouble() * _mm_in_foot;
                    }

                    opening_wall_height_percent = opening_height / wall_height;
                    mesh_rows_in_opening = opening_wall_height_percent * mesh_rows_in_wall;
                    mesh_length_in_opening = mesh_rows_in_opening * opening_width;
                    mesh_length_in_openings += mesh_length_in_opening;
                }

                mesh_length_total = mesh_rows_in_wall * wall_length - mesh_length_in_openings;

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Подсчет кладочной сетки");
                    try
                    {
                        //wall.get_Parameter(guid_par_mesh_length).Set(mesh_length_total);
                        wall.LookupParameter("Арм.ДлинаКладСеткиПМ").Set(mesh_length_total);
                    }
                    catch (NullReferenceException)
                    {
                        throw new ArgumentNullException($"{nameof(mesh_length_total)} guid of param mesh_length is failed.");
                    }

                    trans.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
