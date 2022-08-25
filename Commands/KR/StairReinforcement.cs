using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StairReinforcement : IExternalCommand
    {
        /// <summary>
        /// Армирование лестничного марша
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Фильтр выбора лестниц, созданных инструментом "лестницы",
            // загружаемым семейством или моделью в контексте категории "лестницы".
            List<BuiltInCategory> categoryList = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Stairs,
                BuiltInCategory.OST_StructuralFraming
            };
            var filter = new SelectionFilterElementsOfCategory<Stairs>(categoryList, true);
            // Выбранные элементы - лестницы
            List<Element> selectedElements = null;
            try
            {
                selectedElements = uidoc.Selection.PickObjects(ObjectType.Element, filter, "Выберите лестницы.").Select(e => doc.GetElement(e.ElementId)).ToList();
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }

            // Тестовый выбор первой лестницы из списка
            var testStair = selectedElements.FirstOrDefault();
            var stair = new StairModel(testStair);

            return Result.Succeeded;
        }
    }
}
