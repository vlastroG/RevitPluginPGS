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
using System.Threading.Tasks;
using System.Windows;

namespace MS.Commands.AR
{
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
        private const string _defaultName = "НЕ НАЗНАЧЕНО";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.PGS_FinishingName
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\" " +
                    "отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки",
                    "Ошибка");
                return Result.Cancelled;
            }
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_StructuralColumns,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Несущие колонны\" " +
                    "отсутствуют необходимые общие параметры:" +
                    "\nPGS_НаименованиеОтделки",
                    "Ошибка");
                return Result.Cancelled;
            }

            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { BuiltInCategory.OST_Rooms },
                false);
            List<Element> rooms = null;
            try
            {
                rooms = uidoc.Selection
                    .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        "Выберите помещения для отделки")
                    .Select(e => doc.GetElement(e))
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            List<string> descriptions = new List<string>();
            List<(BoundarySegment Segment, ElementId LevelId, double HRoom, double HElem, double BottomOffset)> validSegmentsAndH =
                new List<(BoundarySegment, ElementId, double, double, double)>();
            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(
                    new SpatialElementBoundaryOptions());

                foreach (IList<BoundarySegment> loop in loops)
                {
                    foreach (BoundarySegment seg in loop)
                    {
                        Element e = doc.GetElement(seg.ElementId);
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
                        var bottomOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                        validSegmentsAndH.Add((seg, room.LevelId, roomH, elemH, bottomOffset));
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

            var ui = new FinishingCreation(dtos, wallTypesAll);
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return Result.Cancelled;
            }
            IReadOnlyDictionary<string, WallType> dictWT = ui.DictWallTypeByFinName;
            WallType wtDefault = null;
            List<Tuple<Element, Element>> pairsToJoin = new List<Tuple<Element, Element>>();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание отделочных стен");

                foreach (var segTuple in validSegmentsAndH)
                {
                    Element e = doc.GetElement(segTuple.Segment.ElementId);

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
                    if (e is Wall)
                    {
                        finName = (e as Wall).WallType
                            .get_Parameter(SharedParams.PGS_FinishingName).AsValueString();
                    }
                    else if (e is FamilyInstance)
                    {
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
                        Curve curve = segTuple.Segment.GetCurve();
                        Curve wallGrid = curve.CreateOffset(-offset, XYZ.BasisZ);
                        double height = 0;
                        double bottomOffset = 0;
                        switch (ui.FinWallsHeightType)
                        {
                            case FinWallsHeight.ByRoom:
                                height = segTuple.HRoom;
                                bottomOffset = segTuple.BottomOffset;
                                break;
                            case FinWallsHeight.ByElement:
                                height = segTuple.HElem;
                                bottomOffset = 0;
                                break;
                            case FinWallsHeight.ByInput:
                                height = ui.InputHeight / SharedValues.FootToMillimeters;
                                bottomOffset = segTuple.BottomOffset;
                                break;
                        }
                        var LevelIdTest = segTuple.LevelId;
                        var wall = Wall.Create(doc, wallGrid, wt.Id, segTuple.LevelId, height, bottomOffset, false, false);
                        Tuple<Element, Element> toJoinPair = new Tuple<Element, Element>(e, wall);
                        pairsToJoin.Add(toJoinPair);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        continue;
                    }

                }
                trans.Commit();
            }
            List<Tuple<Element, Element>> errorsList = new List<Tuple<Element, Element>>();
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
                }
                joining.Commit();
            }
            if (errorsList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Не удалось соединить элементы! Id:");
                foreach (var error in errorsList)
                {
                    sb.AppendLine($"{error.Item1.Id}, {error.Item2.Id}");
                }
                MessageBox.Show(sb.ToString(), "Ошибка");
            }

            return Result.Succeeded;
        }
    }
}
