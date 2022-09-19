using Autodesk.Revit.ApplicationServices;
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
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishing : IExternalCommand
    {
        /// <summary>
        /// Валидация проекта Revit на наличие необходимых общих параметров
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <returns>True, если все общие параметры присутствуют, иначе false</returns>
        private bool ValidateSharedParams(Document doc)
        {
            Guid[] _sharedParamsForWalls = new Guid[] {
            SharedParams.ADSK_RoomNumberInApartment
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForWalls))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\"" +
                    "в экземпляре отсутствует необходимый общий параметр:" +
                    "\nADSK_Номер помещения квартиры",
                    "Ошибка");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Команда для назначения элементам, образующим границы помещения и являющихся внутренней отделкой, 
        /// номер этого помещения. Команда в разработке, необходимо определиться с параметрами, 
        /// куда писать номера помещений и проверить логику работы функции нахождения элементов.
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
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

            View3D view3d
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));

            if (null == view3d)
            {
                MessageBox.Show("Не нейден {3D} вид по умолчанию", "Ошибка");

                return Result.Failed;
            }

            using (Transaction rNumsToWallsTrans = new Transaction(doc))
            {
                rNumsToWallsTrans.Start("Номера помещений отделочным стенам");
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
                                e = WorkWithGeometry.GetElementByRay(doc, view3d,
                                  seg.GetCurve());
                            }
                            if (!(e is Wall)) continue;
                            string rNum = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
                            e.get_Parameter(SharedParams.ADSK_RoomNumberInApartment).Set(rNum);
                        }
                    }
                }
                rNumsToWallsTrans.Commit();
            }
            return Result.Succeeded;
        }


    }
}
