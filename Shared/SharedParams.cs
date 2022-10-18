using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.Shared
{
    public static class SharedParams
    {
        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки = aee96840-3b85-4cb6-a93e-85acee0be8c7 (string)
        /// </summary>
        public static Guid PGS_MarkLintel => Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        /// <summary>
        /// Guid параметра ADSK_МассаЭлемента = 5913a1f9-0b38-4364-96fe-a6f3cb7fcc68 (double)
        /// </summary>
        public static Guid ADSK_MassElement => Guid.Parse("5913a1f9-0b38-4364-96fe-a6f3cb7fcc68");

        /// <summary>
        /// Guid параметра PGS_МассаПеремычки = 77a36313-f239-426c-a6f9-29bf64efee76 (double)
        /// </summary>
        public static Guid PGS_MassLintel => Guid.Parse("77a36313-f239-426c-a6f9-29bf64efee76");

        /// <summary>
        /// Guid параметра PGS_ИзображениеТипоразмераМатериала = 924e3bb2-a048-449f-916f-31093a3aa7a3 (image)
        /// </summary>
        public static Guid PGS_ImageTypeMaterial => Guid.Parse("924e3bb2-a048-449f-916f-31093a3aa7a3");

        /// <summary>
        /// Guid параметра Орг.ТипВключатьВСпецификацию = 45ef1720-9cfe-49a7-b4d7-c67e4f7bd191 (bool)
        /// </summary>
        public static Guid Org_TypeIncludeInSchedule => Guid.Parse("45ef1720-9cfe-49a7-b4d7-c67e4f7bd191");

        /// <summary>
        /// Guid параметра "ADSK_Номер квартиры" = 10fb72de-237e-4b9c-915b-8849b8907695 (string)
        /// </summary>
        public static Guid ADSK_NumberOfApartment => Guid.Parse("10fb72de-237e-4b9c-915b-8849b8907695");

        /// <summary>
        /// Guid параметра "ADSK_Тип помещения" = 56eb1705-f327-4774-b212-ef9ad2c860b0 (int)
        /// </summary>
        public static Guid ADSK_TypeOfRoom => Guid.Parse("56eb1705-f327-4774-b212-ef9ad2c860b0");

        /// <summary>
        /// Guid параметра "ADSK_Количество комнат" = f52108e1-0813-4ad6-8376-a38a1a23a55b (int)
        /// </summary>
        public static Guid ADSK_CountOfRooms => Guid.Parse("f52108e1-0813-4ad6-8376-a38a1a23a55b");

        /// <summary>
        /// Guid параметра "ADSK_Коэффициент площади" = 066eab6d-c348-4093-b0ca-1dfe7e78cb6e (double)
        /// </summary>
        public static Guid ADSK_CoeffOfArea => Guid.Parse("066eab6d-c348-4093-b0ca-1dfe7e78cb6e");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры" = d3035d0f-b738-4407-a0e5-30787b92fa49 (double)
        /// </summary>
        public static Guid ADSK_AreaOfApartment => Guid.Parse("d3035d0f-b738-4407-a0e5-30787b92fa49");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры жилая" = 178e222b-903b-48f5-8bfc-b624cd67d13c (double)
        /// </summary>
        public static Guid ADSK_AreaOfApartmentLiving => Guid.Parse("178e222b-903b-48f5-8bfc-b624cd67d13c");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры общая" = af973552-3d15-48e3-aad8-121fe0dda34e (double)
        /// </summary>
        public static Guid ADSK_AreaOfApartmentTotal => Guid.Parse("af973552-3d15-48e3-aad8-121fe0dda34e");

        /// <summary>
        /// Guid параметра "ADSK_Индекс квартиры" = a2985e5c-b28e-416a-acf6-7ab7e4ee6d86 (string)
        /// </summary>
        public static Guid ADSK_IndexOfApartment => Guid.Parse("a2985e5c-b28e-416a-acf6-7ab7e4ee6d86");

        /// <summary>
        /// Guid параметра "ADSK_Тип квартиры" = 78e3b89c-eb68-4600-84a7-c523de162743 (string)
        /// </summary>
        public static Guid ADSK_TypeOfApartment => Guid.Parse("78e3b89c-eb68-4600-84a7-c523de162743");

        /// <summary>
        /// Guid параметра "ADSK_Площадь проемов" = 18e3f49d-1315-415f-8359-8f045a7a8938 (double)
        /// </summary>
        public static Guid ADSK_AreaOfOpenings => Guid.Parse("18e3f49d-1315-415f-8359-8f045a7a8938");

        /// <summary>
        /// Guid параметра Мрк.МаркаКонструкции = 5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405 (string)
        /// </summary>
        public static Guid Mrk_MarkOfConstruction => Guid.Parse("5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405");

        /// <summary>
        /// Guid параметра Мрк.МаркаСетки = dd157586-3a06-41a8-9564-391c410bae63 (string)
        /// </summary>
        public static Guid Mrk_MeshMark => Guid.Parse("dd157586-3a06-41a8-9564-391c410bae63");

        /// <summary>
        /// Guid параметра ADSK_Марка = 2204049c-d557-4dfc-8d70-13f19715e46d (string)
        /// </summary>
        public static Guid ADSK_Mark => Guid.Parse("2204049c-d557-4dfc-8d70-13f19715e46d");

        /// <summary>
        /// Guid параметра PGS_АрмКолвоАрмРядов = 9d149e47-87a0-45f7-9237-d47436065c80 (double)
        /// </summary>
        public static Guid PGS_ArmCountRows => Guid.Parse("9d149e47-87a0-45f7-9237-d47436065c80");

        /// <summary>
        /// Guid параметра Рзм.Ширина = 8f2e4f93-9472-4941-a65d-0ac468fd6a5d (double)
        /// </summary>
        public static Guid ADSK_DimensionWidth => Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");

        /// <summary>
        /// Guid параметра Рзм.Высота = da753fe3-ecfa-465b-9a2c-02f55d0c2ff1 (double)
        /// </summary>
        public static Guid ADSK_DimensionHeight => Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");

        /// <summary>
        /// Guid параметра ADSK_Размер_Диаметр = 9b679ab7-ea2e-49ce-90ab-0549d5aa36ff (double)
        /// </summary>
        public static Guid ADSK_DimensionDiameter => Guid.Parse("9b679ab7-ea2e-49ce-90ab-0549d5aa36ff");

        /// <summary>
        /// Guid параметра PGS_ИтогАрмСетки = 40f2c9af-2986-4330-a6d9-4c9ae9419342 (double)
        /// </summary>
        public static Guid PGS_TotalMasonryMesh => Guid.Parse("40f2c9af-2986-4330-a6d9-4c9ae9419342");

        /// <summary>
        /// Guid параметра ADSK_Толщина стены = 9350e48f-842b-4c46-a15d-2e36ab1f352f (double)
        /// </summary>
        public static Guid ADSK_ThicknessOfWall => Guid.Parse("9350e48f-842b-4c46-a15d-2e36ab1f352f");

        /// <summary>
        /// Guid параметра ADSK_Наименование = e6e0f5cd-3e26-485b-9342-23882b20eb43 (string)
        /// </summary>
        public static Guid ADSK_Name => Guid.Parse("e6e0f5cd-3e26-485b-9342-23882b20eb43");

        /// <summary>
        /// Guid параметра PGS_МногострочнаяМарка = 5970110c-724d-4b91-bec5-6ff415a2731b (string)
        /// </summary>
        public static Guid PGS_MultiTextMark => Guid.Parse("5970110c-724d-4b91-bec5-6ff415a2731b");

        /// <summary>
        /// Guid параметра PGS_МногострочнаяМарка_2 = f592cad1-9c11-463e-88eb-c8ef7c446666 (string)
        /// </summary>
        public static Guid PGS_MultiTextMark_2 => Guid.Parse("f592cad1-9c11-463e-88eb-c8ef7c446666");

        /// <summary>
        /// Guid параметра PGS_ТипОтделкиСтен = cd68c50c-249b-4cf4-9d90-8671cb7115d5 (string)
        /// </summary>
        public static Guid PGS_FinishingTypeOfWalls => Guid.Parse("cd68c50c-249b-4cf4-9d90-8671cb7115d5");

        /// <summary>
        /// Guid параметра PGS_ТипОтделкиПола = 6f7dd141-1edf-4cd1-82e9-a6834166a84b (string)
        /// </summary>
        public static Guid PGS_FinishingTypeOfFloor => Guid.Parse("6f7dd141-1edf-4cd1-82e9-a6834166a84b");

        /// <summary>
        /// Guid параметра PGS_АрмТип = 93178432-562c-4753-9c86-991aa97dba72 (double)
        /// </summary>
        public static Guid PGS_ArmType => Guid.Parse("93178432-562c-4753-9c86-991aa97dba72");

        /// <summary>
        /// Guid параметра PGS_АрмКолвоСтержней = 03bb8459-28c7-4cdb-8058-cb44563e1c0c (double)
        /// </summary>
        public static Guid PGS_ArmBarsCount => Guid.Parse("03bb8459-28c7-4cdb-8058-cb44563e1c0c");

        /// <summary>
        /// Guid параметра PGS_АрмОтступОтГраней = 54f360ca-8d79-4168-bc13-7ee63f7743e1 (double)
        /// </summary>
        public static Guid PGS_ArmIndentFromFace => Guid.Parse("54f360ca-8d79-4168-bc13-7ee63f7743e1");

        /// <summary>
        /// Guid параметра Мрк.НаименованиеСетки = ca324d54-2a1d-4b38-a91b-09cdaecd01ff (string)
        /// </summary>
        public static Guid Mrk_MeshName => Guid.Parse("ca324d54-2a1d-4b38-a91b-09cdaecd01ff");

        /// <summary>
        /// Guid параметра PGS_АрмШаг = dc0ed051-c8c5-40e8-92d6-5d57937a6590 (double)
        /// </summary>
        public static Guid PGS_ArmStep => Guid.Parse("dc0ed051-c8c5-40e8-92d6-5d57937a6590");

        /// <summary>
        /// Guid параметра PGS_АрмДиаметр = ec329a4f-5899-469e-9335-867c1e38da40
        /// </summary>
        public static Guid PGS_ArmDiameter => Guid.Parse("ec329a4f-5899-469e-9335-867c1e38da40");

        /// <summary>
        /// Guid параметра Арм.КлассСтали = e8e92e84-d88e-415d-8ba0-26a09d8bcccf (string)
        /// </summary>
        public static Guid Arm_SteelClass => Guid.Parse("e8e92e84-d88e-415d-8ba0-26a09d8bcccf");

        /// <summary>
        /// Guid параметра ADSK_Наименование вытяжной системы = f11ed0d9-3b91-4d35-bc44-01a579e45ce7 (string)
        /// </summary>
        public static Guid ADSK_ExhaustSystemName => Guid.Parse("f11ed0d9-3b91-4d35-bc44-01a579e45ce7");

        /// <summary>
        /// Guid параметра ADSK_Наименование приточной системы = 5162f6a4-55c5-43e6-95f4-c06ace52faa0 (string)
        /// </summary>
        public static Guid ADSK_SupplySystemName => Guid.Parse("5162f6a4-55c5-43e6-95f4-c06ace52faa0");

        /// <summary>
        /// Guid параметра Орг.ДиапазонПозиций = 75d41d2e-d5cd-45f4-bd3d-ff28b6a69547 (string)
        /// </summary>
        public static Guid Org_PositionRange => Guid.Parse("75d41d2e-d5cd-45f4-bd3d-ff28b6a69547");

        /// <summary>
        /// Guid параметра PGS_НаименованиеОтделки = f2c3dea3-2edf-4e34-87df-059fd78135f6 (string)
        /// </summary>
        public static Guid PGS_FinishingName => Guid.Parse("f2c3dea3-2edf-4e34-87df-059fd78135f6");

        /// <summary>
        /// Guid параметра ADSK_Код изделия = 2fd9e8cb-84f3-4297-b8b8-75f444e124ed (string)
        /// </summary>
        public static Guid ADSK_ItemCode => Guid.Parse("2fd9e8cb-84f3-4297-b8b8-75f444e124ed");

        /// <summary>
        /// Guid параметра ADSK_Позиция = ae8ff999-1f22-4ed7-ad33-61503d85f0f4 (string)
        /// </summary>
        public static Guid ADSK_Position => Guid.Parse("ae8ff999-1f22-4ed7-ad33-61503d85f0f4");

        /// <summary>
        /// Guid параметра ADSK_Обозначение = 9c98831b-9450-412d-b072-7d69b39f4029 (string)
        /// </summary>
        public static Guid ADSK_Designation => Guid.Parse("9c98831b-9450-412d-b072-7d69b39f4029");

        /// <summary>
        /// Guid параметр ADSK_Группирование = 3de5f1a4-d560-4fa8-a74f-25d250fb3401 (string)
        /// </summary>
        public static Guid ADSK_Grouping => Guid.Parse("3de5f1a4-d560-4fa8-a74f-25d250fb3401");

        /// <summary>
        /// Guid параметра ADSK_Примечание = a85b7661-26b0-412f-979c-66af80b4b2c3 (string)
        /// </summary>
        public static Guid ADSK_Note => Guid.Parse("a85b7661-26b0-412f-979c-66af80b4b2c3");

        /// <summary>
        /// Guid параметра PSG_Откосы_Глубина = 5842657b-ccd1-4a1d-9745-45ac29c2cc12 (double)
        /// </summary>
        public static Guid PGS_SlopeDepth => Guid.Parse("5842657b-ccd1-4a1d-9745-45ac29c2cc12");

        /// <summary>
        /// Guid параметра PGS_Откосы_Площадь = d54eb20d-f66e-43ac-95a9-0908be9aeac6 (area)
        /// </summary>
        public static Guid PGS_SlopesArea => Guid.Parse("d54eb20d-f66e-43ac-95a9-0908be9aeac6");

        /// <summary>
        /// Guid параметра PGS_Длина_Плинтус = ad4d6b89-6138-467e-b472-80e103e004b9 (double)
        /// </summary>
        public static Guid PGS_PlinthLength => Guid.Parse("ad4d6b89-6138-467e-b472-80e103e004b9");

        /// <summary>
        /// Guid параметра ADSK_Номер помещения квартиры = 69890ae1-d66e-4fe9-aced-024c27719f53 (string)
        /// </summary>
        public static Guid ADSK_RoomNumberInApartment => Guid.Parse("69890ae1-d66e-4fe9-aced-024c27719f53");

        /// <summary>
        /// Guid параметра PGS_Идентификация = 3edf98d0-93bb-44bc-a1ab-8d401cacaf28 (string)
        /// </summary>
        public static Guid PGS_Identification => Guid.Parse("3edf98d0-93bb-44bc-a1ab-8d401cacaf28");

        /// <summary>
        /// Guid параметра ADSK_Потеря давления воздуха в нагревателе = f51f7bf1-e50f-4563-aa17-4d1c12a2be81 (double)
        /// </summary>
        public static Guid ADSK_LossPreasureInAirHeater => Guid.Parse("f51f7bf1-e50f-4563-aa17-4d1c12a2be81");

        /// <summary>
        /// Guid параметра ADSK_Температура воздуха на входе в нагреватель = f4f0aae1-c6e9-40be-98d0-d58f84e9d0f0 (double)
        /// </summary>
        public static Guid ADSK_AirTempInAirHeater => Guid.Parse("f4f0aae1-c6e9-40be-98d0-d58f84e9d0f0");

        /// <summary>
        /// Guid параметра ADSK_Температура воздуха на выходе из нагревателя = 4cbc6f9d-54f5-4adc-879f-6ae38e5190c5 (double)
        /// </summary>
        public static Guid ADSK_AirTempOutAirHeater => Guid.Parse("4cbc6f9d-54f5-4adc-879f-6ae38e5190c5");

        /// <summary>
        /// Guid параметра ADSK_Тепловая мощность = be7d2b1b-1916-428f-87f0-d9ee8d4f1efe (double)
        /// </summary>
        public static Guid ADSK_PowerThermal => Guid.Parse("be7d2b1b-1916-428f-87f0-d9ee8d4f1efe");

        /// <summary>
        /// Guid параметра PGS_ВоздухонагревательМощность = 55a3312a-cd79-45ae-8c0b-5b51338d4c70 (double)
        /// </summary>
        public static Guid PGS_AirHeaterPower => Guid.Parse("55a3312a-cd79-45ae-8c0b-5b51338d4c70");

        /// <summary>
        /// Guid параметра PGS_ВоздухонагревательТип = efd37911-6c8e-40a4-990c-574743cf300d (string)
        /// </summary>
        public static Guid PGS_AirHeaterType => Guid.Parse("efd37911-6c8e-40a4-990c-574743cf300d");

        /// <summary>
        /// Guid параметра PGS_ВоздухонагревательКоличество = 2bb6ed27-17da-45ed-9dcd-2cc54ffedd25 (double)
        /// </summary>
        public static Guid PGS_AirHeaterCount => Guid.Parse("2bb6ed27-17da-45ed-9dcd-2cc54ffedd25");

        /// <summary>
        /// Guid параметра ADSK_Потеря давления воздуха в охладителе = 35e504b6-9ff1-4feb-8179-3f145e701ca8 (double)
        /// </summary>
        public static Guid ADSK_LossPreasureInAirCooler => Guid.Parse("35e504b6-9ff1-4feb-8179-3f145e701ca8");

        /// <summary>
        /// Guid параметра ADSK_Температура воздуха на входе в охладитель = 35d72244-2520-43ec-b36f-1a2c4527988b (double)
        /// </summary>
        public static Guid ADSK_AirTempInAirCooler => Guid.Parse("35d72244-2520-43ec-b36f-1a2c4527988b");

        /// <summary>
        /// Guid параметра ADSK_Температура воздуха на выходе из охладителя = b8549b18-1d9f-430e-a2e8-cdbfa511712d (double)
        /// </summary>
        public static Guid ADSK_AirTempOutAirCooler => Guid.Parse("b8549b18-1d9f-430e-a2e8-cdbfa511712d");

        /// <summary>
        /// Guid параметра ADSK_Холодильная мощность = f07965f9-3490-4c68-9404-3fa6721c1c8c (double)
        /// </summary>
        public static Guid ADSK_PowerCooling => Guid.Parse("f07965f9-3490-4c68-9404-3fa6721c1c8c");

        /// <summary>
        /// Guid параметра PGS_ВоздухоохладительМощность = 5b5a38a4-beb7-42dc-9692-6317788af685 (double)
        /// </summary>
        public static Guid PGS_AirCoolerPower => Guid.Parse("5b5a38a4-beb7-42dc-9692-6317788af685");

        /// <summary>
        /// Guid параметра PGS_ВоздухоохладительТип = bbb93870-933b-4d82-838c-409cb9c51e00 (string)
        /// </summary>
        public static Guid PGS_AirCoolerType => Guid.Parse("bbb93870-933b-4d82-838c-409cb9c51e00");

        /// <summary>
        /// Guid параметра PGS_ВоздухоохладительКоличество = 4ba316f2-2b88-4942-8dbf-d269909e0cf7 (double)
        /// </summary>
        public static Guid PGS_AirCoolerCount => Guid.Parse("4ba316f2-2b88-4942-8dbf-d269909e0cf7");

        /// <summary>
        /// Guid параметра ADSK_Сопротивление воздушного фильтра = 8018c952-ae41-4f18-90cd-e5c59c0a1517 (double)
        /// </summary>
        public static Guid ADSK_AirFilterResistance => Guid.Parse("8018c952-ae41-4f18-90cd-e5c59c0a1517");

        /// <summary>
        /// Guid параметра PGS_ФильтрТип = 5cedb7af-049a-4461-8d36-c631be95d07e (string)
        /// </summary>
        public static Guid PGS_FilterType => Guid.Parse("5cedb7af-049a-4461-8d36-c631be95d07e");

        /// <summary>
        /// Guid параметра PGS_ФильтрКоличество = 3f2d437e-6eea-444a-b13d-56620718dd75 (double)
        /// </summary>
        public static Guid PGS_FilterCount => Guid.Parse("3f2d437e-6eea-444a-b13d-56620718dd75");

        /// <summary>
        /// Guid параметра PGS_Наименование системы = f2381a1b-8c10-41a3-984b-21c10b911338 (string)
        /// </summary>
        public static Guid PGS_SystemName => Guid.Parse("f2381a1b-8c10-41a3-984b-21c10b911338");


        /// <summary>
        /// Валидация текущего проекта Revit на наличие общих параметров у заданной категории.
        /// </summary>
        /// <param name="doc">Документ Revit.</param>
        /// <param name="category">Категория Revit.</param>
        /// <param name="sharedParamsGuids">Массив Guid необходимых общих параметров для заданной категории.</param>
        /// <returns>True, если все параметры из массива присутствуют у категории, иначе False.</returns>
        public static bool IsCategoryOfDocContainsSharedParams(Document doc, BuiltInCategory category, Guid[] sharedParamsGuids)
        {
            ElementId categoryId = Category.GetCategory(doc, category).Id;
            bool containsAll = true;
            foreach (Guid sharedParamGuid in sharedParamsGuids)
            {
                try
                {
                    ElementId parId = SharedParameterElement.Lookup(doc, sharedParamGuid).Id;
                    containsAll = containsAll && TableView.GetAvailableParameters(doc, categoryId).Contains(parId);
                }
                catch (NullReferenceException)
                {
                    return false;
                }
                catch (ArgumentNullException)
                {
                    return false;
                }
            }
            return containsAll;
        }

        public static string CreateErrorMessage(out string message, in List<ElementId> elements)
        {
            if (elements.Count > 0)
            {
                message = String.Join(", ", elements.Select(e => e.ToString()));
            }
            else
            {
                message = "отсутствуют";
            }
            return message;
        }
    }
}
