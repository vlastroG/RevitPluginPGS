using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using MS.Commands.AR.DTO;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Вычисляет ключ для словаря типа отделки и списка помещений с этой отделкой
        /// </summary>
        /// <param name="room">Помещение</param>
        /// <param name="finType">Значение параметра типа отделки</param>
        /// <param name="byLevel">Если True, то ключ по уровню и типу отделки, иначе только по типу отделки.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Элемент не является помещением.</exception>
        private string GetKeyForFinishing(in Element room, in string finType, in bool byLevel)
        {
            if (!(room is Room))
            {
                throw new ArgumentException($"Элемент с {room.Id} не является помещением!");
            }
            if (byLevel)
            {
                Document doc = room.Document;
                string levelName = doc.GetElement(room.LevelId).Name;
                return finType + levelName;
            }
            else
            {
                return finType;
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

            var multiNameRange = UserInput.YesNoCancelInput(
                "Помещения с одинаковой отделкой", "Если считать отделку поэтажно - \"Да\", " +
                "если сквозной подсчет - \"Нет\"");
            if (multiNameRange != System.Windows.Forms.DialogResult.Yes
                && multiNameRange != System.Windows.Forms.DialogResult.No)
            {
                return Result.Cancelled;
            }
            bool byLevel;
            if (multiNameRange == System.Windows.Forms.DialogResult.Yes)
            {
                byLevel = true;
            }
            else
            {
                byLevel = false;
            }

            var rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(e => e.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble() > 0)
                .ToList();

            // Словарь пар значений PGS_ТипОтделкиСтен и списка названий помещений с их номерами
            Dictionary<string, MultiNameRoomsDto> dictFinWallsMultiName = new Dictionary<string, MultiNameRoomsDto>();
            // Словарь пар значений PGS_ТипОтделкиПола и списка названий помещений с их номерами
            Dictionary<string, MultiNameRoomsDto> dictFinFloorMultiName = new Dictionary<string, MultiNameRoomsDto>();

            List<Element> roomsWithFinishing = new List<Element>();
            int roomsWithWallFinishing = 0;
            int roomsWithFloorFinishing = 0;
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
                        string keyTypeFinWall = GetKeyForFinishing(room, typeFinWall, byLevel);
                        AddOrUpdateMultiNamesDict(
                            dictFinWallsMultiName,
                            keyTypeFinWall,
                            roomName,
                            roomNumber);
                        roomsWithFinishing.Add(room);
                        roomsWithWallFinishing++;
                    }
                    if (!String.IsNullOrEmpty(typeFinFloor))
                    {
                        string keyTypeFinFloor = GetKeyForFinishing(room, typeFinFloor, byLevel);
                        AddOrUpdateMultiNamesDict(
                            dictFinFloorMultiName,
                            keyTypeFinFloor,
                            roomName,
                            roomNumber);
                        roomsWithFinishing.Add(room);
                        roomsWithFloorFinishing++;
                    }
                }
            }

            int equalFinWallSetCount = 0;
            int equalFinFloorSetCount = 0;
            using (Transaction transEqualFinishing = new Transaction(doc))
            {
                transEqualFinishing.Start("Одинаковая отделка помещений");
                foreach (Element room in roomsWithFinishing)
                {
                    string typeFinWall = room.get_Parameter(SharedParams.PGS_FinishingTypeOfWalls).AsValueString();
                    string typeFinFloor = room.get_Parameter(SharedParams.PGS_FinishingTypeOfFloor).AsValueString();
                    string roomNumber = room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsValueString();
                    string roomName = room.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString();

                    if (!String.IsNullOrEmpty(typeFinWall))
                    {
                        string keyTypeFinWall = GetKeyForFinishing(room, typeFinWall, byLevel);
                        string dictValue = dictFinWallsMultiName[keyTypeFinWall].ToString();
                        string existValue = room.get_Parameter(SharedParams.PGS_MultiTextMark).AsValueString();
                        try
                        {
                            if (existValue != dictValue)
                            {
                                room.get_Parameter(SharedParams.PGS_MultiTextMark).Set(dictValue);
                                equalFinWallSetCount++;
                            }
                        }
                        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                        {
                            MessageBox.Show("Параметр PGS_МногострочнаяМарка доступен только для чтения," +
                                " проверьте, что он не используется в ключевой спецификации.", "Ошибка!");
                            return Result.Failed;
                        }
                    }
                    if (!String.IsNullOrEmpty(typeFinFloor))
                    {
                        string keyTypeFinFloor = GetKeyForFinishing(room, typeFinFloor, byLevel);
                        string dictValue = dictFinFloorMultiName[keyTypeFinFloor].ToString();
                        string existValue = room.get_Parameter(SharedParams.PGS_MultiTextMark_2).AsValueString();
                        try
                        {
                            if (existValue != dictValue)
                            {
                                room.get_Parameter(SharedParams.PGS_MultiTextMark_2).Set(dictValue);
                                equalFinFloorSetCount++;
                            }
                        }
                        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                        {
                            MessageBox.Show("Параметр PGS_МногострочнаяМарка_2 доступен только для чтения," +
                                " проверьте, что он не используется в ключевой спецификации.", "Ошибка!");
                            return Result.Failed;
                        }
                    }
                }
                transEqualFinishing.Commit();
            }

            MessageBox.Show($"Обработано {roomsWithWallFinishing} помещений с отделкой стен, " +
                $"{roomsWithFloorFinishing} помещений с отделкой пола" +
                $" из всех {rooms.Count} помещений в проеке с ненулевой площадью." +
                $"\nPGS_МногострочнаяМарка для одинаковой отделки стен и потолка обновлена {equalFinWallSetCount} раз," +
                $"\nPGS_МногострочнаяМарка_2 для одинаковой отделки пола обновлена {equalFinFloorSetCount} раз.",
                "Одинаковая отделка помещений.");

            return Result.Succeeded;
        }
    }
}
