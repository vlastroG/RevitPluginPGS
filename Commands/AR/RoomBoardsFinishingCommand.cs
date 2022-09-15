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
        /// Смещение контура витража в сторону помещения в футах
        /// </summary>
        private readonly double _tolerance = 1.5;

        /// <summary>
        /// Удаляет из списка кривых нижнюю горизонтальную линию
        /// </summary>
        /// <param name="curves">Список кривых</param>
        /// <returns>Список кривых без нижней горизонтальной линии</returns>
        private List<Curve> RemoveLowerHorizontalCurve(ref List<Curve> curves)
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

        /// <summary>
        /// Валидация проекта Revit на наличие необходимых общих параметров
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <returns>True, если все общие параметры присутствуют, иначе false</returns>
        private bool ValidateSharedParams(Document doc)
        {
            Guid[] _sharedParamsForWalls = new Guid[] {
            SharedParams.PGS_SlopeDepth
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForWalls))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\"" +
                    "отсутствует общий параметр PGS_Откосы_Глубина",
                    "Ошибка");
                return false;
            }
            Guid[] _sharedParamsForRooms = new Guid[] {
            SharedParams.PGS_SlopesArea,
            SharedParams.PGS_PlinthLength
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForRooms))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\"" +
                    "отсутствуют необходимые общие параметры:" +
                    "\nPGS_Откосы_Площадь" +
                    "\nPGS_Длина_Плинтус",
                    "Ошибка");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Получить кортеж кривых, составляющих профиль откосов витражей и глубины откоса по этой кривой. 
        /// Кривые формируются как копии исходных составляющих профилей, 
        /// смещенные в обе стороны от оси витражей на заданное расстояние.
        /// Предполагается, что с Solid помещения может пересечься максимум только одна из двух полученных кривых.
        /// </summary>
        /// <param name="doc">Документ, в котором происзодит поиск откосов витражей</param>
        /// <returns>Кортеж сегмента профиля откоса и его глубины</returns>
        private List<(Curve Curve, double Slope)> GetSlopeCurvesWithDepth(Document doc)
        {
            List<(Wall, double, XYZ)> curtainWallsSlopeDepth = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Where(w => (w as Wall).CurtainGrid != null)
                .Where(w => w.get_Parameter(SharedParams.PGS_SlopeDepth).AsDouble() > 0)
                .Select(w => ((w as Wall), w.get_Parameter(SharedParams.PGS_SlopeDepth).AsDouble(), (w as Wall).Orientation))
                .ToList();

            var sketchFilter = new ElementClassFilter(typeof(Sketch));

            // Создать профили витражей, у которых он не был создан
            var sketchesVirgin = curtainWallsSlopeDepth
                .Where(wd => wd.Item1.SketchId.IntegerValue < 0)
                .Select(wd => (wd.Item1.GetRectangularProfile(), wd.Item2, wd.Item3))
                .Select(wd => (wd.Item1.ToList(), wd.Item2, wd.Item3))
                .Select(wd => (RemoveLowerHorizontalCurve(ref wd.Item1), wd.Item2, wd.Item3))
                .ToList();

            // Профили витражей, у которых он редактировался
            var sketchesChanged = curtainWallsSlopeDepth
                .Where(wd => wd.Item1.SketchId.IntegerValue > 0)
                .Select(wd => (doc.GetElement(wd.Item1.SketchId) as Sketch, wd.Item2, wd.Item3))
                .Select(wd => (wd.Item1.Profile, wd.Item2, wd.Item3))
                .Select(wd => (wd.Profile.GetLongestCurveArray(), wd.Item2, wd.Item3))
                .Select(wd => (wd.Item1.ToList(), wd.Item2, wd.Item3))
                .Select(wd => (RemoveLowerHorizontalCurve(ref wd.Item1), wd.Item2, wd.Item3))
                .ToList();

            var sketches = sketchesVirgin.Concat(sketchesChanged);

            List<(Curve Curve, double Slope)> curvesSlopes = new List<(Curve, double)>();
            foreach (var sketch in sketches)
            {
                // Зполнение списка кривых составляющих провилей откосов, смещенных в обе стороны по направлению
                Transform frontTranslation = Transform.CreateTranslation(sketch.Item3 * _tolerance);
                Transform backTranslation = Transform.CreateTranslation(sketch.Item3.Negate() * _tolerance);
                double slopeDepth = sketch.Item2;
                foreach (var curve in sketch.Item1)
                {
                    Curve curveFront = curve.CreateTransformed(frontTranslation);
                    Curve curveBack = curve.CreateTransformed(backTranslation);
                    curvesSlopes.Add((curveFront, slopeDepth));
                    curvesSlopes.Add((curveBack, slopeDepth));
                }
            }
            return curvesSlopes;
        }

        /// <summary>
        /// Формирует словарь Id помещения и суммы ширин дверей,
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private IReadOnlyDictionary<ElementId, double> GetRoomIdAndDoorsWidthDict(Document doc)
        {
            var doors = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Where(d => (d as FamilyInstance).Host is Wall
                        && ((d as FamilyInstance).Host as Wall).CurtainGrid == null)
                .Where(d => d.get_Parameter(BuiltInParameter.FURNITURE_WIDTH) != null)
                .Cast<FamilyInstance>();

            Dictionary<ElementId, double> _dictRoomIdDoorsWidth = new Dictionary<ElementId, double>();
            foreach (var door in doors)
            {
                var doorWidth = door.get_Parameter(BuiltInParameter.FURNITURE_WIDTH).AsDouble();
                Element fromRoom = door.FromRoom;
                Element toRoom = door.ToRoom;
                if (fromRoom != null)
                {
                    _dictRoomIdDoorsWidth.MapIncrease(fromRoom.Id, doorWidth);
                }
                if (toRoom != null)
                {
                    _dictRoomIdDoorsWidth.MapIncrease(toRoom.Id, doorWidth);
                }
            }
            return _dictRoomIdDoorsWidth;
        }

        /// <summary>
        /// Назначает помещениям длину плинтуса
        /// </summary>
        /// <param name="doc">Документ, в котором производится подсчет</param>
        /// <param name="rooms">Обрабатываемые помещения в документе</param>
        private void SetPlinthLength(Document doc, List<Room> rooms)
        {
            var _dictRoomIdDoorsWidth = GetRoomIdAndDoorsWidthDict(doc);
            SpatialElementBoundaryOptions boundaryOptions = new SpatialElementBoundaryOptions();
            View3D view3d = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));

            using (Transaction setPlinthLength = new Transaction(doc))
            {
                setPlinthLength.Start("Длины плинтусов");
                foreach (var room in rooms)
                {
                    double noPlinthLength = 0;
                    if (_dictRoomIdDoorsWidth.ContainsKey(room.Id)) noPlinthLength += _dictRoomIdDoorsWidth[room.Id];

                    var circuits = room.GetBoundarySegments(boundaryOptions);
                    foreach (var circuit in circuits)
                    {
                        foreach (var bound in circuit)
                        {
                            var boundEl = doc.GetElement(bound.ElementId);
                            if (boundEl != null)
                            {
                                var el_bound_type_name = boundEl.GetType().Name;
                                if (el_bound_type_name == "ModelLine")
                                {
                                    var curtain_wall_behind_modelline = WorkWithGeometry
                                        .GetElementByRay_switch(
                                        doc,
                                        view3d,
                                        bound.GetCurve(), true);

                                    var curtain_wall_before_modelline = WorkWithGeometry
                                        .GetElementByRay_switch(
                                        doc,
                                        view3d,
                                        bound.GetCurve(), false);

                                    if (curtain_wall_behind_modelline == null
                                        && curtain_wall_before_modelline == null)
                                    {
                                        noPlinthLength += bound.GetCurve().Length;
                                        continue;
                                    }
                                    else if (curtain_wall_behind_modelline is Wall)
                                    {
                                        if ((curtain_wall_behind_modelline as Wall).CurtainGrid != null)
                                        {
                                            noPlinthLength += bound.GetCurve().Length;
                                            continue;
                                        }
                                    }
                                    else if (curtain_wall_before_modelline is Wall)
                                    {
                                        if ((curtain_wall_before_modelline as Wall).CurtainGrid != null)
                                        {
                                            noPlinthLength += bound.GetCurve().Length;
                                            continue;
                                        }
                                    }
                                }
                                if (boundEl is Wall)
                                {
                                    if ((boundEl as Wall).CurtainGrid != null)
                                    {
                                        noPlinthLength += bound.GetCurve().Length;
                                    }
                                }
                            }

                        }
                    }
                    double plinthLength = (room.Perimeter - noPlinthLength) * SharedValues.FootToMillimeters;
                    room.get_Parameter(SharedParams.PGS_PlinthLength).Set(plinthLength);
                }
                setPlinthLength.Commit();
            }
        }

        /// <summary>
        /// Назначает помещениям площадь откосов витражей
        /// </summary>
        /// <param name="doc">Документ, в котором происзодит транзакция</param>
        /// <param name="rooms">Обрабатываемые помещения</param>
        /// <returns>Список Id необработанных помещений (ошибок)</returns>
        private List<ElementId> SetSlopeArea(Document doc, List<Room> rooms)
        {
            SolidCurveIntersectionOptions intersectionOptions = new SolidCurveIntersectionOptions();
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            List<(Curve Curve, double Slope)> curvesSlopes = GetSlopeCurvesWithDepth(doc);
            List<ElementId> errors = new List<ElementId>();
            using (Transaction setSlopesTrans = new Transaction(doc))
            {
                setSlopesTrans.Start("Площади откосов");
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
                        errors.Add(room.Id);
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        errors.Add(room.Id);
                        continue;
                    }
                    foreach (var curveSlope in curvesSlopes)
                    {
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
            return errors;
        }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            if (!ValidateSharedParams(doc)) return Result.Cancelled;
            Selection sel = uidoc.Selection;

            // Все выбранные помещения перед запуском команды
            List<Room> rooms = sel.GetElementIds().Select(id => doc.GetElement(id))
                    .Where(e => (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(e)
                    == BuiltInCategory.OST_Rooms)
                    .Cast<Room>()
                    .Where(r => r.Area > 0)
                    .ToList();

            if (rooms.Count == 0)
            {
                try
                {
                    var filter = new SelectionFilterElementsOfCategory<Element>(
                        new List<BuiltInCategory> { BuiltInCategory.OST_Rooms },
                        false);
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

            var errors = SetSlopeArea(doc, rooms);
            SetPlinthLength(doc, rooms);

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Не удалось обработать геометрию помещений id: " +
                    $"{String.Join(", ", errors.Select(e => e.ToString()))}",
                    "Выполнено с ошибками!");
            }
            return Result.Succeeded;
        }
    }
}
