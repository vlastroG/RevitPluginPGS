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
using MS.GUI.ViewModels.General;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        private static SelectorViewModel _settings = new SelectorViewModel();

        private Category GetCategory(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            if (_settings.SelectedCategory is null)
            {
                var settingsCmd = new SelectorSettingsCmd();
                var result = settingsCmd.Execute(commandData, ref message, elements);
                if (result != Result.Succeeded)
                {
                    return null;
                }
            }
            var categories = commandData.Application.ActiveUIDocument.Document.Settings.Categories;
            try
            {
                var test = _settings.SelectedCategory.Name;
            }
            catch (AccessViolationException)
            {
                _settings.SelectedCategory = SelectorViewModel.Categories.FirstOrDefault(c => c.Name.Equals("Помещения"));
            }
            Category category = null;
            foreach (Category categoryInDoc in categories)
            {
                if (categoryInDoc.Name.Equals(_settings.SelectedCategory.Name))
                {
                    return categoryInDoc;
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
                        $"Выберите элементы категории {category.Name}")
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
