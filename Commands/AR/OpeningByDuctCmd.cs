using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpeningByDuctCmd : IExternalCommand
    {
        private readonly string _familyName = "231_Проем прямоуг (Окно_Стена)";

        private readonly string _name = "231_Проем прямоуг (Окно_Стена)";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Element duct = GetDuct(commandData);
            Wall wall = GetWall(commandData);
            if ((duct is null) || (wall is null))
            {
                return Result.Cancelled;
            }
            var ductH = duct.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
            var ductW = duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
            var openingH = ductH + 2 * 50 / SharedValues.FootToMillimeters;
            var openingW = ductW + 2 * 50 / SharedValues.FootToMillimeters;

            var ductCurve = (duct.Location as LocationCurve).Curve as Line;
            var plane = GetWallPlane(wall).GetSurface() as Plane;
            var point = GetPlaneAndLineIntersection(plane, ductCurve);
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Разместить проем");
                var opening = PlaceOpening(doc, point, wall);
                opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(point.Z - 0.5 * openingH);
                opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                trans.Commit();
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Возвращает плоскую поверхность с ниабольшей площадью и нормалью,
        /// совпадающей с нормалью выбранной стены
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>Поверхность стены, или null, если что-то пошло не так</returns>
        private PlanarFace GetWallPlane(Wall wall)
        {
            Plane plane = null;
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

        private FamilyInstance PlaceOpening(in Document doc, XYZ point, in Wall hostWall)
        {
            var openingSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == _familyName && ft.Name == _name);
            var opening = doc.Create.NewFamilyInstance(point, openingSymb, hostWall, StructuralType.NonStructural);
            return opening;
        }


        /// <summary>
        /// Выбор воздуховода из связанного файла пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Воздуховод, или null, если операция отменена или не валидна</returns>
        private Element GetDuct(in ExternalCommandData commandData)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            Selection sel = uidoc.Selection;
            ElementInLinkSelectionFilter<Duct> filter
                = new ElementInLinkSelectionFilter<Duct>(
                  doc);
            Element duct = null;
            try
            {
                Reference ductRef = uidoc.Selection.PickObject(
                    ObjectType.LinkedElement,
                    filter,
                    "Выберите воздуховод из связанного файла");
                duct = filter.LinkedDocument.GetElement(ductRef.LinkedElementId);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбирать воздуховоды из связанных файлов",
                    "Ошибка");
                return null;
            }
            return duct;
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
