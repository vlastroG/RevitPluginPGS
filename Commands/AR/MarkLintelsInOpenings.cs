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
using System.Windows;

namespace MS.Commands.AR
{
    /// <summary>
    /// Скрипт для назначения марок перемычкам
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MarkLintelsInOpenings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Guid[] _sharedParamsForGenericModel = new Guid[] {
                SharedParams.Mrk_MarkOfConstruction
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_GenericModel,
                _sharedParamsForGenericModel))
            {
                MessageBox.Show("В текущем проекте у категории \"Обобщенные модели\" " +
                    "отсутствует общий параметр:" +
                    "\nМрк.МаркаКонструкции",
                    "Ошибка");
                return Result.Cancelled;
            }

            Guid[] _sharedParamsForOpenings = new Guid[] {
                SharedParams.ADSK_Mark
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Doors,
                _sharedParamsForOpenings))
            {
                MessageBox.Show("В текущем проекте у категории \"Двери\" " +
                    "отсутствует общий параметр:" +
                    "\nADSK_Марка",
                    "Ошибка");
                return Result.Cancelled;
            }

            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Windows,
                _sharedParamsForOpenings))
            {
                MessageBox.Show("В текущем проекте у категории \"Окна\"" +
                    "отсутствует общий параметр:" +
                    "\nADSK_Марка",
                    "Ошибка");
                return Result.Cancelled;
            }

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
            var openings = filter_openings.WherePasses(filtered_categories)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<FamilyInstance>()
                .Where(f => f.Host != null)
                .Where(f =>
                (BuiltInCategory)f.Host.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
                .Where(f => f.get_Parameter(SharedParams.PGS_MarkLintel) != null)
                .Where(f => f.get_Parameter(SharedParams.Mrk_MarkOfConstruction) != null)
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
                             .AsValueString() == SharedValues.LintelDescription)
                        .FirstOrDefault();
                    var lintelElem = doc.GetElement(lintelId);
                    var lintelParamAdskMassElem = lintelElem.get_Parameter(SharedParams.ADSK_MassElement);
                    if (lintelParamAdskMassElem != null)
                    {
                        try
                        {
                            opening.get_Parameter(SharedParams.PGS_MarkLintel)
                                .Set(lintelElem.get_Parameter(SharedParams.ADSK_MassElement).AsDouble());
                        }
                        catch (ArgumentNullException)
                        {
                            throw new ArgumentNullException(
                                $"В экземпляре семейства {opening.Id} отсутствует параметр PGS_МассаПеремычки.");
                        }
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