using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Windows;

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
        /// Guid параметра КоличествоАрмируемыхРядов у стен
        /// </summary>
        private readonly Guid guid_par_mesh_rows_count = Guid.Parse("fb502bf5-cbd4-416b-8ce1-cbf2ac40c3b6");

        /// <summary>
        /// Guid параметра Рзм.Ширина у проемов
        /// </summary>
        private readonly Guid guid_par_width = Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");

        /// <summary>
        /// Guid параметра Рзм.Высота у проемов
        /// </summary>
        private readonly Guid guid_par_height = Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");

        /// <summary>
        /// Guid параметра ПлощадьКладочнойСетки у стены
        /// </summary>
        private readonly Guid guid_par_mesh_length = Guid.Parse("7f925503-538c-43f3-9e75-aa7a3f43eb0e");
        //private readonly BuiltInParameter guid_par_mesh_length = BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS;

        /// <summary>
        /// Подсчет кладочной сетки в стенах и его назначение в параметр ДлинаКладочнойСетки
        /// Сейчас заполняется параметр Комментарии.
        /// Для релиза изменить свойство guid_par_mesh_length - для длины кладочной сетки
        /// и guid_par_mesh_rows_count - для количества армируемых рядов.
        /// После нужно скорректировать назначение параметра в транзакции (116 line)
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


            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure() != null
                         && w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(guid_par_mesh_rows_count).HasValue == true
                         && w.get_Parameter(guid_par_mesh_rows_count).AsDouble() >= 1);

            double mesh_rows_in_wall = 0;
            double mesh_length_in_openings = 0;
            double wall_height = 0;
            double wall_length = 0;
            double wall_width = 0;
            double mesh_length_total = 0;
            double mesh_area_total = 0;
            int calculatedWallsCount = 0;
            foreach (var wall in walls)
            {
                mesh_rows_in_wall = wall.get_Parameter(guid_par_mesh_rows_count).AsDouble();
                wall_height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble() * _mm_in_foot;
                wall_length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * _mm_in_foot;
                wall_width = wall.WallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble() * _mm_in_foot;

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
                mesh_area_total = (mesh_length_total / _mm_in_foot) * (wall_width / _mm_in_foot);

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Подсчет кладочной сетки");
                    try
                    {
                        wall.get_Parameter(guid_par_mesh_length).Set(mesh_area_total);
                        calculatedWallsCount++;
                    }
                    catch (NullReferenceException)
                    {
                        throw new ArgumentNullException($"{nameof(mesh_area_total)} guid of param mesh_length is failed.");
                    }

                    trans.Commit();
                }
            }
            if (calculatedWallsCount == 0)
                MessageBox.Show($"Обработано {calculatedWallsCount} стен.");
            else
                MessageBox.Show($"Площадь кладочной сетки подсчитана в м.кв.\nОбработано {calculatedWallsCount} стен.");

            return Result.Succeeded;
        }
    }
}
