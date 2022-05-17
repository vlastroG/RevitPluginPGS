using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.Models;
using MS.GUI.AR;
using System;
using System.Collections.Generic;
using System.Linq;

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

            List<Apartment> _apartments = new List<Apartment>();

            //string paramRoomName = "Имя";//не зависит от шаблона. Зависит только от языка интерфейса.
            //string paramRoomSquare = "Площадь"; //не зависит от шаблона. Зависит только от языка интерфейса.
            //string paramRoomComment = "Комментарии"; //не зависит от шаблона. Зависит только от языка интерфейса.
            string paramRoomCountOfLivingRooms = "АР_КолвоКомнат";
            string paramRoomApartmentNumber = "АР_НомерКвартиры";
            string paramRoomType = "АР_ТипПомещения";
            string paramRoomAreaCoeff = "АР_КоэффПлощади";
            string paramApartmAreaAll = "АР_ПлощКвОбщая";
            string paramApartmArea = "АР_ПлощКвартиры";
            string paramApartmLive = "АР_ПлощКвЖилая";

            //Вывод окна входных данных
            InputRoomsArea inputForm = new InputRoomsArea();
            inputForm.ShowDialog();
            if (inputForm.DialogResult == false)
            {
                return Result.Cancelled;
            }

            //Выбор шаблона, на котором сделан проект
            var projTemp = inputForm.RevitTemplate.ToLower();

            //Назначение названий параметров для квартирографии в соответствии с выбранным шаблоном
            switch (projTemp)
            {
                case "adsk":
                    paramRoomApartmentNumber = "ADSK_Номер квартиры";
                    paramRoomType = "ADSK_Тип помещения";
                    paramRoomCountOfLivingRooms = "ADSK_Количество комнат";
                    paramRoomAreaCoeff = "ADSK_Коэффициент площади";
                    paramApartmArea = "ADSK_Площадь квартиры";
                    paramApartmLive = "ADSK_Площадь квартиры жилая";
                    paramApartmAreaAll = "ADSK_Площадь квартиры общая";
                    break;
                case "":
                    return Result.Cancelled;
                default:
                    break;
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
                        __apartment_number = Rooms[i].LookupParameter(paramRoomApartmentNumber).AsValueString();
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
                //                          параметров paramRoomType и paramRoomApartmentNumber соответственно.
                string RoomName;
                string RoomComment;
                string RoomApartmentNumber;
                foreach (var Room in Rooms)
                {
                    RoomName = Room.get_Parameter(BuiltInParameter.ROOM_NAME).AsString().ToLower();
                    RoomComment = Room.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                    RoomApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();

                    if (RoomComment == "нежилая")
                    {
                        Room.LookupParameter(paramRoomType).Set(2);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (RoomComment == "общий")
                    {
                        Room.LookupParameter(paramRoomType).Set(5);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (RoomName == "кухня-гостиная")
                    {
                        Room.LookupParameter(paramRoomType).Set(1);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType1, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(2);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (Array.Exists(ArrayOfLivingRoomsNames, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(1);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType2, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(3);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(0.5);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType3, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(4);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(0.3);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType4, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(5);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }
                    else if (Array.Exists(ArrayOfUnLivingRoomsNamesType5, element => element == RoomName))
                    {
                        Room.LookupParameter(paramRoomType).Set(4);
                        Room.LookupParameter(paramRoomAreaCoeff).Set(1);
                    }

                    //Создание квартир и добавление в них помещений
                    if (_apartments.Where(a => a.Number == RoomApartmentNumber).Count() == 0)
                    {
                        var apartment = new Apartment(RoomApartmentNumber);
                        apartment.AddRoom(Room as Autodesk.Revit.DB.Architecture.Room, paramRoomApartmentNumber);
                        _apartments.Add(apartment);
                    }
                    else
                    {
                        var apartment = _apartments.Find(a => a.Number == RoomApartmentNumber);
                        apartment.AddRoom(Room as Autodesk.Revit.DB.Architecture.Room, paramRoomApartmentNumber);
                    }
                }
                /*----------------------------------Назначение "типа комнаты" и "коэффициента площади", создание квартир (окончание)------------*/

                // Назначение значений площадей (жилая, отапливаемая, приведенная) и количества комнат жилым помещениям в квартирах
                foreach (var apartment in _apartments)
                {
                    var area_live = apartment.GetAreaLiving(paramRoomType, round_decimals);
                    var area_heated = apartment.GetAreaHeated(paramRoomType, round_decimals);
                    var area_total = apartment.GetAreaTotalCoeff(paramRoomType, paramRoomAreaCoeff, round_decimals);
                    var room_liv_count = apartment.GetLivingRooms(paramRoomType).Count;
                    foreach (var room in apartment.Rooms)
                    {
                        room.LookupParameter(paramApartmLive).Set(area_live);
                        room.LookupParameter(paramApartmArea).Set(area_heated);
                        room.LookupParameter(paramApartmAreaAll).Set(area_total);
                        room.LookupParameter(paramRoomCountOfLivingRooms).Set(room_liv_count);
                    }
                }

                trans.Commit();
            }



            return Result.Succeeded;

        }
    }
}
