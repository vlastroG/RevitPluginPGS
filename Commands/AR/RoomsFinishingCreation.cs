using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.AR.DTO;
using MS.GUI.AR;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MS.Commands.AR
{
    /// <summary>
    /// Перечисление для определения способа построения отделочных стен
    /// </summary>
    public enum FinWallsHeight
    {
        /// <summary>
        /// Заданная высота отделочной стены
        /// </summary>
        ByInput,
        /// <summary>
        /// Высота отделочной стены по высоте помещения
        /// </summary>
        ByRoom,
        /// <summary>
        /// Высота отделочной стены по элементу
        /// </summary>
        ByElement
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishingCreation : IExternalCommand
    {
        /// <summary>
        /// Значение параметра PGS_НаименованиеОтделки по умолчанию для строки во всплывающем окне,
        /// если в модели null или пустая строка
        /// </summary>
        private const string _defaultName = "НЕ НАЗНАЧЕНО";

        /// <summary>
        /// Получить помещения с ненулевой площадью с заполненным параметром PGS_ТипОтделкиСтен для дальнейшего расчета
        /// </summary>
        /// <param name="uidoc"></param>
        /// <returns>Список валидных помещений, 
        /// если операция отменена или пользователь ничего не выбрал, то null</returns>
        private List<Room> GetRooms(in UIDocument uidoc)
        {
            Document doc = uidoc.Document;
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
                    if (rooms.Count == 0) return null;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return null;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    MessageBox.Show("Нельзя выбирать элементы на текущем виде." +
                        "\nЛибо выделите помещения в спецификации, либо перейдите на вид, " +
                        "где можно выбрать помещения вручную.", "Ошибка");
                    return null;
                }
            }
            return rooms;
        }

        private bool ValidateSharedParams(in Document doc)
        {
            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.PGS_FinishingName
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\" " +
                    "в типе отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки",
                    "Ошибка");
                return false;
            }
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_StructuralColumns,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Несущие колонны\" " +
                    "в типе отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки",
                    "Ошибка");
                return false;
            }
            return true;
        }

        private View3D Get3DView(in UIDocument uidoc)
        {
            View3D view3d
                    = new FilteredElementCollector(uidoc.Document)
                      .OfClass(typeof(View3D))
                      .Cast<View3D>()
                      .FirstOrDefault<View3D>(
                        e => e.Name.Equals("{3D}"));
            if (null == view3d)
            {
                MessageBox.Show("Не найден {3D} вид по умолчанию", "Ошибка");
                return null;
            }
            return view3d;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if (!ValidateSharedParams(doc)) return Result.Cancelled;

            View3D view3d = Get3DView(uidoc);
            if (view3d is null) return Result.Cancelled;

            List<Room> rooms = GetRooms(uidoc);
            if (rooms == null) return Result.Cancelled;

            List<List<CurveLoop>> roomBorderLoops = new List<List<CurveLoop>>();
            List<string> descriptions = new List<string>();
            List<(BoundarySegment Segment, ElementId LevelId, double HRoom, double HElem, double RoomBottomOffset)> validSegmentsAndH =
                new List<(BoundarySegment, ElementId, double, double, double)>();
            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(
                    new SpatialElementBoundaryOptions());

                // Создать extension метод для получения List<CurveLoop> из помещения
                roomBorderLoops.Add(loops);

                foreach (IList<BoundarySegment> loop in loops)
                {
                    for (int i = 0; i < loop.Count; i++)
                    {
                        Element borderEl = doc.GetElement(loop[i].ElementId);
                        Element e = null;
                        if (borderEl is ModelLine)
                        {
                            e = WorkWithGeometry.GetElementByRay_switch(doc,
                                        view3d,
                                        loop[i].GetCurve(), true);
                        }
                        else
                        {
                            e = borderEl;
                        }
                        if (null == e)
                        {
                            continue;
                        }
                        if (!(e is RevitLinkInstance)
                            && !(e is Wall)
                            && (WorkWithParameters.GetCategoryIdAsInteger(e)
                                != (int)BuiltInCategory.OST_StructuralColumns))
                        {
                            // Получение границ помещений, которые образованы только связями,
                            // стенами и несущими колоннами
                            continue;
                        }
                        double roomH = room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
                        double elemH = 0;
                        if (!(e is RevitLinkInstance))
                        {
                            var bBox = e.get_Geometry(new Options()).GetBoundingBox();
                            elemH = Math.Round((bBox.Max.Z - bBox.Min.Z) * SharedValues.FootToMillimeters)
                                / SharedValues.FootToMillimeters;
                        }
                        else
                        {
                            elemH = roomH;
                        }
                        var bottomOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                        validSegmentsAndH.Add((loop[i], room.LevelId, roomH, elemH, bottomOffset));
                        try
                        {
                            string finName = _defaultName;
                            if (e is Wall)
                            {
                                finName = (e as Wall).WallType.get_Parameter(SharedParams.PGS_FinishingName)
                                    .AsValueString();
                            }
                            else if (e is FamilyInstance)
                            {
                                finName = (e as FamilyInstance).Symbol.get_Parameter(SharedParams.PGS_FinishingName)
                                    .AsValueString();
                            }
                            if (String.IsNullOrEmpty(finName))
                            {
                                finName = _defaultName;
                            }
                            if (!descriptions.Contains(finName))
                            {
                                descriptions.Add(finName);
                            }
                        }
                        catch (System.NullReferenceException)
                        {
                            continue;
                        }
                    }
                }
            }

            var wallTypesAll = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsElementType()
                .Select(w => w as WallType)
                .Where(w => w.GetCompoundStructure() != null)
                .ToList();
            List<WallTypeFinishingDto> dtos = descriptions.Select(d => new WallTypeFinishingDto(d)).ToList();

            var ceilingTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Ceilings)
                .WhereElementIsElementType()
                .Cast<CeilingType>()
                .ToList();

            var ui = new FinishingCreation(dtos, wallTypesAll, ceilingTypes);
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return Result.Cancelled;
            }
            IReadOnlyDictionary<string, WallType> dictWT = ui.DictWallTypeByFinName;
            WallType wtDefault = null;
            List<(Element, Element)> pairsToJoin = new List<(Element, Element)>();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание отделочных стен");

                for (int i = 0; i < validSegmentsAndH.Count; i++)
                {
                    Element borderEl = doc.GetElement(validSegmentsAndH[i].Segment.ElementId);
                    Element e = null;
                    if (borderEl is ModelLine)
                    {
                        e = WorkWithGeometry.GetElementByRay_switch(doc,
                                    view3d,
                                    validSegmentsAndH[i].Segment.GetCurve(), true);
                    }
                    else
                    {
                        e = borderEl;
                    }
                    if (null == e)
                    {
                        continue;
                    }
                    if (!(e is RevitLinkInstance)
                        && !(e is Wall)
                        && (WorkWithParameters.GetCategoryIdAsInteger(e)
                            != (int)BuiltInCategory.OST_StructuralColumns))
                    {
                        // Получение границ помещений, которые образованы только связями,
                        // стенами и несущими колоннами
                        continue;
                    }

                    var wt = wtDefault;
                    double offset = 0;
                    string finName = _defaultName;
                    double elemBottomOffset = 0;
                    if (e is Wall)
                    {
                        elemBottomOffset = e
                            .get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                        finName = (e as Wall).WallType
                            .get_Parameter(SharedParams.PGS_FinishingName).AsValueString();
                    }
                    else if (e is FamilyInstance)
                    {
                        elemBottomOffset = e
                            .get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
                        finName = (e as FamilyInstance).Symbol
                            .get_Parameter(SharedParams.PGS_FinishingName).AsValueString();
                    }
                    if (String.IsNullOrEmpty(finName))
                    {
                        finName = _defaultName;
                    }
                    wt = dictWT[finName];
                    if (ReferenceEquals(null, wt))
                    {
                        continue;
                    }
                    offset = wt.Width / 2;

                    try
                    {
                        Curve curve = validSegmentsAndH[i].Segment.GetCurve();
                        Curve wallGrid = curve.CreateOffset(-offset, XYZ.BasisZ);
                        double height = 0;
                        double bottomOffset = 0;
                        switch (ui.FinWallsHeightType)
                        {
                            case FinWallsHeight.ByRoom:
                                height = validSegmentsAndH[i].HRoom;
                                bottomOffset = validSegmentsAndH[i].RoomBottomOffset;
                                break;
                            case FinWallsHeight.ByElement:
                                height = validSegmentsAndH[i].HElem;
                                bottomOffset = elemBottomOffset - validSegmentsAndH[i].RoomBottomOffset;
                                break;
                            case FinWallsHeight.ByInput:
                                height = ui.InputWallsHeight / SharedValues.FootToMillimeters;
                                bottomOffset = validSegmentsAndH[i].RoomBottomOffset;
                                break;
                        }
                        var wall = Wall.Create(doc, wallGrid, wt.Id, validSegmentsAndH[i].LevelId, height, bottomOffset, false, false);
                        (Element, Element) toJoinPair = (e, wall);
                        pairsToJoin.Add(toJoinPair);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        continue;
                    }

                }
                trans.Commit();
            }
            List<(Element, Element)> errorsList = new List<(Element, Element)>();
            using (Transaction joining = new Transaction(doc))
            {
                joining.Start("Соединение стен");
                foreach (var pair in pairsToJoin)
                {
                    try
                    {
                        JoinGeometryUtils.JoinGeometry(doc, pair.Item1, pair.Item2);
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException)
                    {
                        errorsList.Add(pair);
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidObjectException)
                    {
                        continue;
                    }
                }
                joining.Commit();
            }
            if (errorsList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Не удалось соединить элементы! Id:");
                var errsGroups = errorsList.Where(errs => errs.Item1.IsValidObject && errs.Item2.IsValidObject)
                    .GroupBy(group => group.Item1.Id);
                foreach (var groupId in errsGroups)
                {
                    if (groupId.Count() > 10)
                    {
                        sb.AppendLine($"{groupId.Key} и более 10 элементов;");
                    }
                    else
                    {
                        foreach (var item in groupId)
                        {
                            sb.AppendLine($"{item.Item1.Id} и {item.Item2.Id};");
                        }
                    }
                }
                MessageBox.Show(sb.ToString(), "Ошибка");
            }

            return Result.Succeeded;
        }
    }
}
