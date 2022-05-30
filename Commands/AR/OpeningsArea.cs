using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpeningsArea : IExternalCommand
    {
        /// <summary>
        /// Guid параметра ПлощадьПроемов
        /// </summary>
        private static readonly Guid _parOpeningsArea = Guid.Parse("18e3f49d-1315-415f-8359-8f045a7a8938");

        /// <summary>
        /// Название стадии, в которой подсчитываются площади проемов
        /// </summary>
        private static readonly string _phase = "Новая конструкция";

        /// <summary>
        /// Словарь Id помещений и площадей проемов, выполненных загружаемыми семействами
        /// </summary>
        private static readonly Dictionary<ElementId, double> _dict_roomId_openingsArea = new Dictionary<ElementId, double>();



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var filter_rooms = new FilteredElementCollector(doc);
            var rooms = filter_rooms
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(r => r.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase)
                .Where(r => (r as Room).Area > 0)
                .ToArray();

            var filter_windows = new FilteredElementCollector(doc);
            var windows = filter_windows
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(w => w.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase);

            var filter_doors = new FilteredElementCollector(doc);
            var doors = filter_doors
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(d => d.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase);

            var openings = doors.Concat(windows);
            var phaseOfRooms = doc.GetElement(rooms.FirstOrDefault().get_Parameter(BuiltInParameter.PHASE_CREATED).AsElementId()) as Phase;

            foreach (var opening in openings)
            {
                var opening_fam_inst = opening as FamilyInstance;
                var opening_area = WorkWithGeometry.GetOpeningArea(doc, opening_fam_inst);
                _dict_roomId_openingsArea.MapIncrease(opening_fam_inst.get_FromRoom(phaseOfRooms).Id, opening_area);
                _dict_roomId_openingsArea.MapIncrease(opening_fam_inst.get_ToRoom(phaseOfRooms).Id, opening_area);
            }

            foreach (var room in rooms)
            {
                double room_area = 0;
                if (_dict_roomId_openingsArea.ContainsKey(room.Id))
                {
                    room_area = _dict_roomId_openingsArea[room.Id];
                }
            }

            return Result.Succeeded;
        }
    }
}
