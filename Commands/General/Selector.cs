using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using MS.GUI.General;
using MS.Commands.General;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        private static SelectorSettingsCmd _settings = new SelectorSettingsCmd();

        private Category GetCategory(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            if (SelectorSettingsCmd.Category == null)
            {
                var result = _settings.Execute(commandData, ref message, elements);
                if (result != Result.Succeeded)
                {
                    return null;
                }
            }
            var categories = commandData.Application.ActiveUIDocument.Document.Settings.Categories;
            Category category = null;
            foreach (Category categoryInDoc in categories)
            {
                if (categoryInDoc.Name == SelectorSettingsCmd.Category.Name)
                {
                    category = categoryInDoc;
                    break;
                }
            }
            return category;
        }

        /// <summary>
        /// Выбирает элементы заданной категории на текущем виде
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Выбор элементов заданной категории текущем виде
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Category category = GetCategory(commandData, ref message, elements);
            if (category == null)
            {
                return Result.Cancelled;
            }

            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { (BuiltInCategory)category.Id.IntegerValue },
                false);
            List<Element> elems = null;
            try
            {
                elems = uidoc.Selection
                    .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        $"Выберите элементы категории {category}")
                    .Select(e => doc.GetElement(e))
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            uidoc.Selection.SetElementIds(elems.Select(it => it.Id).ToList());

            return Result.Succeeded;
        }
    }
}
