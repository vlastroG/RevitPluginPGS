using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using MS.RevitCommands.AR.DTO;
using MS.GUI.AR;
using MS.GUI.ViewModels.AR;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MS.RevitCommands.AR
{
    /// <summary>
    /// Скрипт по подсчету площадей проемов в помещении.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpeningsArea : IExternalCommand
    {
        /// <summary>
        /// Название стадии, в которой подсчитываются площади проемов
        /// </summary>
        private static string _phase = "Новая конструкция";

        /// <summary>
        /// Опции подсчета сегментов границ помещений
        /// </summary>
        private static readonly SpatialElementBoundaryOptions _boundaryOptions = new SpatialElementBoundaryOptions();

        /// <summary>
        /// Допуск на поиск витражей возле комнат.
        /// Допуск означает максимальное расстояние от середины оси витража
        /// до границы геометрии помещения.
        /// </summary>
        private static readonly double _curtain_wall_intersect_tolerance = 2;

        private static List<RoomDto> NotCalculatedRooms = new List<RoomDto>();

        /// <summary>
        /// Подсчет и назначение площадей проемов в параметр помещения "ADSK_Площадь проемов"
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.ADSK_AreaOfOpenings
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\"" +
                    "отсутствует общий параметр ADSK_Площадь проемов",
                    "Ошибка");
                return Result.Cancelled;
            }

            Dictionary<ElementId, double> _dict_roomId_openingsArea = new Dictionary<ElementId, double>();

            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);

            var user_input = UserInput.GetStringFromUser("Выбор стадии", "Введите стадию для расчета площадей:", _phase);
            if (user_input.Length == 0)
            {
                return Result.Cancelled;
            }
            _phase = user_input;

            View3D view3d
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));


            var filter_rooms = new FilteredElementCollector(doc);
            var filtered_rooms = filter_rooms
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(r => r.get_Parameter(BuiltInParameter.ROOM_PHASE).AsValueString() == _phase)
                .Where(r => (r as Room).Area > 0)
                .ToArray();

            List<RoomDto> roomsDto = filtered_rooms.Select(r => new RoomDto(r as Room)).ToList();
            foreach (RoomDto roomDto in roomsDto)
            {
                if (NotCalculatedRooms.Contains(roomDto))
                    roomDto.DoOpeningsAreaCalculation = false;
            }

            var settings = new OpeningsAreaViewModel(roomsDto);
            //Вывод окна входных данных
            RoomsForCalculation inputForm = new RoomsForCalculation(settings);
            inputForm.ShowDialog();

            if (inputForm.DialogResult != true)
            {
                return Result.Cancelled;
            }

            Element[] rooms = settings.RoomDtos
                .Where(rDto => rDto.DoOpeningsAreaCalculation == true)
                .Select(rDto => rDto.RoomRevit as Element)
                .ToArray();

            NotCalculatedRooms = settings.RoomDtos
                .Where(rDto => rDto.DoOpeningsAreaCalculation == false)
                .ToList();

            var filter_glass_wall = new FilteredElementCollector(doc);
            var glass_walls = filter_glass_wall
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(w => w.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase)
                .Where(w => (w as Wall).CurtainGrid != null)
                .ToList();

            var filter_windows = new FilteredElementCollector(doc);
            var windows = filter_windows
                .OfCategory(BuiltInCategory.OST_Windows)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(w => w.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase)
                .Where(w => (w as FamilyInstance).Host is Wall
                        && ((w as FamilyInstance).Host as Wall).CurtainGrid == null);

            var filter_doors = new FilteredElementCollector(doc);
            var doors = filter_doors
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(d => d.get_Parameter(BuiltInParameter.PHASE_CREATED).AsValueString() == _phase)
                .Where(w => (w as FamilyInstance).Host is Wall
                        && ((w as FamilyInstance).Host as Wall).CurtainGrid == null);

            SolidCurveIntersectionOptions solid_curve_intersect_opt = new SolidCurveIntersectionOptions();

            var openings = doors.Concat(windows);
            var phaseOfRooms = doc.GetElement(rooms.FirstOrDefault().get_Parameter(BuiltInParameter.ROOM_PHASE).AsElementId()) as Phase;

            foreach (var opening in openings)
            {
                var opening_fam_inst = opening as FamilyInstance;
                var opening_area = GeometryMethods.GetOpeningArea(doc, opening_fam_inst);

                var from_room = opening_fam_inst.get_FromRoom(phaseOfRooms);
                var to_room = opening_fam_inst.get_ToRoom(phaseOfRooms);
                if (from_room != null)
                {
                    _dict_roomId_openingsArea.MapIncrease(from_room.Id, opening_area);
                }
                if (to_room != null)
                {
                    _dict_roomId_openingsArea.MapIncrease(to_room.Id, opening_area);
                }
            }

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Площади проемов");

                foreach (Room room in rooms)
                {
                    double room_area = 0;
                    if (_dict_roomId_openingsArea.ContainsKey(room.Id))
                    {
                        room_area = _dict_roomId_openingsArea[room.Id];
                    }

                    List<ElementId> glass_walls_in_rooms_ids = new List<ElementId>();

                    var circuits = room.GetBoundarySegments(_boundaryOptions);
                    foreach (var circuit in circuits)
                    {
                        foreach (var bound in circuit)
                        {
                            var el_bound = doc.GetElement(bound.ElementId);
                            if (el_bound != null)
                            {
                                var el_bound_type_name = el_bound.GetType().Name;
                                if (el_bound_type_name == "ModelLine")
                                {
                                    var curtain_wall_behind_modelline = GeometryMethods
                                        .GetElementByRay_switch(
                                        doc,
                                        view3d,
                                        bound.GetCurve(), true);

                                    var curtain_wall_before_modelline = GeometryMethods
                                        .GetElementByRay_switch(
                                        doc,
                                        view3d,
                                        bound.GetCurve(), false);

                                    if (curtain_wall_behind_modelline == null
                                        && curtain_wall_before_modelline == null)
                                    {
                                        room_area += (room.UnboundedHeight) * (bound.GetCurve().Length);
                                    }
                                }
                            }

                        }
                    }

                    SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room);
                    Solid room_solid = results.GetGeometry();

                    foreach (var glass_wall in glass_walls)
                    {
                        var wall_curve = (glass_wall.Location as LocationCurve).Curve;
                        var curve_intersect = GeometryMethods
                            .CreateNormalCenterCurve(wall_curve, 1, _curtain_wall_intersect_tolerance);


                        SolidCurveIntersection curve_room_intersect = room_solid
                            .IntersectWithCurve(
                            curve_intersect,
                            solid_curve_intersect_opt);

                        if (curve_room_intersect.SegmentCount > 0)
                        {
                            var glass_wall_area = GeometryMethods.GetRectangWallArea(glass_wall as Wall);
                            room_area += glass_wall_area;
                        }
                    }

                    room.get_Parameter(SharedParams.ADSK_AreaOfOpenings).Set(room_area);
                }

                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
