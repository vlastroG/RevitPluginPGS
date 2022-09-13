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

namespace MS.Commands.AR
{
    /// <summary>
    /// Команда для подсчета отделки откосов и плинтусов в помещениях
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomBoardsFinishingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
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

            List<Element> curtainWalls = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Where(w => (w as Wall).CurtainGrid != null)
                .Where(w => w.get_Parameter(SharedParams.PGS_SlopeDepth).AsDouble() > 0)
                .ToList();

            var sketchFilter = new ElementClassFilter(typeof(Sketch));

            List<IEnumerable<Element>> sketchesVirgin = null;
            // Создать профили витражей, у которых он не был создан
            sketchesVirgin = curtainWalls.Cast<Wall>()
                .Where(w => w.SketchId.IntegerValue < 0)
                .Select(w => w.CreateProfileSketch())
                .Select(s => s.SketchPlane)
                .Select(sp => sp.GetDependentElements(sketchFilter).Select(lineId => doc.GetElement(lineId)))
                .ToList();

            // Профили витражей, у которых он редактировался
            var sketchesChanged = curtainWalls.Cast<Wall>()
                .Where(w => w.SketchId.IntegerValue > 0)
                .Select(w => doc.GetElement(w.SketchId) as Sketch)
                .Select(s => s.Profile)
                .Select(sp => sp.GetLongestCurveArray())
                .ToList();

            foreach (Room room in rooms)
            {

            }
            return Result.Succeeded;
        }
    }
}
