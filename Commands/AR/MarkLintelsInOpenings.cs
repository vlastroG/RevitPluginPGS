using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.DTO;
using MS.GUI.AR;
using MS.Shared;
using MS.Utilites;
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
                SharedParams.PGS_MarkLintel
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Doors,
                _sharedParamsForOpenings))
            {
                MessageBox.Show("В текущем проекте у категории \"Двери\" " +
                    "Присутствуют НЕ ВСЕ необходимые общие параметры:" +
                    "\nМрк.МаркаКонструкции" +
                    "\nPGS_МаркаПеремычки",
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
                    "\nPGS_МаркаПеремычки",
                    "Ошибка");
                return Result.Cancelled;
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
                .Select(f => new OpeningDto(doc, f))
            .ToList();

            var endToEndMark = UserInput.YesNoCancelInput("Маркировка", "Если маркировка сквозная - \"Да\", поэтажно - \"Нет\"");
            if (endToEndMark != System.Windows.Forms.DialogResult.Yes && endToEndMark != System.Windows.Forms.DialogResult.No)
            {
                return Result.Cancelled;
            }
            bool marking;
            if (endToEndMark == System.Windows.Forms.DialogResult.Yes)
            {
                marking = true;
            }
            else
            {
                marking = false;
            }

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
                    }
                    // Назначить Мрк.МаркаКонструкции в экземпляр семейства,
                    // если значение отличается от марки перемычки DTO
                    if (opening.Opening
                        .get_Parameter(SharedParams.Mrk_MarkOfConstruction).AsValueString() != lintelMark)
                    {
                        opening.Opening
                            .get_Parameter(SharedParams.Mrk_MarkOfConstruction)
                            .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
                    }
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}