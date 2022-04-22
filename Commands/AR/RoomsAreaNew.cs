using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MS.Utilites.UserInput;
using MS.GUI.AR;

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

        private readonly double footSquare = 0.3048 * 0.3048;


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

            string paramRoomName = "Имя";//не зависит от шаблона. Зависит только от языка интерфейса.
            string paramRoomSquare = "Площадь"; //не зависит от шаблона. Зависит только от языка интерфейса.
            string paramRoomComment = "Комментарии"; //не зависит от шаблона. Зависит только от языка интерфейса.
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
                trans.Start("PGS_Flatography");

                // Инициализация коллекции всех помещений в открытом проекте
                List<Element> Rooms = newActiveViewFilterElements
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements().ToList();

                //Значение параметра номера квартиры
                string __apartment_number;
                //Значение параметра номера квартиры для назначения по дефолту в случае ошибки
                string __default = "value_not_found";

                // Очистка списка помещений от незаполненного параметра
                for (var i = 0; i < Rooms.Count; i++)
                {
                    try
                    {
                        __apartment_number = Rooms[i].LookupParameter(paramRoomApartmentNumber).AsValueString();
                    }
                    catch (Exception)
                    {
                        __apartment_number = __default;
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

                // Инициализация списка всех значений параметра помещения АР_ТипПомещения
                List<double> ListOfAllValuesOfParameterTypeOfRoom = new List<double>();

                // Инициализация списка всех значений параметра помещения АР_НомерКвартиры
                List<string> ListOfAllValuesOfParameterNumberOfApartment = new List<string>();

                // Обработка значений параметров всех помещений:
                // по значению параметра paramRoomComment и paramRoomName назначаются значения параметров типа помещения и коэффициента площади.
                // Также в списки добавляются значения RoomTypeOf    и RoomApartmentNumber
                //                          параметров paramRoomType и paramRoomApartmentNumber соответственно.
                int RoomTypeOf;
                string RoomName;
                string RoomComment;
                string RoomApartmentNumber;
                foreach (var Room in Rooms)
                {
                    RoomName = Room.LookupParameter(paramRoomName).AsString().ToLower();
                    RoomComment = Room.LookupParameter(paramRoomComment).AsString();

                    RoomTypeOf = Room.LookupParameter(paramRoomType).AsInteger();
                    ListOfAllValuesOfParameterTypeOfRoom.Add(RoomTypeOf);

                    RoomApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    ListOfAllValuesOfParameterNumberOfApartment.Add(RoomApartmentNumber);

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
                }

                // Получение списка уникальных номеров квартир в проекте
                var ListOfUniqueApartmentNumbers = ListOfAllValuesOfParameterNumberOfApartment.Distinct();

                // Инициализация списка количества жилых помещений по квартирам
                List<int> ListOfCountOfLivingRoomsInApartment = new List<int>();

                /*-------------------------Создание словаря номеров квартир и количества жилых помещений в них (начало)------------------------*/
                int CountOfLivingRoomsInApartment = 0;
                //Заполнение списка количества жилых помещений по квартирам
                foreach (var ApartmentNumber in ListOfUniqueApartmentNumbers)
                {
                    foreach (var Room in Rooms)
                    {
                        if ((Room.LookupParameter(paramRoomApartmentNumber).AsString() == ApartmentNumber)
                            && (Room.LookupParameter(paramRoomType).AsInteger() == 1))
                        {
                            CountOfLivingRoomsInApartment++;
                        }
                    }
                    ListOfCountOfLivingRoomsInApartment.Add(CountOfLivingRoomsInApartment);
                    CountOfLivingRoomsInApartment = 0;
                }

                // Создание словаря количества жилых комнат на основе номеров квартир
                Dictionary<string, int> DictionaryOfApartmentNumberAndLivingRoomsCount = new Dictionary<string, int>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    try
                    {
                        DictionaryOfApartmentNumberAndLivingRoomsCount.Add(
                            ListOfUniqueApartmentNumbers.ToArray()[i],
                            ListOfCountOfLivingRoomsInApartment.ToArray()[i]);
                    }
                    catch (ArgumentNullException)
                    {
                        throw;
                    }
                }
                /*------------------------Создание словаря номеров квартир и количества жилых помещений в них (окончание)----------------------*/

                // Инициализация списка всех жилых комнат в проекте
                List<Element> ListOfAllLivingRooms = new List<Element>();

                // Назначение параметра количества жилых комнат жилым помещениям
                string CurrentNumberOfApartment;
                foreach (var Room in Rooms)
                {
                    if (Room.LookupParameter(paramRoomType).AsInteger() == 1)
                    {
                        ListOfAllLivingRooms.Add(Room);
                    }
                    CurrentNumberOfApartment = Room.LookupParameter(paramRoomApartmentNumber).AsString();

                    Room.LookupParameter(paramRoomCountOfLivingRooms).Set(DictionaryOfApartmentNumberAndLivingRoomsCount[CurrentNumberOfApartment]);
                }

                // Получение жилой площади квартир 
                // Инициализация списка жилой площади квартир
                List<Double> ListOfLivingAreasOfApartments = new List<double>();

                // Расчет жилых площадей квартир
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    double OneApartmentArea = 0;
                    foreach (var Room in ListOfAllLivingRooms)
                    {
                        if (Room.LookupParameter(paramRoomApartmentNumber).AsString() == Apartment.ToString())
                        {
                            double RoomArea = Math.Round(Math.Round(Room.LookupParameter(paramRoomSquare).AsDouble() * footSquare, round_decimals), round_decimals);
                            OneApartmentArea = OneApartmentArea + RoomArea;
                        }
                    }
                    ListOfLivingAreasOfApartments.Add(OneApartmentArea);
                }

                // Создание словаря номер квартиры -> жилая площадь квартиры
                Dictionary<string, double> DictionaryOfApartmentNumberAndLivingArea = new Dictionary<string, double>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    DictionaryOfApartmentNumberAndLivingArea
                        .Add(
                        ListOfUniqueApartmentNumbers.ToArray()[i],
                        ListOfLivingAreasOfApartments.ToArray()[i]);
                }

                /*-----------------------------------------------------------------------------------------------------------------------*/
                // Назначение жилой площади помещениям в квартирах
                foreach (var Room in ListOfAllLivingRooms)
                {
                    string ApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    double AreaFromDictionaryNumberAndArea =
                                                DictionaryOfApartmentNumberAndLivingArea[ApartmentNumber] / footSquare;

                    Room.LookupParameter(paramApartmLive).Set(AreaFromDictionaryNumberAndArea);
                }
                /*-----------------------------------------------------------------------------------------------------------------------*/


                // Список жилых и нежилых помещений
                List<Element> ListOfAllLivingAndUnlivingRooms = new List<Element>();
                foreach (var Room in Rooms)
                {
                    if (Room.LookupParameter(paramRoomType).AsInteger() < 3)
                    {
                        ListOfAllLivingAndUnlivingRooms.Add(Room);
                    }
                }

                // Список общих площадей квартир
                List<double> ListOfLivingAndUnlivingAreaOfRooms = new List<double>();
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    double OneApartmentUnlivingAndLivingArea = 0;
                    foreach (var Room in ListOfAllLivingAndUnlivingRooms)
                    {
                        if (Room.LookupParameter(paramRoomApartmentNumber).AsString() == Apartment.ToString())
                        {
                            double RoomUnlivAndLivArea = Math.Round(Math.Round(Room.LookupParameter(paramRoomSquare).AsDouble() * footSquare, round_decimals), round_decimals);
                            OneApartmentUnlivingAndLivingArea = OneApartmentUnlivingAndLivingArea + RoomUnlivAndLivArea;
                        }
                    }
                    ListOfLivingAndUnlivingAreaOfRooms.Add(OneApartmentUnlivingAndLivingArea);
                }

                // Словарь номеров квартир -> площади жилых и нежилых помещений
                Dictionary<string, double> DictionaryOfApartmentNumberAndLivingAndUnlivingArea = new Dictionary<string, double>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    DictionaryOfApartmentNumberAndLivingAndUnlivingArea.Add(
                        ListOfUniqueApartmentNumbers.ToArray()[i],
                        ListOfLivingAndUnlivingAreaOfRooms.ToArray()[i]);
                }

                /*-----------------------------------------------------------------------------------------------------------------------*/
                // Назначение общай (жилая+нежилая) площади помещениям в квартирах
                foreach (var Room in ListOfAllLivingAndUnlivingRooms)
                {
                    string ApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    double AreaFromDictionaryOfNumberAndLivUnlivArea =
                        DictionaryOfApartmentNumberAndLivingAndUnlivingArea[ApartmentNumber] / footSquare;

                    Room.LookupParameter(paramApartmArea).Set(AreaFromDictionaryOfNumberAndLivUnlivArea);
                }
                /*-----------------------------------------------------------------------------------------------------------------------*/


                // Список площадей из типов помещений 3, 4, 5 
                List<double> ListOfAreasOfUnlivColdRooms = new List<double>();
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    double OneApartmentBalkonArea = 0;
                    double OneApartmentLodjArea = 0;
                    double OneApartmentTotalFiveArea = 0;
                    double OneApartmentColdTotalArea = 0;
                    foreach (var Room in Rooms)
                    {
                        if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 3))
                        {
                            double RoomLodjArea = Math.Round(
                                Math.Round(
                                Room.LookupParameter(paramRoomSquare).AsDouble()
                                * footSquare,
                                round_decimals)
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble(),
                                round_decimals);
                            OneApartmentLodjArea = OneApartmentLodjArea + RoomLodjArea;
                        }
                        else if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 4))
                        {
                            double RoomBalkonArea = Math.Round(
                                Math.Round(
                                Room.LookupParameter(paramRoomSquare).AsDouble()
                                * footSquare,
                                round_decimals)
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble(),
                                round_decimals);
                            OneApartmentBalkonArea = OneApartmentBalkonArea + RoomBalkonArea;
                        }
                        else if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 5))
                        {
                            double RoomTotalArea = Math.Round(
                                Math.Round(
                                Room.LookupParameter(paramRoomSquare).AsDouble()
                                * footSquare,
                                round_decimals)
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble(),
                                round_decimals);
                            OneApartmentTotalFiveArea = OneApartmentTotalFiveArea + RoomTotalArea;
                        }
                        OneApartmentColdTotalArea = OneApartmentLodjArea + OneApartmentBalkonArea + OneApartmentTotalFiveArea;
                    }
                    ListOfAreasOfUnlivColdRooms.Add(OneApartmentColdTotalArea);
                }

                // Список ИТОГОВЫХ общих площадей квартир 
                List<double> ListOfTotalAreas = new List<double>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    ListOfTotalAreas.Add(ListOfAreasOfUnlivColdRooms.ToArray()[i] + ListOfLivingAndUnlivingAreaOfRooms.ToArray()[i]);
                }

                // Словарь номеров квартир и площадей жилых и нежилых помещений вместе с неотапливаемыми
                Dictionary<string, double> DictionaryOfApartmentNumberAndTotalArea = new Dictionary<string, double>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    DictionaryOfApartmentNumberAndTotalArea.Add(ListOfUniqueApartmentNumbers.ToArray()[i], ListOfTotalAreas.ToArray()[i]);
                }

                /*-----------------------------------------------------------------------------------------------------------------------*/
                // Назначение общей площади (отапливаемые жилые и нежилые помещения и неотапливаемые помещения) помещениям в квартирах
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    foreach (var Room in Rooms)
                    {
                        string ApartmentNumberLast = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                        double TotalAreaAll = DictionaryOfApartmentNumberAndTotalArea[ApartmentNumberLast] / footSquare;

                        Room.LookupParameter(paramApartmAreaAll).Set(TotalAreaAll);
                    }
                }
                /*-----------------------------------------------------------------------------------------------------------------------*/


                trans.Commit();
            }
            return Result.Succeeded;

        }
    }
}
