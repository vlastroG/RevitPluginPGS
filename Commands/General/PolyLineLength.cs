using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.Commands.General
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PolyLineLength : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            var selectedLines = sel.GetElementIds().Select(id => doc.GetElement(id))
                .Where(e => (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(e)
                == BuiltInCategory.OST_Lines)
                .ToList();

            double lengthTotal = 0;
            if (selectedLines.Count > 0)
            {
                foreach (var line in selectedLines)
                {
                    lengthTotal += line.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble()
                        * SharedValues.FootToMillimeters;
                }
                MessageBox.Show($"Суммарная длина выбранных линий: {lengthTotal}", "Длина линий");
                return Result.Succeeded;
            }
            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { BuiltInCategory.OST_Lines },
                false);
            List<Element> lines = null;
            try
            {
                lines = uidoc.Selection
                    .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        "Выберите линии")
                    .Select(e => doc.GetElement(e))
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            foreach (var line in lines)
            {
                lengthTotal += line.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble() * SharedValues.FootToMillimeters;
            }
            MessageBox.Show($"Суммарная длина выбранных линий: {lengthTotal}", "Длина линий");
            return Result.Succeeded;
        }
    }
}
