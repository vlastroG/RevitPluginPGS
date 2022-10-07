using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.AR.DTO;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Extensions;
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
    internal class RoomFinishingScheduleCreationCmd : IExternalCommand
    {
        /// <summary>
        /// Заголовок в спецификации
        /// </summary>
        private static string _header = String.Empty;

        /// <summary>
        /// Строка, которая должна содержаться в названии типа отделочноых стен
        /// </summary>
        private readonly string _finWalls = "_F_";

        /// <summary>
        /// Название спецификации в диспетчере проекта
        /// </summary>
        private static string _scheduleName = String.Empty;

        /// <summary>
        /// Высота заголовка спецификации
        /// </summary>
        private readonly double _heightHeader = 8 / SharedValues.FootToMillimeters;

        /// <summary>
        /// Высота строки для вида отделки
        /// </summary>
        private readonly double _heightRow = 15 / SharedValues.FootToMillimeters;

        /// <summary>
        /// Список ширин столбцов ведомости отделки
        /// </summary>
        private readonly List<double> _widths = new List<double>()
        {
            50 / SharedValues.FootToMillimeters,
            20 / SharedValues.FootToMillimeters,
            40 / SharedValues.FootToMillimeters,
            15 / SharedValues.FootToMillimeters,
            40 / SharedValues.FootToMillimeters,
            15 / SharedValues.FootToMillimeters,
            30 / SharedValues.FootToMillimeters
        };

        /// <summary>
        /// Добавить заголовок к спецификации ведомости отделки и создать заготовку для 1 вида отделки для 1 типа отделки
        /// </summary>
        /// <param name="table">Таблица ведомости отделки</param>
        /// <param name="header">Заголовок спецификации</param>
        private void WriteHeader(ref TableSectionData table, string header)
        {
            int zero = 0;
            int one = 1;
            table.ClearCell(zero, zero);
            table.SetCellText(zero, zero, header);
            table.SetRowHeight(zero, _heightHeader);
            table.InsertRow(one);
            table.SetRowHeight(one, _heightRow);
            for (int i = 0; i < _widths.Count; i++)
            {
                if (i < (_widths.Count - 1))
                {
                    table.InsertColumn(i + 1);
                }
                table.SetColumnWidth(i, _widths[i]);
            }
            table.MergeCells(new TableMergedCell(zero, zero, zero, _widths.Count - 1));
            table.RefreshData();
        }

        /// <summary>
        /// Заполняет созданную спецификацию
        /// </summary>
        /// <param name="table">Спецификация ведомости отделки</param>
        /// <param name="header">Заголовок спецификации</param>
        /// <param name="fintypeRowDtos">Список DTO для видов отделки стен и потолка</param>
        private void FillTable(
            ref TableSectionData table,
            string header,
            in List<ScheduleFinishWallsCeilingsRowDto> fintypeRowDtos)
        {
            WriteHeader(ref table, header);
            foreach (var dto in fintypeRowDtos)
            {
                AddFintypeRow(ref table, dto);
            }
        }

        /// <summary>
        /// Добавляет в спецификацию тип отделки путем добавления каждого вида отделки как отдельной строки 
        /// и объединяя пустые оставшиеся строки. 
        /// Добавление начинается с заполнения последней строки строки спецификации
        /// и заканчивается добавлением новой заготовки строки в низ спецификации для нового типа отделки.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fintypeRowDto"></param>
        private void AddFintypeRow(ref TableSectionData table, in ScheduleFinishWallsCeilingsRowDto fintypeRowDto)
        {
            int startRowIndex = table.LastRowNumber;
            int colCeilingIndex = 2;
            int colWalltypeIndex = 4;
            int walltypesCount = fintypeRowDto.WallTypesAreas.Count;
            int ceilingtypesCount = fintypeRowDto.CeilingTypeAreas.Count;
            table.SetCellText(startRowIndex, 0, fintypeRowDto.RoomNames);
            table.SetCellText(startRowIndex, 1, fintypeRowDto.FintypeWallsCeilings);
            int rowsCount =
                walltypesCount >= ceilingtypesCount ? walltypesCount : ceilingtypesCount;
            rowsCount = rowsCount > 0 ? rowsCount : 1;
            for (int i = startRowIndex + 1; i < startRowIndex + rowsCount; i++)
            {
                // Добавить строки по максимальному количеству типов отделки стен или потолка
                table.InsertRow(i);
                table.RefreshData();
                table.SetRowHeight(i, _heightRow);
            }
            if (rowsCount > 1)
            {
                table.MergeCells(new TableMergedCell(startRowIndex, 0, startRowIndex + rowsCount - 1, 0));
                table.MergeCells(new TableMergedCell(startRowIndex, 1, startRowIndex + rowsCount - 1, 1));
                table.MergeCells(new TableMergedCell(startRowIndex, 6, startRowIndex + rowsCount - 1, 6));
                FillFintypeAreaRows(ref table, fintypeRowDto.CeilingTypeAreas, rowsCount, startRowIndex, colCeilingIndex);
                FillFintypeAreaRows(ref table, fintypeRowDto.WallTypesAreas, rowsCount, startRowIndex, colWalltypeIndex);
            }
            // Добавить строку вниз для последующего типа отделки
            table.InsertRow(startRowIndex + rowsCount);
            table.SetRowHeight(startRowIndex + rowsCount, _heightRow);
            table.RefreshData();
        }

        /// <summary>
        /// Заполняет строчки и колонки для вида отделки (стены или потолок)
        /// </summary>
        /// <param name="table">Таблица спецификации ведомости отделки</param>
        /// <param name="fintypes">Список кортежей наименований типов отделки и их площадей</param>
        /// <param name="rowsCount">Количество строк для максимального количества типов отделки 
        /// по видам отделки (стены/потолок) для данного типа отделки помещений</param>
        /// <param name="startRowIndex">Индекс первой строки, куда писать наименование вида отделки</param>
        /// <param name="startColIndex">Индекс первой колонки, куда писать значение площади отделки</param>
        private void FillFintypeAreaRows(
            ref TableSectionData table,
            in IReadOnlyList<(string FinType, double Area)> fintypes,
            int rowsCount,
            int startRowIndex,
            int startColIndex)
        {
            for (int i = startRowIndex, j = 0; i < startRowIndex + fintypes.Count; i++, j++)
            {
                // заполнить ячейки для типов отделки потолка
                table.SetCellText(i, startColIndex, fintypes[j].FinType);
                table.SetCellText(i, startColIndex + 1, fintypes[j].Area.ToString());
            }
            int fintypesCount = fintypes.Count > 1 ? fintypes.Count : 1;
            if (fintypesCount < rowsCount)
            {
                // Объединить строки для ячеек с отделкой потолка, если остались пустые
                table.MergeCells(
                    new TableMergedCell(
                        startRowIndex + fintypesCount - 1,
                        startColIndex,
                        startRowIndex + rowsCount - 1,
                        startColIndex));
                table.MergeCells(
                    new TableMergedCell(
                        startRowIndex + fintypesCount - 1,
                        startColIndex + 1,
                        startRowIndex + rowsCount - 1,
                        startColIndex + 1));
            }
        }

        /// <summary>
        /// Создает фиктивную спецификацию для ведомости отделки
        /// </summary>
        /// <param name="doc">Документ, в котором происходит транзакция</param>
        /// <param name="ScheduleTittle">Название спецификации</param>
        /// <param name="header">Заголовок спецификации</param>
        /// <param name="fintypeRowDtos">Список DTO для типов отделки помещений</param>
        /// <returns>Созданная и заполненная спецификация</returns>
        private ViewSchedule CreateSchedule(
            in Document doc,
            string ScheduleTittle,
            string header,
            in List<ScheduleFinishWallsCeilingsRowDto> fintypeRowDtos)
        {
            ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Parking));
            doc.Regenerate();
            try
            {
                schedule.Name = ScheduleTittle;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                throw new ArgumentException("Спецификация с заданным названием уже существует, введите другое имя.");
            }
            SchedulableField schedulableField = schedule.Definition.GetSchedulableFields().First();
            if (schedulableField != null)
            {
                schedule.Definition.AddField(schedulableField);
            }
            schedule.GetTableData().Width = _widths.Sum();
            TableSectionData headerTable = schedule.GetTableData().GetSectionData(SectionType.Header);
            FillTable(ref headerTable, header, fintypeRowDtos);
            return schedule;
        }

        /// <summary>
        /// Валидация проекта Revit на наличие необходимых общих параметров
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <returns>True, если все общие параметры присутствуют, иначе false</returns>
        private bool ValidateSharedParams(in Document doc)
        {
            Guid[] _sharedParamsForRooms = new Guid[] {
            SharedParams.PGS_FinishingTypeOfWalls,
            SharedParams.PGS_MultiTextMark
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForRooms))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\"" +
                    "присутствуют не все необходимые общие параметры" +
                    "\nPGS_ТипОтделкиСтен" +
                    "\nPGS_МногострочнаяМарка",
                    "Ошибка");
                return false;
            }
            Guid[] _sharedParamsForWallsAnsCeiling = new Guid[] {
            SharedParams.ADSK_RoomNumberInApartment,
            SharedParams.PGS_FinishingTypeOfWalls
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Walls,
                _sharedParamsForWallsAnsCeiling))
            {
                MessageBox.Show("В текущем проекте у категории \"Стены\"" +
                    "отсутствуют необходимые общие параметры:" +
                    "\nADSK_Номер помещения квартиры" +
                    "\nPGS_ТипОтделкиСтен",
                    "Ошибка");
                return false;
            }
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Ceilings,
                _sharedParamsForWallsAnsCeiling))
            {
                MessageBox.Show("В текущем проекте у категории \"Потолки\"" +
                    "отсутствуют необходимые общие параметры:" +
                    "\nADSK_Номер помещения квартиры" +
                    "\nPGS_ТипОтделкиСтен",
                    "Ошибка");
                return false;
            }
            return true;
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
                    .Where(r => !String.IsNullOrEmpty(r.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString()))
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
                        .Where(r => !String.IsNullOrEmpty(r.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString()))
                        .ToList();
                    if (rooms.Count == 0) return null;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return null;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    MessageBox.Show("Перейдите на вид, где можно выбирать помещения, " +
                        "или выделите их в спецификации и нажмите команду", "Ошибка");
                    return null;
                }
            }
            return rooms;
        }

        /// <summary>
        /// Создает список кортежей типов отделки стен и их площадей для коматы.
        /// Также значениф параметров помещения 'ADSK_Номер помещения квартиры' и 'PGS_ТипОтделкиСтен'
        /// заносятся в соответствующие параметры отделочных стен стен, 
        /// имеющих в названии типа <see cref="_finWalls">Строку</see>.
        /// </summary>
        /// <param name="doc">Документ, в котором происходит создание ведомости отделки</param>
        /// <param name="room">Помещение, для которого составляется список</param>
        /// <param name="view3d">3D вид по умолчанию</param>
        /// <param name="opts">Опции для обработки геометрии</param>
        /// <returns>Список кортежей типов отделки стен и их площадей для данной комнаты</returns>
        private List<(string WallType, double Area)> GetRoomWalltypesAreas(
            in Document doc,
            in Room room,
            in View3D view3d,
            in SpatialElementBoundaryOptions opts)
        {
            IList<IList<BoundarySegment>> loops
                      = room.GetBoundarySegments(
                        new SpatialElementBoundaryOptions());
            List<(string WallType, double Area)> tupleList = new List<(string WallType, double Area)>();
            List<int> idsInt = new List<int>();
            string rNum = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
            string rFintype = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
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
                    if (!(e as Wall).Name.Contains(_finWalls)) continue;
                    Wall wall = (Wall)e;
                    int wallId = wall.Id.IntegerValue;
                    if (!idsInt.Contains(wallId))
                    {
                        idsInt.Add(wallId);
                        tupleList.AddOrUpdate(
                            (wall.WallType.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION).AsValueString(),
                            Math.Round(wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble() * SharedValues.SqFeetToMeters,
                            3)));
                        // Назначить номер помещения и тип отделки стене
                        e.get_Parameter(SharedParams.ADSK_RoomNumberInApartment).Set(rNum);
                        e.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).Set(rFintype);
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return tupleList;
        }

        /// <summary>
        /// Возвращает список кортежей типов отделки потолков и их площадей для помещения
        /// </summary>
        /// <param name="doc">Документ, в котором происходит создание ведомости отделки</param>
        /// <param name="room">Помещение, для которого составляется список кортежей отделки с площадью</param>
        /// <param name="ceilingPointsAndIds">Список кортежей точек потолков и Id потолков во всем проекте.
        /// После нахождения принаджежности потолков из списка помещениям, они удаляются из него.</param>
        /// <returns>Список кортежей типов отделки потолков с площадями</returns>
        private List<(string Ceiling, double Area)> GetRoomCeilingAreas(
            in Document doc,
            in Room room,
            ref List<(XYZ ceilingPoint, ElementId Id)> ceilingPointsAndIds)
        {
            List<(XYZ, ElementId)> ceilingsInRoomToRemove = new List<(XYZ, ElementId)>();
            List<(string Ceiling, double Area)> ceilingsTuple = new List<(string Ceiling, double Area)>();
            foreach (var ceilingPointId in ceilingPointsAndIds)
            {
                if (room.IsPointInRoom(ceilingPointId.ceilingPoint))
                {
                    ceilingsInRoomToRemove.Add(ceilingPointId);
                    Ceiling ceiling = doc.GetElement(ceilingPointId.Id) as Ceiling;
                    string description = doc.GetElement(ceiling.GetTypeId())
                        .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                        .AsValueString();
                    double area =
                        Math.Round(ceiling.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble() * SharedValues.SqFeetToMeters,
                        3);
                    ceilingsTuple.AddOrUpdate((description, area));
                    // Назначить номер помещения и тип отделки стен и потолка потолку 
                    ceiling.get_Parameter(SharedParams.ADSK_RoomNumberInApartment).Set(room.Number);
                    ceiling.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls)
                        .Set(room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString());
                }
            }
            foreach (var ceilintRemove in ceilingsInRoomToRemove)
            {
                ceilingPointsAndIds.Remove(ceilintRemove);
            }
            return ceilingsTuple;
        }

        /// <summary>
        /// Создает список DTO для строчек типов отделки в ведомости отделки
        /// </summary>
        /// <param name="uidoc">Документ, в котором создается ведомость отделки</param>
        /// <returns>Список DTO типов отделки для ведомости отделки</returns>
        private List<ScheduleFinishWallsCeilingsRowDto> CreateRoomFinishDtos(in UIDocument uidoc)
        {
            var rooms = GetRooms(uidoc);
            View3D view3d
              = new FilteredElementCollector(uidoc.Document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));
            Options optsDetail = new Options();
            if (null == view3d)
            {
                MessageBox.Show("Не найден {3D} вид по умолчанию", "Ошибка");
                return null;
            }
            SpatialElementBoundaryOptions opts = new SpatialElementBoundaryOptions();
            if (rooms is null) return null;
            // Изначальное создание списка DTO строчек для типов отделки с заполненными первыми 2 столбцами спецификации:
            // Список помещений с одинаковой отделкой и Наименование типа отделки
            List<ScheduleFinishWallsCeilingsRowDto> dtos =
                rooms.DistinctBy(r => r.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString())
                .Select(r => (r.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString(),
                              r.get_Parameter(SharedParams.PGS_MultiTextMark).AsValueString()))
                .Select(tuple => new ScheduleFinishWallsCeilingsRowDto(tuple.Item1, tuple.Item2))
                .OrderBy(o => o.FintypeWallsCeilings)
                .ToList();
            List<(XYZ ceilingPoint, ElementId Id)> ceilingPointAndId = new FilteredElementCollector(uidoc.Document)
                 .OfCategory(BuiltInCategory.OST_Ceilings)
                 .WhereElementIsNotElementType()
                 .Cast<Ceiling>()
                 .Select(c => (c.GetRoomPoint(), c.Id))
                 .Where(tuple => tuple.Item1 != null)
                 .ToList();
            foreach (Room room in rooms)
            {
                string fintype = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                var dto = dtos.First(d => d.FintypeWallsCeilings == fintype);
                var walltypesAreas = GetRoomWalltypesAreas(uidoc.Document, room, view3d, opts);
                var ceilingsAreas = GetRoomCeilingAreas(uidoc.Document, room, ref ceilingPointAndId);
                foreach (var wtArea in walltypesAreas)
                {
                    dto.AddWallFinType(wtArea);
                }
                foreach (var cArea in ceilingsAreas)
                {
                    dto.AddCeilingFinType(cArea);
                }
            }
            return dtos;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (!ValidateSharedParams(doc)) return Result.Cancelled;

            // Запуск команды для назначения параметра PGS_МногострочнаяМарка помещениям с одинаковой отделкой
            var multimarkRoomsFinCmd = new RoomsFinishingMultiMark();
            Result prevResult = multimarkRoomsFinCmd.Execute(commandData, ref message, elements);
            if (prevResult != Result.Succeeded)
            {
                return Result.Cancelled;
            }

            _scheduleName = UserInput.GetStringFromUser("Ведомость отделки", "Введите название спецификации", _scheduleName);
            if (_scheduleName.Length == 0) return Result.Cancelled;
            _header = UserInput.GetStringFromUser("Ведомость отделки", "Введите заголовок", _header);
            if (_header.Length == 0) return Result.Cancelled;
            // Формирование DTO для строчек с типами отделки для ведомости отделки
            List<ScheduleFinishWallsCeilingsRowDto> dtos;
            using (Transaction wallsCeilingsSetParamsTrans = new Transaction(doc))
            {
                wallsCeilingsSetParamsTrans.Start("Принадлежность стен и потолков помещениям");
                dtos = CreateRoomFinishDtos(uidoc);
                wallsCeilingsSetParamsTrans.Commit();
            }
            // Если в проекте не найдено помещений для обработки, или если команда отменена, или в случае другой ошибки
            if (dtos is null) return Result.Cancelled;

            View view;
            using (Transaction finishingScheduleTrans = new Transaction(doc))
            {
                finishingScheduleTrans.Start("Ведомость отделки");
                // Заполнить таблицу спецификации по dto для типов отделки
                try
                {
                    view = CreateSchedule(doc, _scheduleName, _header, dtos);
                }
                catch (ArgumentException e)
                {
                    MessageBox.Show(e.Message, "Ошибка");
                    return Result.Cancelled;
                }
                finishingScheduleTrans.Commit();
            }
            uidoc.ActiveView = view;

            return Result.Succeeded;
        }
    }
}
