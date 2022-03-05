using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            FilterSelectByClick(commandData.Application.ActiveUIDocument);

            return Result.Succeeded;
        }
        public void FilterSelectByClick(UIDocument uIDocument)
        {
            UIDocument uidoc = uIDocument;
            Document doc = uidoc.Document;

            Reference refer = uidoc.Selection.PickObject(ObjectType.Element, "Выберите элемент для назначения категории фильтру."); //Pick an object by which Category you will filter

            IList<Element> elements = uidoc.Selection.PickElementsByRectangle();

            uidoc.Selection.SetElementIds(elements
                .Where(x => x.Category.Id.IntegerValue.Equals(doc.GetElement(refer).Category.Id.IntegerValue))
                .Select(x => x.Id)
                .ToList());
        }

        
    }
}
