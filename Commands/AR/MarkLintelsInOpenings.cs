using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.DTO;
using MS.GUI.AR;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
                SharedParams.Mrk_MarkOfConstruction,
                SharedParams.PGS_MarkLintel
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_GenericModel,
                _sharedParamsForGenericModel))
            {
                MessageBox.Show("В текущем проекте у категории \"Обобщенные модели\" " +
                    "Присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nМрк.МаркаКонструкции" +
                    "\nPGS_МаркаПеремычки",
                    "Ошибка");
                return Result.Cancelled;
            }

            Guid[] _sharedParamsForOpenings = new Guid[] {
                SharedParams.Mrk_MarkOfConstruction,
                SharedParams.PGS_MarkLintel,
                SharedParams.PGS_MassLintel
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Doors,
                _sharedParamsForOpenings))
            {
                MessageBox.Show("В текущем проекте у категории \"Двери\" " +
                    "Присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nМрк.МаркаКонструкции" +
                    "\nPGS_МаркаПеремычки" +
                    "\nPGS_МассаПеремычки",
                    "Ошибка");
                return Result.Cancelled;
            }

            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Windows,
                _sharedParamsForOpenings))
            {
                MessageBox.Show("В текущем проекте у категории \"Окна\"" +
                    "Присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nМрк.МаркаКонструкции" +
                    "\nPGS_МаркаПеремычки" +
                    "\nPGS_МассаПеремычки",
                    "Ошибка");
                return Result.Cancelled;
            }

            var endToEndMark = UserInput.YesNoCancelInput("Маркировка", "Если маркировка сквозная - \"Да\", поэтажно - \"Нет\"");
            if (endToEndMark != System.Windows.Forms.DialogResult.Yes && endToEndMark != System.Windows.Forms.DialogResult.No)
            {
                return Result.Cancelled;
            }
            bool marking;
            if (endToEndMark == System.Windows.Forms.DialogResult.Yes)
            {
                // Маркировка сквозная, в хэш-код OpeningDto не включать Уровень
                marking = true;
            }
            else
            {
                // Маркировка поэтажная, в хэш-код OpeningDto включать Уровень
                marking = false;
            }

            var filter_openings = new FilteredElementCollector(doc);
            var filtered_categories = new ElementMulticategoryFilter(
                new Collection<BuiltInCategory> {
                     BuiltInCategory.OST_Windows,
                     BuiltInCategory.OST_Doors});

            var openings = filter_openings.WherePasses(filtered_categories)
            .WhereElementIsNotElementType()
            .ToElements()
            .Cast<FamilyInstance>()
            .Where(f => f.Host != null)
            .Where(f =>
              (BuiltInCategory)f.Host.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
            .Where(f => f.get_Parameter(SharedParams.PGS_MarkLintel) != null)
            .Where(f => f.get_Parameter(SharedParams.Mrk_MarkOfConstruction) != null)
            .Where(f => f.GetSubComponentIds().FirstOrDefault(
                             id => (doc.GetElement(id) as FamilyInstance).Symbol
                             .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                             .AsValueString() == SharedValues.LintelDescription) != null)
            .Select(f => new OpeningDto(doc, f, !marking))
            .ToList();

            int lintelMarkSetCount = 0;
            int mrkMarkConstrSetCount = 0;
            int lintelMassSetCount = 0;



            //Вывод окна входных данных
            OpeningsLintelsMark inputForm = new OpeningsLintelsMark(openings, marking);
            inputForm.ShowDialog();

            if (inputForm.DialogResult == false)
            {
                return Result.Cancelled;
            }

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("PGS set marks lintels");

                foreach (OpeningDto opening in openings)
                {
                    // Марка перемычки
                    string lintelMark = OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()];

                    // Назначить PGS_МаркаПеремычки в экземпляр семейства,
                    // если значение отличается от марки перемычки DTO.
                    if (opening.Opening.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString() != lintelMark)
                    {
                        opening.Opening
                            .get_Parameter(SharedParams.PGS_MarkLintel)
                            .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
                        lintelMarkSetCount++;
                    }
                    // Назначить Мрк.МаркаКонструкции в экземпляр семейства,
                    // если значение отличается от марки перемычки DTO
                    if (opening.Opening
                        .get_Parameter(SharedParams.Mrk_MarkOfConstruction).AsValueString() != lintelMark)
                    {
                        opening.Opening
                            .get_Parameter(SharedParams.Mrk_MarkOfConstruction)
                            .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
                        mrkMarkConstrSetCount++;
                    }
                    if (opening.Opening
                        .get_Parameter(SharedParams.PGS_MassLintel).AsDouble()
                        != opening.Lintel.get_Parameter(SharedParams.ADSK_MassElement).AsDouble())
                    {
                        opening.Opening.get_Parameter(SharedParams.PGS_MassLintel).Set(opening.MassOfLintel);
                        lintelMassSetCount++;
                    }
                }

                trans.Commit();
            }

            MessageBox.Show(
                $"Принято в обработку {openings.Count} экземпляров семейств окон и дверей." +
                $"\n\nPGS_МаркаПеремычки назначен {lintelMarkSetCount} раз," +
                $"\nМрк.МаркаКонструкции назначен {mrkMarkConstrSetCount} раз," +
                $"\nPGS_МассаПеремычки назначен {lintelMassSetCount} раз.",
                "Маркировка переимычек");

            return Result.Succeeded;
        }
    }
}