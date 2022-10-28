using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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
        /// <summary>
        /// Настройки команды
        /// </summary>
        private readonly SettingsViewModelKR _settings = new SettingsViewModelKR();

        /// <summary>
        /// Категории элементов MEP, которые можно выбирать
        /// </summary>
        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeCurves
        };

        /// <summary>
        /// Максимальный отступ от MEP элемента
        /// </summary>
        private readonly int _maxOffset = 1000;

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
            if (_settings.OpeningOffset > _maxOffset)
            {
                MessageBox.Show($"Слишком большой отступ: {_settings.OpeningOffset} мм", "Ошибка");
                var settingsKRview = new SettingsKRview();
                settingsKRview.ShowDialog();
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            (Element mep, Line mepLine) = GetMEPelement(commandData);
            Wall wall = GetWall(commandData);
            if ((mep is null) || (wall is null))
            {
                return Result.Cancelled;
            }
            var plane = GetWallPlane(wall).GetSurface() as Plane;
            var point = GetPlaneAndLineIntersection(plane, mepLine);
            if (point is null)
            {
                MessageBox.Show("Не найдена точка пересечения оси воздуховода и стены", "Ошибка");
                return Result.Cancelled;
            }
            (double openingH, double openingW) = GetOpeningDimensions(mep);
            if (openingH == openingW && openingW == 0)
            {
                MessageBox.Show("Нельзя определить геометрию выбранного воздуховода/трубы",
                    "Ошибка");
                return Result.Failed;
            }
            var test1 = openingH;
            var test2 = openingW;

            FamilyInstance opening = PlaceOpeningRectangle(doc, point, wall, openingH, openingW);
            if (opening is null)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Возвращает габариты проема, 
        /// который должен быть размещен по центру пересечения MEP элемента и стены/плиты
        /// </summary>
        /// <param name="mepEl"></param>
        /// <returns></returns>
        private (double OpeningH, double OpeningW) GetOpeningDimensions(in Element mepEl)
        {
            double mepH = 0;
            double mepW = 0;
            double openingH;
            double openingW;
            if (mepEl is Duct)
            {
                try
                {
                    // Если воздуховод прямоугольный или овальный
                    mepH = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
                    mepW = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
                }
                catch (System.NullReferenceException)
                {
                    // Воздуховод круглого сечения
                    mepH = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble();
                    mepW = mepH;
                }
            }
            else if (mepEl is Pipe)
            {
                // Труба
                mepH = mepEl.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                mepW = mepH;
            }
            else
            {
                return (0, 0);
            }
            openingH = mepH + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;
            openingW = mepW + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;
            return (openingH, openingW);
        }

        /// <summary>
        /// Вывод сообщения о не найденом семействе и типоразмере
        /// </summary>
        /// <param name="familyName">Название семейства</param>
        /// <param name="typeName">Название типоразмера</param>
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
                try
                {
                    opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                    opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                }
                catch (NullReferenceException)
                {
                    // Параметр отсутствует у экземпляра или доступен только на чтение
                }
                trans.Commit();
                return opening;
            }
        }


        /// <summary>
        /// Выбор воздуховода из связанного файла пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Воздуховод, или null, если операция отменена или не валидна</returns>
        private (Element duct, Line ductLine) GetMEPelement(in ExternalCommandData commandData)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            MulticategoryInLinkSelectionFilter filter
                = new MulticategoryInLinkSelectionFilter(
                  doc, _categories);
            Element mepEl = null;
            try
            {
                Reference mepElRef = uidoc.Selection.PickObject(
                    ObjectType.LinkedElement,
                    filter,
                    "Выберите воздуховод или трубу из связи");
                mepEl = filter.LinkedDocument.GetElement(mepElRef.LinkedElementId);
                var link = doc.GetElement(mepElRef.ElementId) as RevitLinkInstance;
                var mepElCurve = (mepEl.Location as LocationCurve).Curve;
                Transform transform = link.GetTransform();
                if (!transform.AlmostEqual(Transform.Identity))
                {
                    mepElCurve = mepElCurve.CreateTransformed(transform);
                }
                return (mepEl, mepElCurve as Line);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return (null, null);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбирать воздуховоды и трубы из связанных файлов",
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
                    "Перейдите на вид, где можно выделить стены",
                    "Ошибка");
                return null;
            }
            return wall;
        }
    }
}
