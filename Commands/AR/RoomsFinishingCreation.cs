using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Office.Interop.Excel;
using MS.Commands.AR.DTO;
using MS.Commands.AR.DTO.FinishingCreationCmd;
using MS.Commands.AR.Enums;
using MS.GUI.AR;
using MS.GUI.ViewModels.AR;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishingCreation : IExternalCommand
    {
        /// <summary>
        /// Значение параметра PGS_НаименованиеОтделки по умолчанию для строки во всплывающем окне,
        /// если в модели null или пустая строка
        /// </summary>
        private const string _defaultName = "НЕ НАЗНАЧЕНО";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            if (!ValidateSharedParams(doc)) return Result.Cancelled;

            View3D view3d = Get3DView(uidoc);
            if (view3d is null) return Result.Cancelled;

            List<Room> rooms = GetRooms(uidoc);
            if (rooms == null) return Result.Cancelled;

            (List<WallDto> wallDtos, List<WallTypeFinishingDto> descriptions) = GetWallsCreationData(doc, rooms, view3d, spatialElementBoundaryOptions);

            (List<WallType> wallTypes, List<CeilingType> ceilingTypes) = GetWallsAndCeilingsTypes(doc);

            var settings = new FinishingCreationViewModel(wallTypes, ceilingTypes, descriptions);
            var ui = new FinishingCreation(settings);
            ui.ShowDialog();
            if (ui.DialogResult != true || (!settings.CreateWalls && !settings.CreateCeiling))
            {
                return Result.Cancelled;
            }
            bool createWalls = settings.CreateWalls;
            bool createCeilings = settings.CreateCeiling;
            if (createWalls)
            {
                CreateWalls(doc, wallDtos, settings, view3d);
            }
            if (createCeilings && !(settings.SelectedCeilingType is null))
            {
                var errorRoomsForCeiling = CreateCeilings(doc, rooms, settings, spatialElementBoundaryOptions);
                if (errorRoomsForCeiling.Count > 0)
                {
                    string ids = String.Join(", ", errorRoomsForCeiling.Select(e => e.ToString()));
                    MessageBox.Show($"Ошибка: Не удалось создать потолок в следующих помещениях Id: {ids}.",
                        "Создание потолков");
                }
            }

            return Result.Succeeded;
        }

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
            Guid[] _sharedParamsForWalls = new Guid[] {
            SharedParams.PGS_FinishingName,
            SharedParams.ADSK_RoomNumberInApartment,
            SharedParams.PGS_FinishingTypeOfWalls
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForWalls))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\" " +
                    "в типе отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки" +
                    "\nВ экземпляре:" +
                    "\nADSK_Номер помещения квартиры" +
                    "\nPGS_ТипОтделкиСтен",
                    "Ошибка");
                return false;
            }

            Guid[] _sharedParamsForCeilings = new Guid[] {
            SharedParams.ADSK_RoomNumberInApartment,
            SharedParams.PGS_FinishingTypeOfWalls
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Ceilings,
                _sharedParamsForCeilings))
            {
                MessageBox.Show("В текущем проекте у категории \"Потолки\" " +
                    "\nв экземпляре отсутствуют необходимые общие параметры:" +
                    "\nADSK_Номер помещения квартиры" +
                    "\nPGS_ТипОтделкиСтен",
                    "Ошибка");
                return false;
            }

            Guid[] _sharedParamsForColumns = new Guid[] {
            SharedParams.PGS_FinishingName
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_StructuralColumns,
                _sharedParamsForColumns))
            {
                MessageBox.Show("В текущем проекте у категории \"Несущие колонны\" " +
                    "в типе отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки",
                    "Ошибка");
                return false;
            }

            Guid[] _sharedParamsForRooms = new Guid[] {
            SharedParams.PGS_FinishingTypeOfWalls
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForRooms))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\" " +
                    "в экземпляре отсутствуют необходимые общие параметры:" +
                    "\nPGS_ТипОтделкиСтен",
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

        private void CreateWalls(
            in Document doc,
            in List<WallDto> wallDtos,
            in FinishingCreationViewModel settings,
            in View3D view3d)
        {
            WallType wtDefault = null;
            List<(ElementId, ElementId)> pairsToJoin = new List<(ElementId, ElementId)>();
            IReadOnlyDictionary<string, WallType> dictWT =
                settings
                .DTOs
                .ToDictionary(dto => dto.FinishingName, dto => dto.WallType);

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание отделочных стен");

                for (int i = 0; i < wallDtos.Count; i++)
                {
                    var wt = wtDefault;
                    double wallGridOffset = 0;
                    wt = dictWT[wallDtos[i].FinTypeName];
                    if (ReferenceEquals(null, wt))
                    {
                        continue;
                    }
                    wallGridOffset = wt.Width / 2;

                    try
                    {
                        Curve wallGrid = wallDtos[i].Curve.CreateOffset(-wallGridOffset, XYZ.BasisZ);
                        double height = 0;
                        double bottomOffset = 0;
                        switch (settings.WallsHeightType)
                        {
                            case FinWallsHeight.ByRoom:
                                height = wallDtos[i].HRoom;
                                bottomOffset = wallDtos[i].RoomBottomOffset;
                                break;
                            case FinWallsHeight.ByElement:
                                height = wallDtos[i].HElem;
                                bottomOffset = wallDtos[i].ElementBottomOffset - wallDtos[i].RoomBottomOffset;
                                break;
                            case FinWallsHeight.ByInput:
                                height = settings.WallsHeight / SharedValues.FootToMillimeters;
                                bottomOffset = wallDtos[i].RoomBottomOffset;
                                break;
                        }
                        var wall = Wall.Create(doc, wallGrid, wt.Id, wallDtos[i].LevelId, height, bottomOffset, false, false);
                        wall.get_Parameter(SharedParams.ADSK_RoomNumberInApartment).Set(wallDtos[i].RoomNumber);
                        wall.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).Set(wallDtos[i].RoomFinTypeWalls);
                        foreach (var elemToJoin in wallDtos[i].ElementsToJoin)
                        {
                            (ElementId, ElementId) toJoinPair = (elemToJoin, wall.Id);
                            if (!pairsToJoin.Contains(toJoinPair))
                            {
                                pairsToJoin.Add(toJoinPair);
                            }
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        continue;
                    }

                }
                trans.Commit();
            }
            JoinWalls(doc, pairsToJoin);
        }

        private void JoinWalls(in Document doc, List<(ElementId, ElementId)> pairsToJoin)
        {
            List<(ElementId, ElementId)> errorsList = new List<(ElementId, ElementId)>();
            using (Transaction joining = new Transaction(doc))
            {
                joining.Start("Соединение стен");
                foreach (var pair in pairsToJoin)
                {
                    try
                    {
                        JoinGeometryUtils.JoinGeometry(doc, doc.GetElement(pair.Item1), doc.GetElement(pair.Item2));
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
                var errsGroups = errorsList.Where(errs => (errs.Item1.IntegerValue > 0) && (errs.Item2.IntegerValue > 0))
                    .GroupBy(group => group.Item1);
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
                            sb.AppendLine($"{item.Item1} и {item.Item2};");
                        }
                    }
                }
                MessageBox.Show(sb.ToString(), "Ошибка");
            }
        }

        /// <summary>
        /// Получает значение параметра PGS_ТипОтделкиСтен у элемента
        /// </summary>
        /// <param name="elem">Элемент, для которого находится значение</param>
        /// <returns>Значение параметра элемента PGS_ТипОтделкиСтен</returns>
        private string GetFinName(in Element elem)
        {
            string finName = _defaultName;
            try
            {
                if (elem is Wall)
                {
                    finName = (elem as Wall).WallType
                        .get_Parameter(SharedParams.PGS_FinishingName).AsValueString();
                }
                else if (elem is FamilyInstance)
                {
                    finName = (elem as FamilyInstance).Symbol
                        .get_Parameter(SharedParams.PGS_FinishingName).AsValueString();
                }
            }
            catch (System.NullReferenceException)
            {
                finName = _defaultName;
            }
            if (String.IsNullOrEmpty(finName))
            {
                finName = _defaultName;
            }
            return finName;
        }

        /// <summary>
        /// Получить высоту элемента
        /// </summary>
        /// <param name="element"></param>
        /// <param name="roomHeight">Высота помещения, границу которого образует элемент</param>
        /// <param name="opts"></param>
        /// <returns>высота элемента. Если это связь, то будет возвращена высота помещения</returns>
        private double GetElemHeight(in Element element, double roomHeight, in Options opts)
        {
            double elemH = 0;
            if (!(element is RevitLinkInstance))
            {
                var bBox = element.get_Geometry(opts).GetBoundingBox();
                elemH = Math.Round((bBox.Max.Z - bBox.Min.Z) * SharedValues.FootToMillimeters)
                    / SharedValues.FootToMillimeters;
            }
            else
            {
                elemH = roomHeight;
            }
            return elemH;
        }

        /// <summary>
        /// Получить смещение снизу для отделываемого элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Смещение снизу</returns>
        private double GetElemBottomOffset(Element element)
        {
            double elemBottomOffset = 0;
            if (element is Wall)
            {
                elemBottomOffset = element
                    .get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
            }
            else if (element is FamilyInstance)
            {
                elemBottomOffset = element
                    .get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
            }
            return elemBottomOffset;
        }

        /// <summary>
        /// Находит элемент, который образует границу помещения
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="boundarySegment"></param>
        /// <param name="view3D"></param>
        /// <returns></returns>
        private Element GetBoundaryElement(in Document doc, in BoundarySegment boundarySegment, in View3D view3D)
        {
            Element borderEl = doc.GetElement(boundarySegment.ElementId);
            Element e = null;
            if (borderEl is ModelLine)
            {
                e = WorkWithGeometry.GetElementByRay_switch(doc,
                            view3D,
                            boundarySegment.GetCurve(), true);
            }
            else
            {
                e = borderEl;
            }
            return e;
        }

        private (List<WallDto> WallDtos, List<WallTypeFinishingDto> Descriptions) GetWallsCreationData(
            in Document doc,
            in List<Room> rooms,
            in View3D view3d,
            in SpatialElementBoundaryOptions spatialElementBoundaryOptions)
        {
            List<WallTypeFinishingDto> descriptions = new List<WallTypeFinishingDto>();
            List<WallDto> wallDtos = new List<WallDto>();
            Options opts = new Options();
            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(
                    spatialElementBoundaryOptions);
                var roomBottomOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                string roomNumber = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
                string roomFinTypeName = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                foreach (IList<BoundarySegment> loop in loops)
                {
                    string finNamePrev = _defaultName;
                    double elemHPrev = 0;
                    Curve curvePrev = null;
                    double elemBottomOffsetPrev = 0;
                    List<ElementId> elementsToJoin = new List<ElementId>();
                    for (int i = 0; i < loop.Count; i++)
                    {
                        Curve curveCurrent = loop[i].GetCurve();
                        Element e = GetBoundaryElement(doc, loop[i], view3d);
                        //int eIdInt = e.Id.IntegerValue; 1057189
                        if (null == e)
                        {
                            continue;
                        }
                        if (!(e is RevitLinkInstance)
                            && !(e is Wall)
                            && (WorkWithParameters.GetCategoryIdAsInteger(e)
                                != (int)BuiltInCategory.OST_StructuralColumns))
                        {
                            continue;
                        }
                        string finNameCurrent = GetFinName(e);
                        WallTypeFinishingDto finCurrent = new WallTypeFinishingDto(finNameCurrent);
                        if (!descriptions.Contains(finCurrent))
                        {
                            descriptions.Add(finCurrent);
                        }
                        double roomH = room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
                        double elemH = GetElemHeight(e, roomH, opts);
                        double elemBottomOffset = GetElemBottomOffset(e);
                        bool isCurveAdded = (finNameCurrent == finNamePrev) ? CurveExtension.AppendCurve(ref curvePrev, curveCurrent) : false;
                        if (i == 0 || (curvePrev is null))
                        {
                            finNamePrev = finNameCurrent;
                            curvePrev = curveCurrent;
                            elemHPrev = elemH;
                            elemBottomOffsetPrev = elemBottomOffset;
                            elementsToJoin.Add(e.Id);
                            continue;
                        }
                        if ((i != loop.Count - 1) && !isCurveAdded)
                        {
                            elementsToJoin.DistinctBy(t => t.IntegerValue);
                            wallDtos.Add(new WallDto(
                                curvePrev,
                                room.LevelId,
                                finNamePrev,
                                roomH,
                                elemHPrev,
                                roomBottomOffset,
                                elemBottomOffsetPrev,
                                elementsToJoin,
                                roomNumber,
                                roomFinTypeName));
                            finNamePrev = finNameCurrent;
                            curvePrev = curveCurrent;
                            elemHPrev = elemH;
                            elemBottomOffsetPrev = elemBottomOffset;
                            elementsToJoin.Clear();
                            elementsToJoin.Add(e.Id);
                            continue;
                        }
                        if (i == loop.Count - 1)
                        {
                            if ((finNameCurrent == finNamePrev) && isCurveAdded)
                            {
                                finNamePrev = finNameCurrent;
                                elemHPrev = elemH;
                                elemBottomOffsetPrev = elemBottomOffset;
                                elementsToJoin.Add(e.Id);
                                elementsToJoin.DistinctBy(t => t.IntegerValue);
                                wallDtos.Add(new WallDto(
                                    curvePrev,
                                    room.LevelId,
                                    finNamePrev,
                                    roomH,
                                    elemHPrev,
                                    roomBottomOffset,
                                    elemBottomOffsetPrev,
                                    elementsToJoin,
                                    roomNumber,
                                    roomFinTypeName));
                            }
                            else
                            {
                                elementsToJoin.DistinctBy(t => t.IntegerValue);
                                wallDtos.Add(new WallDto(
                                    curvePrev,
                                    room.LevelId,
                                    finNamePrev,
                                    roomH,
                                    elemHPrev,
                                    roomBottomOffset,
                                    elemBottomOffsetPrev,
                                    elementsToJoin,
                                    roomNumber,
                                    roomFinTypeName));
                                finNamePrev = finNameCurrent;
                                curvePrev = curveCurrent;
                                elemHPrev = elemH;
                                elemBottomOffsetPrev = elemBottomOffset;
                                elementsToJoin.Clear();
                                elementsToJoin.Add(e.Id);
                                wallDtos.Add(new WallDto(
                                    curvePrev,
                                    room.LevelId,
                                    finNamePrev,
                                    roomH,
                                    elemHPrev,
                                    roomBottomOffset,
                                    elemBottomOffsetPrev,
                                    elementsToJoin,
                                    roomNumber,
                                    roomFinTypeName));
                            }
                        }
                        elementsToJoin.Add(e.Id);
                    }
                }
            }
            return (wallDtos, descriptions);
        }

        private (List<WallType> WallTypes, List<CeilingType> CeilingTypes) GetWallsAndCeilingsTypes(in Document doc)
        {
            var wallTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsElementType()
                .Select(w => w as WallType)
                .Where(w => w.GetCompoundStructure() != null)
                .ToList();
            var ceilingTypes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Ceilings)
                .WhereElementIsElementType()
                .Cast<CeilingType>()
                .ToList();
            return (wallTypes, ceilingTypes);
        }

        /// <summary>
        /// Создает потолки по помещениям по заданной высоте или по высоте помещения
        /// </summary>
        /// <param name="doc">Документ, в котором происходит транзакция</param>
        /// <param name="rooms">Список помещений, в которых будет создан потолок</param>
        /// <param name="ui">Форма для ввода настроект генерации отделки</param>
        /// <param name="opts">Опции для получения геометрии помещений</param>
        /// <returns>Список Id комнат, в которых не удалось построить потолок</returns>
        private List<ElementId> CreateCeilings(
            in Document doc,
            in List<Room> rooms,
            in FinishingCreationViewModel settings,
            in SpatialElementBoundaryOptions opts)
        {
            List<ElementId> errorRoomsForCeilings = new List<ElementId>();
            using (Transaction ceilingsCreationTrans = new Transaction(doc))
            {
                ceilingsCreationTrans.Start("Создание потолков");
                foreach (Room room in rooms)
                {
                    try
                    {
                        IList<CurveLoop> curveLoops = room.GetCurveLoops(opts).Select(loop => loop.Simplify()).ToList();
                        double height = 0;
                        if (settings.CeilingHeightByRoom)
                        {
                            height = room.get_Parameter(BuiltInParameter.ROOM_HEIGHT).AsDouble();
                        }
                        else
                        {
                            height = settings.CeilingHeight / SharedValues.FootToMillimeters;
                        }
                        ElementId ceilingTypeId = settings.SelectedCeilingType.Id;
                        ElementId levelId = room.LevelId;
                        string roomNumber = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
                        string roomFinTypeWalls = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                        var ceiling = Ceiling.Create(doc, curveLoops, ceilingTypeId, levelId);
                        ceiling.get_Parameter(SharedParams.ADSK_RoomNumberInApartment).Set(roomNumber);
                        ceiling.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).Set(roomFinTypeWalls);
                        ceiling.get_Parameter(BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM).Set(height);
                    }
                    catch (Exception)
                    {
                        errorRoomsForCeilings.Add(room.Id);
                    }
                }
                ceilingsCreationTrans.Commit();
            }
            return errorRoomsForCeilings;
        }
    }
}
