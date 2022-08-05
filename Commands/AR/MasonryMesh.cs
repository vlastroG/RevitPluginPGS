using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MasonryMesh : IExternalCommand
    {
        /// <summary>
        /// Сформировать значение наименования сетки для стены по типу армирования, ширине стены и отступу от граней стены
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="reinforceType"></param>
        /// <param name="wallWidth"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        private string CreateMeshName(Element wall, int reinforceType, double wallWidth, int indent)
        {
            string meshMark = wall.get_Parameter(SharedParams.Mrk_MeshMark).AsValueString();//  ?? String.Empty; //Значение параметра Мрк.МаркаСетки
            string steel = wall.get_Parameter(SharedParams.Arm_SteelClass).AsValueString();//  ?? String.Empty; //Значение параметра Арм.КлассСтали
            int barStep = (int)wall.get_Parameter(SharedParams.PGS_ArmStep).AsDouble();//  ?? String.Empty; //Значение параметра PGS_АрмШаг
            int diameter = (int)wall.get_Parameter(SharedParams.PGS_ArmDiameter).AsDouble(); //Значение параметра PGS_АрмДиаметр
            int meshWidth = ((int)wallWidth - indent * 2) / 10; //Значение ширины кладочной сетки = ширина стены - отступ*2

            StringBuilder sb = new StringBuilder();
            if (reinforceType == 1)
            {
                sb.Append(meshMark);
                sb.Append(' ');
                sb.Append(diameter);
                sb.Append(steel);
                sb.Append('-');
                sb.Append(barStep);
                sb.Append('/');
                sb.Append(diameter);
                sb.Append(steel);
                sb.Append('-');
                sb.Append(barStep);
                sb.Append(' ');
                sb.Append(meshWidth);
            }
            else if (reinforceType == 2)
            {
                sb.Append("Ø");
                sb.Append(diameter);
                sb.Append(' ');
                sb.Append(steel);
                sb.Append(", м.п.");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Подсчет кладочной сетки в стенах и его назначение в параметр PGS_ИтогАрмСетки
        /// наименование кладочного изделия пишется в Мрк.НаименованиеСетки
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
            SharedParams.PGS_TotalMasonryMesh,
            SharedParams.PGS_ArmType,
            SharedParams.PGS_ArmBarsCount,
            SharedParams.PGS_ArmCountRows,
            SharedParams.PGS_ArmIndentFromFace,
            SharedParams.Mrk_MeshName,
            SharedParams.Mrk_MeshMark,
            SharedParams.PGS_ArmStep,
            SharedParams.PGS_ArmDiameter
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\" " +
                    "присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nPGS_ИтогАрмСетки" +
                    "\nPGS_АрмТип" +
                    "\nPGS_АрмКолвоСтержней" +
                    "\nPGS_АрмКолвоАрмРядов" +
                    "\nPGS_АрмОтступОтГраней" +
                    "\nМрк.НаименованиеСетки" +
                    "\nМрк.МаркаСетки" +
                    "\nPGS_АрмШаг" +
                    "\nPGS_АрмДиаметр",
                    "Ошибка");
                return Result.Cancelled;
            }

            int indent = 0; //Значение отступа кладочной сетки от грани стены (с одной стороны)
            try
            {
                indent = UserInput.GetIntFromUser("Ввод отступа кладочной сетки в мм", "Введите ЦЕЛОЕ число:", 10);
            }
            catch (System.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            // Выбор всех однослойных стен в проекте, у которых значение параметра ТипАрмирования == 1 || 2
            var filter = new FilteredElementCollector(doc);
            var walls = filter
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as Wall)
                .Where(w => w.WallType.GetCompoundStructure() != null
                         && w.WallType.GetCompoundStructure().LayerCount == 1)
                .Where(w => w.get_Parameter(SharedParams.PGS_ArmType).HasValue == true
                         && (w.get_Parameter(SharedParams.PGS_ArmType).AsDouble() == 1
                         || w.get_Parameter(SharedParams.PGS_ArmType).AsDouble() == 2))
                .ToArray();

            int setLengthCount = 0;
            int setNameCount = 0;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Подсчет кладочной сетки");
                foreach (var wall in walls)
                {
                    double mesh_rows_in_wall = wall.get_Parameter(SharedParams.PGS_ArmCountRows).AsDouble();
                    double wall_height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble()
                        * SharedValues.FootToMillimeters;
                    double wall_length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble()
                        * SharedValues.FootToMillimeters;
                    double wall_width = wall.WallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble()
                        * SharedValues.FootToMillimeters;
                    int reinforceType = (int)wall.get_Parameter(SharedParams.PGS_ArmType).AsDouble();
                    string meshName = CreateMeshName(wall, reinforceType, wall_width, indent);
                    string meshNameExist = wall.get_Parameter(SharedParams.Mrk_MeshName).AsValueString();

                    var list_openings = wall
                        .FindInserts(false, false, true, true)
                        .Select(i => doc.GetElement(i))
                        .ToArray();

                    double mesh_length_in_openings = 0;
                    foreach (var opening in list_openings)
                    {
                        var (Height, Width) = WorkWithGeometry.GetWidthAndHeightOfElement(opening);
                        double opening_height = Height * SharedValues.FootToMillimeters;
                        double opening_width = Width * SharedValues.FootToMillimeters;
                        double opening_wall_height_percent = opening_height / wall_height;
                        int mesh_rows_in_opening = (int)(opening_wall_height_percent * mesh_rows_in_wall);
                        double mesh_length_in_opening = mesh_rows_in_opening * opening_width;
                        mesh_length_in_openings += mesh_length_in_opening;
                    }
                    // Длина кладочной сетки в метрах
                    double mesh_length_total = (mesh_rows_in_wall * wall_length - mesh_length_in_openings) / 1000;
                    double meshLength = wall.get_Parameter(SharedParams.PGS_TotalMasonryMesh).AsDouble();
                    if (meshLength != mesh_length_total)
                    {
                        wall.get_Parameter(SharedParams.PGS_TotalMasonryMesh).Set(mesh_length_total);
                        setLengthCount++;
                    }
                    if (meshName != meshNameExist)
                    {
                        wall.get_Parameter(SharedParams.Mrk_MeshName).Set(meshName);
                        setNameCount++;
                    }
                }
                trans.Commit();
            }

            MessageBox.Show($"Длина кладочной сетки подсчитана в м.п." +
                $"\nPGS_ИтогАрмСетки обновлен {setLengthCount} раз," +
                $"\nМрк.НаименованиеСетки обновлено {setNameCount} раз.");
            return Result.Succeeded;
        }
    }
}
