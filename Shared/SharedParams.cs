using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Shared
{
    public static class SharedParams
    {
        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки = aee96840-3b85-4cb6-a93e-85acee0be8c7
        /// </summary>
        public static readonly Guid PGS_MarkLintel = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        /// <summary>
        /// Guid параметра ADSK_МассаЭлемента = 5913a1f9-0b38-4364-96fe-a6f3cb7fcc68
        /// </summary>
        public static readonly Guid ADSK_MassElement = Guid.Parse("5913a1f9-0b38-4364-96fe-a6f3cb7fcc68");

        /// <summary>
        /// Guid параметра PGS_МассаПеремычки = 77a36313-f239-426c-a6f9-29bf64efee76
        /// </summary>
        public static readonly Guid PGS_MassLintel = Guid.Parse("77a36313-f239-426c-a6f9-29bf64efee76");

        /// <summary>
        /// Guid параметра PGS_ИзображениеТипоразмераМатериала = 924e3bb2-a048-449f-916f-31093a3aa7a3
        /// </summary>
        public static readonly Guid PGS_ImageTypeMaterial = Guid.Parse("924e3bb2-a048-449f-916f-31093a3aa7a3");

        /// <summary>
        /// Guid параметра Орг.ТипВключатьВСпецификацию = 45ef1720-9cfe-49a7-b4d7-c67e4f7bd191
        /// </summary>
        public static readonly Guid Org_TypeIncludeInSchedule = Guid.Parse("45ef1720-9cfe-49a7-b4d7-c67e4f7bd191");

        /// <summary>
        /// Guid параметра "ADSK_Номер квартиры" = 10fb72de-237e-4b9c-915b-8849b8907695
        /// </summary>
        public static readonly Guid ADSK_NumberOfApartment = Guid.Parse("10fb72de-237e-4b9c-915b-8849b8907695");

        /// <summary>
        /// Guid параметра "ADSK_Тип помещения" = 56eb1705-f327-4774-b212-ef9ad2c860b0
        /// </summary>
        public static readonly Guid ADSK_TypeOfRoom = Guid.Parse("56eb1705-f327-4774-b212-ef9ad2c860b0");

        /// <summary>
        /// Guid параметра "ADSK_Количество комнат" = f52108e1-0813-4ad6-8376-a38a1a23a55b
        /// </summary>
        public static readonly Guid ADSK_CountOfRooms = Guid.Parse("f52108e1-0813-4ad6-8376-a38a1a23a55b");

        /// <summary>
        /// Guid параметра "ADSK_Коэффициент площади" = 066eab6d-c348-4093-b0ca-1dfe7e78cb6e
        /// </summary>
        public static readonly Guid ADSK_CoeffOfArea = Guid.Parse("066eab6d-c348-4093-b0ca-1dfe7e78cb6e");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры" = d3035d0f-b738-4407-a0e5-30787b92fa49
        /// </summary>
        public static readonly Guid ADSK_AreaOfApartment = Guid.Parse("d3035d0f-b738-4407-a0e5-30787b92fa49");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры жилая" = 178e222b-903b-48f5-8bfc-b624cd67d13c
        /// </summary>
        public static readonly Guid ADSK_AreaOfApartmentLiving = Guid.Parse("178e222b-903b-48f5-8bfc-b624cd67d13c");

        /// <summary>
        /// Guid параметра "ADSK_Площадь квартиры общая" = af973552-3d15-48e3-aad8-121fe0dda34e
        /// </summary>
        public static readonly Guid ADSK_AreaOfApartmentTotal = Guid.Parse("af973552-3d15-48e3-aad8-121fe0dda34e");

        /// <summary>
        /// Guid параметра "ADSK_Индекс квартиры" = a2985e5c-b28e-416a-acf6-7ab7e4ee6d86
        /// </summary>
        public static readonly Guid ADSK_IndexOfApartment = Guid.Parse("a2985e5c-b28e-416a-acf6-7ab7e4ee6d86");

        /// <summary>
        /// Guid параметра "ADSK_Тип квартиры" = 78e3b89c-eb68-4600-84a7-c523de162743
        /// </summary>
        public static readonly Guid ADSK_TypeOfApartment = Guid.Parse("78e3b89c-eb68-4600-84a7-c523de162743");

        /// <summary>
        /// Guid параметра "ADSK_Площадь проемов" = 18e3f49d-1315-415f-8359-8f045a7a8938
        /// </summary>
        public static readonly Guid ADSK_AreaOfOpenings = Guid.Parse("18e3f49d-1315-415f-8359-8f045a7a8938");

        /// <summary>
        /// Guid параметра Мрк.МаркаКонструкции = 5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405
        /// </summary>
        public static readonly Guid Mrk_MarkOfConstruction = Guid.Parse("5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405");

        /// <summary>
        /// Guid параметра ADSK_Марка = 2204049c-d557-4dfc-8d70-13f19715e46d
        /// </summary>
        public static readonly Guid ADSK_Mark = Guid.Parse("2204049c-d557-4dfc-8d70-13f19715e46d");

        /// <summary>
        /// Guid параметра Арм.КолвоАрмированияКладки = fb502bf5-cbd4-416b-8ce1-cbf2ac40c3b6
        /// </summary>
        public static readonly Guid Arm_CountReinforcedRowsMasonry = Guid.Parse("fb502bf5-cbd4-416b-8ce1-cbf2ac40c3b6");

        /// <summary>
        /// Guid параметра Рзм.Ширина = 8f2e4f93-9472-4941-a65d-0ac468fd6a5d
        /// </summary>
        public static readonly Guid ADSK_DimensionWidth = Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");

        /// <summary>
        /// Guid параметра Рзм.Высота = da753fe3-ecfa-465b-9a2c-02f55d0c2ff1
        /// </summary>
        public static readonly Guid ADSK_DimensionHeight = Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");

        /// <summary>
        /// Guid параметра Арм.ПлощадьКлСетки = 7f925503-538c-43f3-9e75-aa7a3f43eb0e
        /// </summary>
        public static readonly Guid Arm_AreaOfMasonryMesh = Guid.Parse("7f925503-538c-43f3-9e75-aa7a3f43eb0e");

        /// <summary>
        /// Guid параметра ADSK_Толщина стены = 9350e48f-842b-4c46-a15d-2e36ab1f352f
        /// </summary>
        public static readonly Guid ADSK_ThicknessOfWall = Guid.Parse("9350e48f-842b-4c46-a15d-2e36ab1f352f");

        /// <summary>
        /// Guid параметра ADSK_Наименование = e6e0f5cd-3e26-485b-9342-23882b20eb43
        /// </summary>
        public static readonly Guid ADSK_Name = Guid.Parse("e6e0f5cd-3e26-485b-9342-23882b20eb43");

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
