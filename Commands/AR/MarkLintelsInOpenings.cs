using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.DTO;
using MS.GUI.AR;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MarkLintelsInOpenings : IExternalCommand
    {
        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки
        /// </summary>
        private static readonly Guid _parPgsLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        /// <summary>
        /// Guid параметра Мрк.МаркаКонструкции
        /// </summary>
        private static readonly Guid _parMrkMarkConstruction = Guid.Parse("5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405");

        /// <summary>
        /// Guid параметра ADSK_Марка
        /// </summary>
        private static readonly Guid _parAdskMarkOfSymbol = Guid.Parse("2204049c-d557-4dfc-8d70-13f19715e46d");

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;


            var filter_openings = new FilteredElementCollector(doc);
            var filtered_categories = new ElementMulticategoryFilter(
                new Collection<BuiltInCategory> {
                     BuiltInCategory.OST_Windows,
                     BuiltInCategory.OST_Doors});

            //var openings = filter_openings.WherePasses(filtered_categories)
            //    .WhereElementIsNotElementType()
            //    .ToElements()
            //    .Cast<FamilyInstance>()
            //    .Where(f => f.Host != null)
            //    .Where(f =>
            //    (BuiltInCategory)f.Host.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
            //    .Where(f => f.get_Parameter(_parPgsLintelMark) != null)
            //    .Where(f => f.get_Parameter(_parMrkMarkConstruction) != null)
            //    .Where(f => f.Symbol.get_Parameter(_parAdskMarkOfSymbol) != null)
            //    .Select(f => new OpeningDto(f))
            //    .ToList();
            string lintelOn = "ПеремычкаВКЛ";
            string lintelDescription = "Перемычка";
            string PGS_MassLintel = "PGS_МассаПеремычки";
            var openings = filter_openings.WherePasses(filtered_categories)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .Where(f => f.Host != null)
                .Where(f =>
                (BuiltInCategory)f.Host.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
                .Where(f => f.LookupParameter(lintelOn) != null && f.LookupParameter(lintelOn).AsInteger() == 1)
                .ToList();

            using (Transaction transMarkOpenings = new Transaction(doc))
            {
                transMarkOpenings.Start("Назначить массы перемычек");
                foreach (FamilyInstance opening in openings)
                {
                    var lintelId = opening
                        .GetSubComponentIds()
                     .Where(
                             sub => (doc.GetElement(sub) as FamilyInstance).Symbol
                             .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                             .AsValueString() == lintelDescription)
                     .FirstOrDefault();
                    var lintelElem = doc.GetElement(lintelId);
                    var lintelParamAdskMassElem = lintelElem.get_Parameter(SharedParams.ADSK_MassElement);
                    if (lintelParamAdskMassElem != null)
                    {
                        opening.LookupParameter(PGS_MassLintel)
                            .Set(lintelElem.get_Parameter(SharedParams.ADSK_MassElement).AsDouble());
                    }
                }
                transMarkOpenings.Commit();
            }

            //Вывод окна входных данных
            //OpeningsLintelsMark inputForm = new OpeningsLintelsMark(openings);
            //inputForm.ShowDialog();

            //if (inputForm.DialogResult == false)
            //{
            //    return Result.Cancelled;
            //}

            //using (Transaction trans = new Transaction(doc))
            //{
            //    trans.Start("Назначить марки перемычек");

            //    foreach (OpeningDto opening in openings)
            //    {
            //        // Марка перемычки
            //        string lintelMark = OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()];
            //        // Марка проема
            //        string openingMark = OpeningDto.DictOpeningMarkByHashCode[opening.GetHashCode()];

            //        // Назначить PGS_МаркаПеремычки в экземпляр семейства,
            //        // если значение отличается от марки перемычки DTO.
            //        if (opening.Opening.get_Parameter(_parPgsLintelMark).AsValueString() != lintelMark)
            //        {
            //            opening.Opening
            //                .get_Parameter(_parPgsLintelMark)
            //                .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
            //        }
            //        // Назначить Марку в экземпляр семейства,
            //        // если значение отличается от марки проема DTO
            //        if (opening.Opening
            //            .get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsValueString() != openingMark)
            //        {
            //            opening.Opening
            //                .get_Parameter(BuiltInParameter.ALL_MODEL_MARK)
            //                .Set(OpeningDto.DictOpeningMarkByHashCode[opening.GetHashCode()]);
            //        }
            //        // Назначить Мрк.МаркаКонструкции в экземпляр семейства,
            //        // если значение отличается от марки перемычки DTO
            //        if (opening.Opening
            //            .get_Parameter(_parMrkMarkConstruction).AsValueString() != lintelMark)
            //        {
            //            opening.Opening
            //                .get_Parameter(_parMrkMarkConstruction)
            //                .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
            //        }
            //    }

            //    trans.Commit();
            //}

            return Result.Succeeded;
        }
    }
}