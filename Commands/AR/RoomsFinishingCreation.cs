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
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishingCreation : IExternalCommand
    {
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
            List<BoundarySegment> validSegments = new List<BoundarySegment>();
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
                        // 3101789
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
                        validSegments.Add(seg);
                        try
                        {
                            string finName = String.Empty;
                            if (e is Wall)
                            {
                                finName = (e as Wall).WallType.get_Parameter(SharedParams.PGS_FinishingName)
                                    .AsValueString() ?? String.Empty;
                            }
                            else if (e is FamilyInstance)
                            {
                                finName = (e as FamilyInstance).Symbol.get_Parameter(SharedParams.PGS_FinishingName)
                                    .AsValueString() ?? String.Empty;
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

            // 2. Добавить вывод окна для назначений типов отделочных слоев---------------------------
            var wallTypesAll = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsElementType()
                .Select(w => w as WallType)
                .ToList();
            List<WallTypeFinishingDto> dtos = descriptions.Select(d => new WallTypeFinishingDto(d)).ToList();

            var ui = new FinishingCreation(dtos, wallTypesAll);
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return Result.Cancelled;
            }

            var uiTestGet = ui.Dtos;

            var wtTest = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsElementType()
                .ToArray()[15] as WallType;

            List<Tuple<Element, Element>> pairsToJoin = new List<Tuple<Element, Element>>();

            var offset = wtTest.GetCompoundStructure().GetWidth() / 2;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание отделочных стен");

                foreach (BoundarySegment seg in validSegments)
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

                    try
                    {
                        Curve curve = seg.GetCurve();
                        Line line1 = (Line)curve;
                        Line line = (Line)curve.CreateOffset(-offset, XYZ.BasisZ);

                        var wall = Wall.Create(doc, line, wtTest.Id, e.LevelId, 10, 0, false, false);
                        Tuple<Element, Element> item = new Tuple<Element, Element>(e, wall);
                        pairsToJoin.Add(item);
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
