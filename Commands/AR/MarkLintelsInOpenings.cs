using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Commands.AR.DTO;
using MS.GUI.AR;
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
        private static readonly Guid _parPgsLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");


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

            var openings = filter_openings.WherePasses(filtered_categories)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as FamilyInstance)
                .Where(f => f.Host != null)
                .Where(f =>
                (BuiltInCategory)f.Host.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
                .Select(f => new OpeningDto(f))
                .ToList();

            //Вывод окна входных данных
            OpeningsLintelsMark inputForm = new OpeningsLintelsMark(openings);
            inputForm.ShowDialog();

            if (inputForm.DialogResult == false)
            {
                return Result.Cancelled;
            }

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Назначить марки перемычек");

                foreach (OpeningDto opening in openings)
                {
                    opening.Opening
                        .get_Parameter(_parPgsLintelMark)
                        .Set(OpeningDto.DictLintelMarkByHashCode[opening.GetHashCode()]);
                    opening.Opening
                        .get_Parameter(BuiltInParameter.ALL_MODEL_MARK)
                        .Set(OpeningDto.DictOpeningMarkByHashCode[opening.GetHashCode()]);
                }

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
