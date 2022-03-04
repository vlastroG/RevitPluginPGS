using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;



namespace MS.core
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsArea : IExternalCommand
    {
        /// <summary>
        /// Выполнение команды
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            FilteredElementCollector newRoomFilter = new FilteredElementCollector(doc);
            Transaction trans = new Transaction(doc);
            trans.Start("PGS_Flatography");




            // Инициализация коллекции всех помещений в открытом проекте
            ICollection<Element> AllRooms = newRoomFilter.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements();

            // Инициализация списка всех значений параметра помещения АР_ТипПомещения
            List<double> ListOfAllValuesOfParameterTypeOfRoom = new List<double>();

            // Инициализация списка всех значений параметра помещения АР_НомерКвартиры
            List<string> ListOfAllValuesOfParameterNumberOfApartment = new List<string>();



            // Список названий названий помещений для определения их типа
            #region Массивы названий помещений для обработки
            string[] ArrayOfLivingRoomsNames = { "комната", "гостиная", "спальня" };
            string[] ArrayOfUnLivingRoomsNamesType1 = {"кухня", "столовая", "с.у.", "с/у", "ванная с санузом", "санузел", "ванная", "душевая",
             "офисное помещение", "богодельня", "холл", "коридор", "прихожая", "гардероб", "тамбур", "лестница",
             "кухня-столовая"};
            string[] ArrayOfUnLivingRoomsNamesType2 = { "лоджия" };
            string[] ArrayOfUnLivingRoomsNamesType3 = { "балкон", "терраса" };
            string[] ArrayOfUnLivingRoomsNamesType4 = { "вестибюль", "лестничная клетка", "лифтовая шахта", "лифтовый холл" };
            #endregion

            // Обработка значений параметров всех помещений:
            // по значению параметра "Комментарии" и "Имя" назначаются значения параметров "АР_ТипПомещения" и "АР_КоэффПлощади".
            // Также в списки добавляются значения параметров "АР_ТипПомещения" и "АР_НомерКвартиры"
            foreach (var Room in AllRooms)
            #region Обработка значений параметров помещений
            {
                string RoomName = Room.LookupParameter("Имя").AsString().ToLower();
                string RoomApartmentNumber = Room.LookupParameter("АР_НомерКвартиры").AsString();
                string RoomSquare = Room.LookupParameter("Площадь").AsValueString();
                string RoomComment = Room.LookupParameter("Комментарии").AsString();
                double RoomTypeOf = Room.LookupParameter("АР_ТипПомещения").AsDouble();
                double RoomCountOfLivingRooms = Room.LookupParameter("АР_КолвоКомнат").AsDouble();

                ListOfAllValuesOfParameterTypeOfRoom.Add(RoomTypeOf);
                ListOfAllValuesOfParameterNumberOfApartment.Add(RoomApartmentNumber);

                if (RoomComment == "нежилая")
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(2);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
                else if (RoomComment == "общий")
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(5);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
                else if (RoomName == "кухня-гостиная")
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(1);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
                else if (Array.Exists(ArrayOfUnLivingRoomsNamesType1, element => element == RoomName))
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(2);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
                else if (Array.Exists(ArrayOfLivingRoomsNames, element => element == RoomName))
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(1);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
                else if (Array.Exists(ArrayOfUnLivingRoomsNamesType2, element => element == RoomName))
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(3);
                    Room.LookupParameter("АР_КоэффПлощади").Set(0.5);
                }
                else if (Array.Exists(ArrayOfUnLivingRoomsNamesType3, element => element == RoomName))
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(4);
                    Room.LookupParameter("АР_КоэффПлощади").Set(0.3);
                }
                else if (Array.Exists(ArrayOfUnLivingRoomsNamesType4, element => element == RoomName))
                {
                    Room.LookupParameter("АР_ТипПомещения").Set(5);
                    Room.LookupParameter("АР_КоэффПлощади").Set(1);
                }
            }
            #endregion

            // Получение списка уникальных номеров квартир в проекте
            var ListOfUniqueApartmentNumbers = ListOfAllValuesOfParameterNumberOfApartment.Distinct();

            // Инициализация списка количества жилых помещений по квартирам
            List<double> ListOfCountOfLivingRoomsInApartment = new List<double>();
            // Инициализация переменной количества жилых помещений в квартире
            double CountOfLivingRoomsInApartment;

            foreach (var ApartmentNumber in ListOfUniqueApartmentNumbers)
            {
                List<Element> ListOfLivingRoomsInApartment = new List<Element>();
                foreach (var Room in AllRooms)
                {
                    if ((Room.LookupParameter("АР_НомерКвартиры").AsString() == ApartmentNumber) && (Room.LookupParameter("АР_ТипПомещения").AsDouble() == 1))
                    {
                        ListOfLivingRoomsInApartment.Add(Room);
                    }
                }
                CountOfLivingRoomsInApartment = ListOfLivingRoomsInApartment.Count();
                ListOfCountOfLivingRoomsInApartment.Add(CountOfLivingRoomsInApartment);
            }

            // Создание словаря количества жилых комнат на основе номеров квартир
            Dictionary<string, double> DictionaryOfApartmentNumberAndLivingRoomsCount = new Dictionary<string, double>();
            for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
            {
                DictionaryOfApartmentNumberAndLivingRoomsCount.Add(ListOfUniqueApartmentNumbers.ToArray()[i], ListOfCountOfLivingRoomsInApartment.ToArray()[i]);
            }

            // Инициализация списка всех жилых комнат в проекте
            List<Element> ListOfAllLivingRooms = new List<Element>();

            // Назначение параметра количества жилых комнат жилым помещениям
            string CurrentNumberOfApartment;
            foreach (var Room in AllRooms)
            {
                if (Room.LookupParameter("АР_ТипПомещения").AsDouble() == 1)
                {
                    ListOfAllLivingRooms.Add(Room);
                }
                CurrentNumberOfApartment = Room.LookupParameter("АР_НомерКвартиры").AsString();

                Room.LookupParameter("АР_КолвоКомнат").Set(DictionaryOfApartmentNumberAndLivingRoomsCount[CurrentNumberOfApartment]);
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
                    if (Room.LookupParameter("АР_НомерКвартиры").AsString() == Apartment.ToString())
                    {
                        double RoomArea = Room.LookupParameter("Площадь").AsDouble() * (Math.Pow(0.3048, 2));
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
                string ApartmentNumber = Room.LookupParameter("АР_НомерКвартиры").AsString();
                double AreaFromDictionaryNumberAndArea = DictionaryOfApartmentNumberAndLivingArea[ApartmentNumber] / (Math.Pow(0.3048, 2));
                Room.LookupParameter("АР_ПлощКвЖилая").Set(AreaFromDictionaryNumberAndArea);
            }

            // Список жилых и нежилых помещений
            List<Element> ListOfAllLivingAndUnlivingRooms = new List<Element>();
            foreach (var Room in AllRooms)
            {
                if (Room.LookupParameter("АР_ТипПомещения").AsDouble() < 3)
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
                    if (Room.LookupParameter("АР_НомерКвартиры").AsString() == Apartment.ToString())
                    {
                        double RoomUnlivAndLivArea = Room.LookupParameter("Площадь").AsDouble() * (Math.Pow(0.3048, 2));
                        OneApartmentUnlivingAndLivingArea = OneApartmentUnlivingAndLivingArea + RoomUnlivAndLivArea;
                    }
                }
                ListOfLivingAndUnlivingAreaOfRooms.Add(OneApartmentUnlivingAndLivingArea);
            }

            // Словарь номеров квартир -> площади жилых и нежилых помещений
            Dictionary<string, double> DictionaryOfApartmentNumberAndLivingAndUnlivingArea = new Dictionary<string, double>();
            for (int i = 0; i < ListOfUniqueApartmentNumbers.Count(); i++)
            {
                DictionaryOfApartmentNumberAndLivingAndUnlivingArea.Add(ListOfUniqueApartmentNumbers.ToArray()[i], ListOfLivingAndUnlivingAreaOfRooms.ToArray()[i]);
            }

            // Назначение общай (жилая+нежилая) площади помещениям в квартирах
            foreach (var Room in ListOfAllLivingAndUnlivingRooms)
            {
                string ApartmentNumber = Room.LookupParameter("АР_НомерКвартиры").AsString();
                double AreaFromDictionaryOfNumberAndLivUnlivArea = DictionaryOfApartmentNumberAndLivingAndUnlivingArea[ApartmentNumber] / (Math.Pow(0.3048, 2));
                Room.LookupParameter("АР_ПлощКвартиры").Set(AreaFromDictionaryOfNumberAndLivUnlivArea);
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
                    if ((Room.LookupParameter("АР_НомерКвартиры").AsString() == Apartment.ToString()) && (Room.LookupParameter("АР_ТипПомещения").AsDouble() == 3))
                    {
                        double RoomLodjArea = (Room.LookupParameter("Площадь").AsDouble() * Math.Pow(0.3048, 2)) * Room.LookupParameter("АР_КоэффПлощади").AsDouble();
                        OneApartmentLodjArea = OneApartmentLodjArea + RoomLodjArea;
                    }
                    else if ((Room.LookupParameter("АР_НомерКвартиры").AsString() == Apartment.ToString()) && (Room.LookupParameter("АР_ТипПомещения").AsDouble() == 4))
                    {
                        double RoomBalkonArea = (Room.LookupParameter("Площадь").AsDouble() * Math.Pow(0.3048, 2)) * Room.LookupParameter("АР_КоэффПлощади").AsDouble();
                        OneApartmentBalkonArea = OneApartmentBalkonArea + RoomBalkonArea;
                    }
                    else if ((Room.LookupParameter("АР_НомерКвартиры").AsString() == Apartment.ToString()) && (Room.LookupParameter("АР_ТипПомещения").AsDouble() == 5))
                    {
                        double RoomTotalArea = (Room.LookupParameter("Площадь").AsDouble() * Math.Pow(0.3048, 2)) * Room.LookupParameter("АР_КоэффПлощади").AsDouble();
                        OneApartmentTotalFiveArea = OneApartmentTotalFiveArea + RoomTotalArea;
                    }
                    OneApartmentColdTotalArea = OneApartmentLodjArea + OneApartmentBalkonArea + OneApartmentTotalFiveArea;
                }
                ListOfAreasOfUnlivColdRooms.Add(OneApartmentColdTotalArea);
            }

            // Список итоговых общих площадей квартир (всей блять, вообще всей, которая у квартиры есть балконы ебучие, спальни, толчки... и т. д., всей, ебать!!)
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
                    string ApartmentNumberLast = Room.LookupParameter("АР_НомерКвартиры").AsString();
                    double TotalAreaAll = DictionaryOfApartmentNumberAndTotalArea[ApartmentNumberLast] / (Math.Pow(0.3048, 2));
                    Room.LookupParameter("АР_ПлощКвОбщая").Set(TotalAreaAll);
                }
            }

            trans.Commit();

            return Result.Succeeded;

        }
    }
}
