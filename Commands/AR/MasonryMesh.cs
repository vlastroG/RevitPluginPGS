using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
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

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.Arm_CountReinforcedRowsMasonry,
            SharedParams.Arm_AreaOfMasonryMesh
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\" " +
                    "присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nАрм.КолвоАрмированияКладки" +
                    "\nАрм.ПлощадьКлСетки",
                    "Ошибка");
                return Result.Cancelled;
            }

            // Выбор всех однослойных стен в проекте, у которых значение параметра КоличествоАрмируемыхРядов >= 1.
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure() != null
                         && w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(SharedParams.Arm_CountReinforcedRowsMasonry).HasValue == true
                         && w.get_Parameter(SharedParams.Arm_CountReinforcedRowsMasonry).AsDouble() >= 1);

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
                mesh_rows_in_wall = wall.get_Parameter(SharedParams.Arm_CountReinforcedRowsMasonry).AsDouble();
                wall_height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble() * SharedValues.FootToMillimeters;
                wall_length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * SharedValues.FootToMillimeters;
                wall_width = wall.WallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble() * SharedValues.FootToMillimeters;

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
                    opening_width = opening.get_Parameter(SharedParams.ADSK_DimensionWidth).AsDouble() * SharedValues.FootToMillimeters;
                    if (opening_width == 0)
                    {
                        opening_width = opening.Symbol.get_Parameter(SharedParams.ADSK_DimensionWidth).AsDouble() * SharedValues.FootToMillimeters;
                    }

                    opening_height = opening.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble() * SharedValues.FootToMillimeters;
                    if (opening_height == 0)
                    {
                        opening_height = opening.Symbol.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble() * SharedValues.FootToMillimeters;
                    }

                    opening_wall_height_percent = opening_height / wall_height;
                    mesh_rows_in_opening = opening_wall_height_percent * mesh_rows_in_wall;
                    mesh_length_in_opening = mesh_rows_in_opening * opening_width;
                    mesh_length_in_openings += mesh_length_in_opening;
                }

                mesh_length_total = mesh_rows_in_wall * wall_length - mesh_length_in_openings;
                mesh_area_total = (mesh_length_total / SharedValues.FootToMillimeters) * (wall_width / SharedValues.FootToMillimeters);

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Подсчет кладочной сетки");
                    try
                    {
                        wall.get_Parameter(SharedParams.Arm_AreaOfMasonryMesh).Set(mesh_area_total);
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
