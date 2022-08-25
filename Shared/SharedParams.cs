using Autodesk.Revit.DB;
using System;

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
    }
}
