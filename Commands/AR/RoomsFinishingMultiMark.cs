using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

    /// <summary>
    /// Маркировка помещений с одинаковой отделкой
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishingMultiMark : IExternalCommand
    {
        private bool ValidateParams(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Guid[] _sharedParamsForCommand = new Guid[]
                {
                SharedParams.PGS_FinishingTypeOfWalls,
                SharedParams.PGS_FinishingTypeOfFloor,
                SharedParams.PGS_MultiTextMark,
                SharedParams.PGS_MultiTextMark_2,
                };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\" " +
                    "присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nPGS_ТипОтделкиСтен," +
                    "\nPGS_ТипОтделкиПола," +
                    "\nPGS_МногострочнаяМарка," +
                    "\nPGS_МногострочнаяМарка_2",
                    "Ошибка");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Обновить словарь многострочных названий помещений с номерами
        /// </summary>
        /// <param name="dictMultiNames">Словарь типа отделки и многострочного представления
        /// списка названий помещений с номерами</param>
        /// <param name="key">Значение типа отделки</param>
        /// <param name="roomName">Название помещения</param>
        /// <param name="roomNumber">Номер комнаты</param>
        private void AddOrUpdateMultiNamesDict(
            Dictionary<string, MultiNameRoomsDto> dictMultiNames,
            string key,
            string roomName,
            string roomNumber)
        {
            if (dictMultiNames.ContainsKey(key))
            {
                dictMultiNames[key].AddNameWithNumber(roomName, roomNumber);
            }
            else
            {
                dictMultiNames.Add(key,
                    new MultiNameRoomsDto(
                        new NameWithNumbersDto(roomName, roomNumber)));
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (!ValidateParams(commandData))
            {
                return Result.Cancelled;
            }

            string finWallsScheduleName = UserInput.GetStringFromUser(
                "Ключевые спецификации отделки",
                "Введите название ключевой спецификации для отделки помещений (стены+потолок):",
                "В_Отделка-помещения-01_Стили помещений_Ключевая"
                );
            if (finWallsScheduleName.Length == 0)
            {
                return Result.Cancelled;
            }
            string finFloorScheduleName = UserInput.GetStringFromUser(
                "Ключевые спецификации отделки",
                "Введите название ключевой спецификации для отделки помещений (пол):",
                "В_Полы-помещения-01_Стили полов_Ключевая"
                );
            if (finFloorScheduleName.Length == 0)
            {
                return Result.Cancelled;
            }

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            // Словарь пар значений PGS_ТипОтделкиСтен и списка названий помещений с их номерами
            Dictionary<string, MultiNameRoomsDto> dictFinWallsMultiName = new Dictionary<string, MultiNameRoomsDto>();
            // Словарь пар значений PGS_ТипОтделкиПола и списка названий помещений с их номерами
            Dictionary<string, MultiNameRoomsDto> dictFinFloorMultiName = new Dictionary<string, MultiNameRoomsDto>();

            foreach (Element room in rooms)
            {
                string typeFinWall = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                string typeFinFloor = room.get_Parameter(SharedParams.PGS_FinishingTypeOfFloor).AsValueString();
                string roomNumber = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
                string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString();
                if ((!String.IsNullOrEmpty(roomName)) && (!String.IsNullOrEmpty(roomNumber)))
                {
                    if (!String.IsNullOrEmpty(typeFinWall))
                    {
                        AddOrUpdateMultiNamesDict(
                            dictFinWallsMultiName,
                            typeFinWall,
                            roomName,
                            roomNumber);
                    }
                    if (!String.IsNullOrEmpty(typeFinFloor))
                    {
                        AddOrUpdateMultiNamesDict(
                            dictFinFloorMultiName,
                            typeFinFloor,
                            roomName,
                            roomNumber);
                    }
                }
            }

            Element finWallsSchedule = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Schedules)
                .FirstOrDefault(s => s.Name == finWallsScheduleName);

            if (finWallsSchedule != null)
            {
                ElementId scheduleViewId = finWallsSchedule.Id;
                ElementOwnerViewFilter filter = new ElementOwnerViewFilter(scheduleViewId);
                var elems = finWallsSchedule.GetDependentElements(filter).Select(id => doc.GetElement(id)).ToArray();
                foreach (var row in elems)
                {
                    try
                    {
                        string key = row.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                        string multiText = row.get_Parameter(SharedParams.PGS_MultiTextMark).AsValueString();
                        string multiTextDict = dictFinWallsMultiName[key].ToString();
                        if (multiText != multiTextDict)
                        {
                            row.get_Parameter(SharedParams.PGS_MultiTextMark).Set(dictFinWallsMultiName[key].ToString());
                        }
                    }
                    catch (ArgumentNullException)
                    {
                        MessageBox.Show($"В спецификации {finWallsScheduleName} присутствуют не все необходимые поля:" +
                            $"\nPGS_ТипОтделкиСтен" +
                            $"\nPGS_", "");
                        return Result.Failed;
                    }
                }
            }


            Element finFloorSchedule = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Schedules)
                .FirstOrDefault(s => s.Name == finFloorScheduleName);



            return Result.Succeeded;
        }
    }
}
