using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        /// <summary>
        /// Выбирает помещения рамкой на текущем виде
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Выбор помещений рамкой на текущем виде
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter = new SelectionFilterRooms();

            List<Element> selectedElements = null;
            try
            {
                selectedElements = uidoc.Selection.PickElementsByRectangle(filter, "Выберите помещения.").ToList();
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }

            uidoc.Selection.SetElementIds(selectedElements.Select(it => it.Id).ToList());

            return Result.Succeeded;
        }


        /// <summary>
        /// Старый метод выбора элементов
        /// </summary>
        /// <param name="uIDocument"></param>
        private void FilterSelectByClick(UIDocument uIDocument)
        {
            UIDocument uidoc = uIDocument;
            Document doc = uidoc.Document;
            //Pick an object by which Category you will filter
            Reference refer = uidoc.Selection.PickObject(ObjectType.Element, "Выберите элемент для назначения категории фильтру.");

            IList<Element> elements = uidoc.Selection.PickElementsByRectangle();

            uidoc.Selection.SetElementIds(elements
                .Where(x => x.Category.Id.IntegerValue.Equals(doc.GetElement(refer).Category.Id.IntegerValue))
                .Select(x => x.Id)
                .ToList());
        }
    }
}
