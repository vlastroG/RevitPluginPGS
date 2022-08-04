using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.Models;
using MS.GUI.AR;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsAreaNew : IExternalCommand
    {
        /// <summary>
        /// Список названий помещений с типом комнаты 1 и коэффициентом площади 1
        /// </summary>
        private readonly string[] ArrayOfLivingRoomsNames = {
                "гостиная",
                "детская",
                "жилая комната",
                "комната",
                "помещение",
                "спальня"};

        /// <summary>
        /// Список названий помещений с типом комнаты 2 и коэффициентом площади 1
        /// </summary>
        private readonly string[] ArrayOfUnLivingRoomsNamesType1 = {
                "бельевая",
                "богодельня",
                "ванная",
                "ванная с санузом",
                "встроенный шкаф",
                "гардероб",
                "гардеробная",
                "душевая",
                "коридор",
                "кухня",
                "кухня-ниша",
                "кухня-столовая",
                "кухня-гостиная",
                "лестница",
                "лестница внутриквартирная",
                "офисное помещение",
                "постирочная",
                "прихожая",
                "санузел",
                "совмещенный санузел",
                "столовая",
                "с.у.",
                "с/у",
                "туалет",
                "холл",
                "отапливаемая кладовая",
                "тамбур"};

        /// <summary>
        /// Список названий помещений с типом комнаты 3 и коэффициентом площади 0.5
        /// </summary>
        private readonly string[] ArrayOfUnLivingRoomsNamesType2 = {
                "лоджия" };

        /// <summary>
        /// Список названий помещений с типом комнаты 4 и коэффициентом площади 0.3
        /// </summary>
        private readonly string[] ArrayOfUnLivingRoomsNamesType3 = {
                "балкон",
                "терраса" };

        /// <summary>
        /// Список названий помещений с типом комнаты 5 и коэффициентом площади 1
        /// </summary>
        private readonly string[] ArrayOfUnLivingRoomsNamesType4 = {
                "вестибюль",
                "лестничная клетка",
                "лифтовая шахта",
                "лифтовый холл" };

        /// <summary>
        /// Список названий помещений с типом комнаты 4 и коэффициентом площади 1
        /// </summary>
        private readonly string[] ArrayOfUnLivingRoomsNamesType5 = {
                "холодная кладовая",
                "веранда"};


        /// <summary>
        /// Проверяет, являестя строка null,
        /// и возвращает либо пустую строку, либо исходную (если она не null)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string MakeStringValid(ref string value)
        {
            if (!ReferenceEquals(value, null))
            {
                return value;
            }
            else
            {
                value = String.Empty;
                return value;
            }
        }

        /// <summary>
        /// Расчитывает площади помещений и квартир с учетом коэффициентов, и принадлежности к жилой/нежилой зоне. 
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            /*---------------------------------------Ввод входных данных (начало)--------------------------------------------------------------*/
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Guid[] _sharedParamsForCommand = new Guid[] {
                SharedParams.ADSK_NumberOfApartment,
                SharedParams.ADSK_TypeOfRoom,
                SharedParams.ADSK_CountOfRooms,
                SharedParams.ADSK_CoeffOfArea,
                SharedParams.ADSK_AreaOfApartment,
                SharedParams.ADSK_AreaOfApartmentLiving,
                SharedParams.ADSK_AreaOfApartmentTotal
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Rooms,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Помещения\" " +
                    "присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nADSK_Номер квартиры" +
                    "\nADSK_Тип помещения" +
                    "\nADSK_Количество комнат" +
                    "\nADSK_Коэффициент площади" +
                    "\nADSK_Площадь квартиры" +
                    "\nADSK_Площадь квартиры жилая" +
                    "\nADSK_Площадь квартиры общая",
                    "Ошибка");
                return Result.Cancelled;
            }

            List<Apartment> _apartments = new List<Apartment>();

            //Вывод окна входных данных
            InputRoomsArea inputForm = new InputRoomsArea();
            inputForm.ShowDialog();
            if (inputForm.DialogResult == false)
            {
                return Result.Cancelled;
            }

            // Выбор диапазона подсчета площадей.
            // Подсчет площадей помещений во всем проекте, если true, и только видимые на виде, если false
            var all_project_rooms = inputForm.AllProjCalc;

            //Назначение числа знаков после запятой для округления значения площадей помещений
            var round_decimals = inputForm.AreaRound;

            // Создание фильтра в соответствии с заданным диапазоном - весь проект/активный вид
            FilteredElementCollector newActiveViewFilterElements;
            if (all_project_rooms)
            {
                newActiveViewFilterElements = new FilteredElementCollector(doc);
            }
            else
            {
                // Фильтр для помещений в активном виде
                newActiveViewFilterElements = new FilteredElementCollector(doc, doc.ActiveView.Id);
            }
            /*---------------------------------------Ввод входных данных (окончание)-----------------------------------------------------------*/

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("ПГС квартирография");

                /*---------------------------------------------Получение списка помещений для обработки (начало)-----------------------------*/
                // Инициализация коллекции всех помещений в открытом проекте
                List<Element> Rooms = newActiveViewFilterElements
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements().ToList();

                //Значение параметра номера квартиры
                string __apartment_number;

                // Очистка списка помещений от незаполненного параметра
                for (var i = 0; i < Rooms.Count; i++)
                {
                    try
                    {
                        __apartment_number = Rooms[i].get_Parameter(SharedParams.ADSK_NumberOfApartment).AsValueString();
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException(Rooms[i].get_Parameter(BuiltInParameter.ID_PARAM).ToString());
                    }
                    //Исключение помещений (замена на null) с изначально не заданным параметром номера квартиры.
                    //Если номер был задан, но потом его стерли, то такие помещения также заменяются на null.
                    if (String.IsNullOrEmpty(__apartment_number))
                    {
                        Rooms[i] = null;
                    }
                }
                //Очистка списка помещений от null
                Rooms.RemoveAll(r => r == null);

                /*---------------------------------------------Получение списка помещений для обработки (окончание)-----------------------------*/

                /*-------------------------------Назначение "типа комнаты" и "коэффициента площади", создание квартир (начало)---------------------*/
                // Обработка значений параметров всех помещений:
                // по значению параметра Комментарии и Имя комнаты назначаются значения параметров "тип комнаты" и "коэффициента площади".
                // Также в списки добавляются значения RoomTypeOf    и RoomApartmentNumber
                //                          параметров SharedParams.ADSK_TypeOfRoom и paramRoomApartmentNumber соответственно.
                string RoomName;
                string RoomComment;
                string RoomApartmentNumber;
                foreach (var Room in Rooms)
                {
                    RoomName = Room.get_Parameter(BuiltInParameter.ROOM_NAME).AsValueString();
                    RoomName = MakeStringValid(ref RoomName).ToLower();
                    RoomComment = Room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsValueString();
                    RoomComment = MakeStringValid(ref RoomComment).ToLower();
                    RoomApartmentNumber = Room.get_Parameter(SharedParams.ADSK_NumberOfApartment).AsValueString();
                    RoomApartmentNumber = MakeStringValid(ref RoomApartmentNumber);

                    if (RoomComment.Contains("нежилая"))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(2);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (RoomComment.Contains("общий"))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(5);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (RoomName.Contains("кухня-гостиная"))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(1);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType1, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(2);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (Array.Exists(ArrayOfLivingRoomsNames, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(1);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType2, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(3);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(0.5);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType3, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(4);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(0.3);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType4, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(5);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType5, element => element == RoomName))
                    {
                        Room.get_Parameter(SharedParams.ADSK_TypeOfRoom).Set(4);
                        Room.get_Parameter(SharedParams.ADSK_CoeffOfArea).Set(1);
                    }

                    //Создание квартир и добавление в них помещений
                    if (_apartments.Where(a => a.Number == RoomApartmentNumber).Count() == 0)
                    {
                        var apartment = new Apartment(RoomApartmentNumber);
                        apartment.AddRoom(Room as Autodesk.Revit.DB.Architecture.Room);
                        _apartments.Add(apartment);
                    }
                    else
                    {
                        var apartment = _apartments.Find(a => a.Number == RoomApartmentNumber);
                        apartment.AddRoom(Room as Autodesk.Revit.DB.Architecture.Room);
                    }
                }
                /*----------------------------------Назначение "типа комнаты" и "коэффициента площади", создание квартир (окончание)------------*/

                // Назначение значений площадей (жилая, отапливаемая, приведенная) и количества комнат жилым помещениям в квартирах
                foreach (var apartment in _apartments)
                {
                    var area_live = apartment.GetAreaLiving(round_decimals);
                    var area_heated = apartment.GetAreaHeated(round_decimals);
                    var area_total = apartment.GetAreaTotalCoeff(round_decimals);
                    var room_liv_count = apartment.GetLivingRooms().Count;
                    foreach (var room in apartment.Rooms)
                    {
                        room.get_Parameter(SharedParams.ADSK_AreaOfApartmentLiving).Set(area_live);
                        room.get_Parameter(SharedParams.ADSK_AreaOfApartment).Set(area_heated);
                        room.get_Parameter(SharedParams.ADSK_AreaOfApartmentTotal).Set(area_total);
                        room.get_Parameter(SharedParams.ADSK_CountOfRooms).Set(room_liv_count);
                    }
                }

                trans.Commit();
            }



            return Result.Succeeded;

        }
    }
}
