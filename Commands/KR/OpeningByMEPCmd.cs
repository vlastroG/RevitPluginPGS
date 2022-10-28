using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.KR;
using MS.GUI.ViewModels.KR;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpeningByMEPCmd : IExternalCommand
    {
        private readonly SettingsViewModelKR _settings = new SettingsViewModelKR();

        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeCurves
        };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (String.IsNullOrEmpty(_settings.OpeningFamName)
                || String.IsNullOrEmpty(_settings.OpeningTypeName)
                || String.IsNullOrEmpty(_settings.OpeningOffsetString))
            {
                var settingsKRview = new SettingsKRview();
                settingsKRview.ShowDialog();
                if (settingsKRview.DialogResult != true)
                {
                    return Result.Cancelled;
                }
            }
            if (_settings.OpeningOffset > 1000)
            {
                MessageBox.Show($"Слишком большой отступ: {_settings.OpeningOffset} мм", "Ошибка");
                var settingsKRview = new SettingsKRview();
                settingsKRview.ShowDialog();
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            (Element duct, Line ductLine) = GetDuct(commandData);
            Wall wall = GetWall(commandData);
            return Result.Succeeded;
            if ((duct is null) || (wall is null))
            {
                return Result.Cancelled;
            }
            var plane = GetWallPlane(wall).GetSurface() as Plane;
            var point = GetPlaneAndLineIntersection(plane, ductLine);
            if (point is null)
            {
                MessageBox.Show("Не найдена точка пересечения оси воздуховода и стены", "Ошибка");
                return Result.Cancelled;
            }
            double ductH = 0;
            double ductW = 0;
            double openingH;
            double openingW;
            try
            {
                // Если воздуховод прямоугольный или овальный
                ductH = duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
                ductW = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
            }
            catch (System.NullReferenceException)
            {
                // Воздуховод круглого сечения
                ductH = duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble();
                ductW = ductH;
            }
            openingH = ductH + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;
            openingW = ductW + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;

            FamilyInstance opening = PlaceOpeningRectangle(doc, point, wall, openingH, openingW);
            if (opening is null)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        private void ShowError(string familyName, string typeName)
        {
            MessageBox.Show(
                $"В проекте не найдено семейство:" +
                $"\n{familyName}" +
                $"\nс типом" +
                $"\n{typeName}",
                "Ошибка");
        }

        /// <summary>
        /// Возвращает плоскую поверхность с ниабольшей площадью и нормалью,
        /// совпадающей с нормалью выбранной стены
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>Поверхность стены, или null, если что-то пошло не так</returns>
        private PlanarFace GetWallPlane(Wall wall)
        {
            Solid wallSolid = null;
            GeometryElement geomElem = wall.get_Geometry(new Options());
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        wallSolid = solid;
                        break;
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                wallSolid = solid;
                                break;
                            }
                        }
                    }
                }
            }

            if (wallSolid == null) return null;
            var wallNormal = wall.Orientation;
            var faces = wallSolid.Faces;
            PlanarFace wallPlanarFace = null;
            foreach (var face in faces)
            {
                if ((face is PlanarFace face1)
                    && face1.FaceNormal.IsAlmostEqualTo(wallNormal)
                    && ((wallPlanarFace is null) || (face1.Area > wallPlanarFace.Area)))
                {
                    wallPlanarFace = face1;
                }
            }
            return wallPlanarFace;
        }

        /// <summary>
        /// Находит точку пересечения поверхности с линией
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="line"></param>
        /// <returns>Точка пересечеия, или null</returns>
        private XYZ GetPlaneAndLineIntersection(Plane plane, Line line)
        {
            UV uv1, uv2 = new UV();

            plane.Project(line.Origin, out uv1, out double d);
            plane.Project(line.Origin + line.Direction, out uv2, out double b);

            XYZ xyz1 = plane.Origin + (uv1.U * plane.XVec) + (uv1.V * plane.YVec);
            XYZ xyz2 = plane.Origin + (uv2.U * plane.XVec) + (uv2.V * plane.YVec);

            if (xyz1.IsAlmostEqualTo(xyz2))
            {
                return xyz1;
            }

            Line projectedLine = Line.CreateUnbound(xyz1, xyz2 - xyz1);

            IntersectionResultArray iResult = new IntersectionResultArray();
            if (line.Intersect(projectedLine, out iResult) != SetComparisonResult.Disjoint)
            {
                return iResult.get_Item(0).XYZPoint;
            }
            return null;
        }

        /// <summary>
        /// Размещает семейство проема
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point">Точка размещения</param>
        /// <param name="hostWall">Стена, в которой будет расположен проем</param>
        /// <param name="openingH">Высота проема</param>
        /// <param name="openingW">Ширина проема</param>
        /// <returns>Экземпляр семейства проема</returns>
        private FamilyInstance PlaceOpeningRectangle(in Document doc, XYZ point, in Wall hostWall, double openingH, double openingW)
        {
            var openingSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == _settings.OpeningFamName && ft.Name == _settings.OpeningTypeName);
            if (openingSymb is null)
            {
                ShowError(_settings.OpeningFamName, _settings.OpeningTypeName);
                return null;
            }
            if (!openingSymb.IsActive)
            {
                openingSymb.Activate();
            }
            Level level = (doc.GetElement(hostWall.LevelId)) as Level;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Отверстие КР по инженерке");
                var opening = doc.Create.NewFamilyInstance(point, openingSymb, hostWall, level, StructuralType.NonStructural);
                opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(point.Z - 0.5 * openingH - level.Elevation);
                opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                trans.Commit();
                return opening;
            }
        }


        /// <summary>
        /// Выбор воздуховода из связанного файла пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Воздуховод, или null, если операция отменена или не валидна</returns>
        private (Element duct, Line ductLine) GetDuct(in ExternalCommandData commandData)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            MulticategoryInLinkSelectionFilter filter
                = new MulticategoryInLinkSelectionFilter(
                  doc, _categories);
            Element duct = null;
            try
            {
                Reference ductRef = uidoc.Selection.PickObject(
                    ObjectType.LinkedElement,
                    filter,
                    "Выберите воздуховод или трубу из связи");
                duct = filter.LinkedDocument.GetElement(ductRef.LinkedElementId);
                var link = doc.GetElement(ductRef.ElementId) as RevitLinkInstance;
                var ductCurve = (duct.Location as LocationCurve).Curve;
                Transform transform = link.GetTransform();
                if (!transform.AlmostEqual(Transform.Identity))
                {
                    ductCurve = ductCurve.CreateTransformed(transform);
                }
                return (duct, ductCurve as Line);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return (null, null);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбирать воздуховоды из связанных файлов",
                    "Ошибка");
                return (null, null);
            }
        }

        /// <summary>
        /// Выбор стены пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Стена, или null, если операция отменена или не валидна</returns>
        private Wall GetWall(in ExternalCommandData commandData)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            Selection sel = uidoc.Selection;
            SelectionFilterElementsOfCategory<Wall> filter
                = new SelectionFilterElementsOfCategory<Wall>(
                    new List<BuiltInCategory> {
                        BuiltInCategory.OST_Walls
                    },
                    false);
            Wall wall = null;
            try
            {
                Reference wallRef = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    filter,
                    "Выберите воздуховод из связанного файла");
                wall = doc.GetElement(wallRef) as Wall;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно стены",
                    "Ошибка");
                return null;
            }
            return wall;
        }
    }
}
