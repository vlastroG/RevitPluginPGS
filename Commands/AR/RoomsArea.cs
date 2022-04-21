using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static MS.Utilites.UserInput;
using MS.GUI.AR;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsArea : IExternalCommand
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

            InputRoomsArea inputForm = new InputRoomsArea();
            inputForm.ShowDialog();
            if (inputForm.DialogResult == false)
            {
                return Result.Cancelled;
            }


            var projTemp = inputForm.RevitTemplate.ToLower();
            //var projTemp = GetStringFromUser(
            //    "Указание текущего шаблона",
            //    "Проект выполнен в шаблоне PGS или ADSK?\nВведите \'ADSK' или \'PGS\'.",
            //    "PGS");

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


            var all_project_rooms = inputForm.AllProjCalc;
            //var dialogResult = YesNoCancelInput(
            //    "Выбор диапазона расчета",
            //    "Расчитывать площади помещений во всем проекте?");

            //if (dialogResult == DialogResult.Yes)
            //{
            //    all_project_rooms = true;
            //}
            //else if (dialogResult == DialogResult.No)
            //{
            //    all_project_rooms = false;
            //}
            //else if (dialogResult == DialogResult.Cancel)
            //{
            //    return Result.Cancelled;
            //}

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

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("PGS_Flatography");


                // Инициализация коллекции всех помещений в открытом проекте
                ICollection<Element> Rooms = newActiveViewFilterElements
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements();

                string __apartment_number;
                string __default = "value_not_found";

                List<Element> __rooms_list = new List<Element>();
                foreach (var room in Rooms)
                {
                    try
                    {
                        __apartment_number = room.LookupParameter(paramRoomApartmentNumber).AsValueString();
                    }
                    catch (Exception)
                    {
                        __apartment_number = __default;
                    }
                    //Исключение помещений с изначально не заданным параметром номера квартиры.
                    //Если номер был задан, но потом его стерли, то такие помещения сгруппируются в одну квартиру и тоже посчитаются.
                    //На расчет нужных помещений с заполненными параметрами это не повлияет.
                    if (!ReferenceEquals(__apartment_number, null))
                    {
                        __rooms_list.Add(room);
                    }
                }

                Element[] AllRooms = __rooms_list.ToArray();

                // Инициализация списка всех значений параметра помещения АР_ТипПомещения
                List<double> ListOfAllValuesOfParameterTypeOfRoom = new List<double>();

                // Инициализация списка всех значений параметра помещения АР_НомерКвартиры
                List<string> ListOfAllValuesOfParameterNumberOfApartment = new List<string>();


                // Обработка значений параметров всех помещений:
                // по значению параметра paramRoomComment и paramRoomName назначаются значения параметров paramRoomType и paramRoomAreaCoeff.
                // Также в списки добавляются значения параметров paramRoomType и paramRoomApartmentNumber
                foreach (var Room in AllRooms)
                {
                    string RoomName = Room.LookupParameter(paramRoomName).AsString().ToLower();
                    string RoomApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    string RoomSquare = Room.LookupParameter(paramRoomSquare).AsValueString();
                    string RoomComment = Room.LookupParameter(paramRoomComment).AsString();
                    int RoomTypeOf = Room.LookupParameter(paramRoomType).AsInteger();
                    int RoomCountOfLivingRooms = Room.LookupParameter(paramRoomCountOfLivingRooms).AsInteger();

                    ListOfAllValuesOfParameterTypeOfRoom.Add(RoomTypeOf);
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
                // Инициализация переменной количества жилых помещений в квартире
                int CountOfLivingRoomsInApartment;

                foreach (var ApartmentNumber in ListOfUniqueApartmentNumbers)
                {
                    List<Element> ListOfLivingRoomsInApartment = new List<Element>();
                    foreach (var Room in AllRooms)
                    {
                        if ((Room.LookupParameter(paramRoomApartmentNumber).AsString() == ApartmentNumber)
                            && (Room.LookupParameter(paramRoomType).AsInteger() == 1))
                        {
                            ListOfLivingRoomsInApartment.Add(Room);
                        }
                    }
                    CountOfLivingRoomsInApartment = ListOfLivingRoomsInApartment.Count();
                    ListOfCountOfLivingRoomsInApartment.Add(CountOfLivingRoomsInApartment);
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

                // Инициализация списка всех жилых комнат в проекте
                List<Element> ListOfAllLivingRooms = new List<Element>();

                // Назначение параметра количества жилых комнат жилым помещениям
                string CurrentNumberOfApartment;
                foreach (var Room in AllRooms)
                {
                    if (Room.LookupParameter(paramRoomType).AsInteger() == 1)
                    {
                        ListOfAllLivingRooms.Add(Room);
                    }
                    CurrentNumberOfApartment = Room.LookupParameter(paramRoomApartmentNumber).AsString();

                    Room.LookupParameter(paramRoomCountOfLivingRooms).Set(DictionaryOfApartmentNumberAndLivingRoomsCount[CurrentNumberOfApartment]);
                }

                // получение жилой площади квартир ///////////////////////////////////////////////////////////////////
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
                            double RoomArea = Room.LookupParameter(paramRoomSquare).AsDouble() * (Math.Pow(0.3048, 2));
                            OneApartmentArea = OneApartmentArea + RoomArea;
                        }
                    }
                    ListOfLivingAreasOfApartments.Add(OneApartmentArea);
                }

                // Создание словаря номер квартиры -> жилая площадь квартиры
                Dictionary<string, double> DictionaryOfApartmentNumberAndLivingArea = new Dictionary<string, double>();
                for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
                {
                    DictionaryOfApartmentNumberAndLivingArea.Add(ListOfUniqueApartmentNumbers.ToArray()[i], ListOfLivingAreasOfApartments.ToArray()[i]);
                }

                // Назначение жилой площади помещениям в квартирах
                foreach (var Room in ListOfAllLivingRooms)
                {
                    string ApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    double AreaFromDictionaryNumberAndArea = DictionaryOfApartmentNumberAndLivingArea[ApartmentNumber] / (Math.Pow(0.3048, 2));
                    Room.LookupParameter(paramApartmLive).Set(AreaFromDictionaryNumberAndArea);
                }

                // Список жилых и нежилых помещений
                List<Element> ListOfAllLivingAndUnlivingRooms = new List<Element>();
                foreach (var Room in AllRooms)
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
                            double RoomUnlivAndLivArea = Room.LookupParameter(paramRoomSquare).AsDouble() * (Math.Pow(0.3048, 2));
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

                // Назначение общай (жилая+нежилая) площади помещениям в квартирах
                foreach (var Room in ListOfAllLivingAndUnlivingRooms)
                {
                    string ApartmentNumber = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                    double AreaFromDictionaryOfNumberAndLivUnlivArea =
                        DictionaryOfApartmentNumberAndLivingAndUnlivingArea[ApartmentNumber] / (Math.Pow(0.3048, 2));
                    Room.LookupParameter(paramApartmArea).Set(AreaFromDictionaryOfNumberAndLivUnlivArea);
                }

                // Список площадей из типов помещений 3, 4, 5 
                List<double> ListOfAreasOfUnlivColdRooms = new List<double>();
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    double OneApartmentBalkonArea = 0;
                    double OneApartmentLodjArea = 0;
                    double OneApartmentTotalFiveArea = 0;
                    double OneApartmentColdTotalArea = 0;
                    foreach (var Room in AllRooms)
                    {
                        if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 3))
                        {
                            double RoomLodjArea = (Room.LookupParameter(paramRoomSquare).AsDouble()
                                * Math.Pow(0.3048, 2))
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble();
                            OneApartmentLodjArea = OneApartmentLodjArea + RoomLodjArea;
                        }
                        else if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 4))
                        {
                            double RoomBalkonArea = (Room.LookupParameter(paramRoomSquare).AsDouble()
                                * Math.Pow(0.3048, 2))
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble();
                            OneApartmentBalkonArea = OneApartmentBalkonArea + RoomBalkonArea;
                        }
                        else if ((Room.LookupParameter(paramRoomApartmentNumber).AsString()
                            == Apartment.ToString())
                            && (Room.LookupParameter(paramRoomType).AsInteger()
                            == 5))
                        {
                            double RoomTotalArea = (Room.LookupParameter(paramRoomSquare).AsDouble()
                                * Math.Pow(0.3048, 2))
                                * Room.LookupParameter(paramRoomAreaCoeff).AsDouble();
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

                // Назначение общей площади (отапливаемые жилые и нежилые помещения и неотапливаемые помещения) помещениям в квартирах
                foreach (var Apartment in ListOfUniqueApartmentNumbers)
                {
                    foreach (var Room in AllRooms)
                    {
                        string ApartmentNumberLast = Room.LookupParameter(paramRoomApartmentNumber).AsString();
                        double TotalAreaAll = DictionaryOfApartmentNumberAndTotalArea[ApartmentNumberLast] / (Math.Pow(0.3048, 2));
                        Room.LookupParameter(paramApartmAreaAll).Set(TotalAreaAll);
                    }
                }

                trans.Commit();
            }
            return Result.Succeeded;

        }
    }
}
