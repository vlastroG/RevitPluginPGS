using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MS.Utilites.Extensions.ExtensionCurveArrArray;
using static MS.Utilites.Extensions.ExtensionCurve;
using static MS.Utilites.Extensions.ExtensionWall;
using static MS.Utilites.Extensions.ExtensionCurveArray;

namespace MS.Commands.AR
{
    /// <summary>
    /// Команда для подсчета отделки откосов и плинтусов в помещениях
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomBoardsFinishingCommand : IExternalCommand
    {
        /// <summary>
        /// Удаляет из списка кривых нижнюю горизонтальную линию
        /// </summary>
        /// <param name="curves">Список кривых</param>
        /// <returns>Список кривых без нижней горизонтальной линии</returns>
        public List<Curve> RemoveLowerHorizontalCurve(ref List<Curve> curves)
        {
            // Все горизонтальные линии
            List<Curve> horizontalCurves = curves.Where(c => c.IsHorizontal() == true).ToList();
            if (horizontalCurves.Count == 0) return curves;
            int zero = 0;
            // Сортировка списка горизонтальных кривых по высоте (в начале будут нижние)
            horizontalCurves.Sort((c1, c2) => c1.GetEndPoint(zero).Z.CompareTo(c2.GetEndPoint(zero).Z));
            int indexOfBottomCurve = curves.IndexOf(horizontalCurves[zero]);
            // Удаление первой нижней кривой из списка кривых
            curves.RemoveAt(indexOfBottomCurve);
            return curves;
        }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SolidCurveIntersectionOptions intersectionOptions = new SolidCurveIntersectionOptions();
            // Все выбранные помещения перед запуском команды
            List<Room> rooms = sel.GetElementIds().Select(id => doc.GetElement(id))
                .Where(e => (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(e)
                == BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(r => r.Area > 0)
                .ToList();

            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { BuiltInCategory.OST_Rooms },
                false);

            if (rooms.Count == 0)
            {
                try
                {
                    // Пользователь выбирает помещения
                    rooms = uidoc.Selection
                        .PickObjects(
                            Autodesk.Revit.UI.Selection.ObjectType.Element,
                            filter,
                            "Выберите помещения")
                        .Select(e => doc.GetElement(e))
                        .Cast<Room>()
                        .Where(r => r.Area > 0)
                        .ToList();
                    if (rooms.Count == 0) return Result.Cancelled;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
            }

            List<(Wall, double)> curtainWallsSlopeDepth = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Where(w => (w as Wall).CurtainGrid != null)
                .Where(w => w.get_Parameter(SharedParams.PGS_SlopeDepth).AsDouble() > 0)
                .Select(w => ((w as Wall), w.get_Parameter(SharedParams.PGS_SlopeDepth).AsDouble()))
                .ToList();

            var sketchFilter = new ElementClassFilter(typeof(Sketch));

            // Создать профили витражей, у которых он не был создан
            var sketchesVirgin = curtainWallsSlopeDepth
                .Where(wd => wd.Item1.SketchId.IntegerValue < 0)
                .Select(wd => (wd.Item1.GetRectangularProfile(), wd.Item2))
                .Select(wd => (wd.Item1.ToList(), wd.Item2))
                .Select(wd => (RemoveLowerHorizontalCurve(ref wd.Item1), wd.Item2))
                .ToList();

            // Профили витражей, у которых он редактировался
            var sketchesChanged = curtainWallsSlopeDepth
                .Where(wd => wd.Item1.SketchId.IntegerValue > 0)
                .Select(wd => (doc.GetElement(wd.Item1.SketchId) as Sketch, wd.Item2))
                .Select(wd => (wd.Item1.Profile, wd.Item2))
                .Select(wd => (wd.Profile.GetLongestCurveArray(), wd.Item2))
                .Select(wd => (wd.Item1.ToList(), wd.Item2))
                .Select(wd => (RemoveLowerHorizontalCurve(ref wd.Item1), wd.Item2))
                .ToList();

            var sketches = sketchesVirgin.Concat(sketchesChanged);

            List<(Curve Curve, double Slope)> curvesSlopes = new List<(Curve, double)>();
            foreach (var sketch in sketches)
            {
                foreach (var curve in sketch.Item1)
                {
                    curvesSlopes.Add((curve, sketch.Item2));
                }
            }

            using (Transaction setSlopesTrans = new Transaction(doc))
            {
                setSlopesTrans.Start("Назначение площадей откосов");
                foreach (Room room in rooms)
                {
                    Solid solid;
                    SpatialElement spatial = room;
                    double slopeArea = 0;
                    try
                    {
                        SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(spatial);
                        solid = results.GetGeometry();
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException)
                    {
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        continue;
                    }
                    foreach (var curveSlope in curvesSlopes)
                    {
                        var start = curveSlope.Curve.GetEndPoint(0);
                        var finish = curveSlope.Curve.GetEndPoint(1);
                        SolidCurveIntersection result = solid.IntersectWithCurve(curveSlope.Curve, intersectionOptions);
                        foreach (var intersectCurve in result)
                        {
                            slopeArea += (intersectCurve.Length * curveSlope.Slope / SharedValues.FootToMillimeters);
                        }
                    }
                    room.get_Parameter(SharedParams.PGS_SlopesArea).Set(slopeArea);
                }
                setSlopesTrans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
