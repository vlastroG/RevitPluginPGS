using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System.Collections.Generic;
using System.Linq;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Выбор помещений рамкой на текущем виде
            SelectByBuiltInCategory(
                commandData.Application.ActiveUIDocument,
                BuiltInCategory.OST_Walls,
                "Выберите помещения.");

            return Result.Succeeded;
        }

        /// <summary>
        /// Выбор элементов рамкой по заданной категории
        /// </summary>
        /// <param name="uIDocument">Активный документ</param>
        /// <param name="category">Встроенная категория для выбора</param>
        /// <param name="message">Подсказка для пользователя</param>
        private void SelectByBuiltInCategory(UIDocument uIDocument, BuiltInCategory category, string message)
        {
            UIDocument uidoc = uIDocument;

            var filter = new SelectionBuiltInCategoryFilter(category);

            IList<Element> elements = uidoc.Selection.PickElementsByRectangle(filter, message);
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
