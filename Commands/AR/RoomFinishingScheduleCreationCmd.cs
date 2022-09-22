using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.AR.DTO;
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
    internal class RoomFinishingScheduleCreationCmd : IExternalCommand
    {
        /// <summary>
        /// Заголовок в спецификации
        /// </summary>
        private static string _header = String.Empty;

        /// <summary>
        /// Название спецификации в диспетчере проекта
        /// </summary>
        private static string _scheduleName = String.Empty;

        /// <summary>
        /// Высота заголовка спецификации
        /// </summary>
        private readonly double _heightHeader = 8 / SharedValues.FootToMillimeters / 10;

        /// <summary>
        /// Высота строки для вида отделки
        /// </summary>
        private readonly double _heightRow = 15 / SharedValues.FootToMillimeters / 10;

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
            int one = 0;
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
            for (int i = startRowIndex + 1; i < startRowIndex + rowsCount; i++)
            {
                // Добавить строки по максимальному количеству типов отделки стен или потолка
                table.InsertRow(i);
                table.SetRowHeight(i, _heightRow);
            }
            table.MergeCells(new TableMergedCell(startRowIndex, 0, startRowIndex + rowsCount, 0));
            table.MergeCells(new TableMergedCell(startRowIndex, 1, startRowIndex + rowsCount, 1));
            table.MergeCells(new TableMergedCell(startRowIndex, 6, startRowIndex + rowsCount, 6));
            FillFintypeAreaRows(ref table, fintypeRowDto.CeilingTypeAreas, rowsCount, startRowIndex, colCeilingIndex);
            FillFintypeAreaRows(ref table, fintypeRowDto.WallTypesAreas, rowsCount, startRowIndex, colWalltypeIndex);
            // Добавить строку вниз для последующего типа отделки
            table.InsertRow(startRowIndex + rowsCount);
            table.SetRowHeight(startRowIndex + rowsCount, _heightRow);
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
            if (fintypes.Count < rowsCount)
            {
                // Объединить строки для ячеек с отделкой потолка, если остались пустые
                table.MergeCells(
                    new TableMergedCell(
                        startRowIndex + fintypes.Count,
                        startColIndex,
                        startRowIndex + rowsCount,
                        startColIndex));
                table.MergeCells(
                    new TableMergedCell(
                        startRowIndex + fintypes.Count,
                        startColIndex + 1,
                        startRowIndex + rowsCount,
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
            Document doc,
            string ScheduleTittle,
            string header,
            in List<ScheduleFinishWallsCeilingsRowDto> fintypeRowDtos)
        {
            ViewSchedule schedule = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Parking));
            doc.Regenerate();
            schedule.Name = ScheduleTittle;
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
        private bool ValidateSharedParams(Document doc)
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
        private List<Room> GetRooms(UIDocument uidoc)
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
            }
            return rooms;
        }

        private List<ScheduleFinishWallsCeilingsRowDto> CreateRoomFinishDtos(UIDocument uidoc)
        {
            var rooms = GetRooms(uidoc);
            if (rooms is null) return null;
            return null;//////////////////////////////////////////////////////
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            if (!ValidateSharedParams(doc)) return Result.Cancelled;

            _scheduleName = UserInput.GetStringFromUser("Ведомость отделки", "Введите название спецификации", _scheduleName);
            _header = UserInput.GetStringFromUser("Ведомость отделки", "Введите заголовок", _header);
            View view;
            List<ScheduleFinishWallsCeilingsRowDto> dtos = CreateRoomFinishDtos(uidoc);
            if (dtos is null) return Result.Cancelled;

            using (Transaction finishingScheduleTrans = new Transaction(doc))
            {
                finishingScheduleTrans.Start("Ведомость отделки");
                view = CreateSchedule(doc, _scheduleName, _header, dtos);
                finishingScheduleTrans.Commit();
            }

            uidoc.ActiveView = view;
            return Result.Succeeded;
        }
    }
}
