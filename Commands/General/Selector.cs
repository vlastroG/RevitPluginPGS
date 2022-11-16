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
        /// <summary>
        /// Настройки категории выбираемых элементов
        /// </summary>
        private static SelectorViewModel _settings = new SelectorViewModel();

        /// <summary>
        /// Получить категорию для выбора элементов в текущем документу по настройкам
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
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
            try
            {
                var test = _settings.SelectedCategory.Name;
            }
            catch (NullReferenceException)
            {
                var settingsCmd = new SelectorSettingsCmd();
                var result = settingsCmd.Execute(commandData, ref message, elements);
                if (result != Result.Succeeded)
                {
                    return null;
                }
            }
            catch (AccessViolationException)
            {
                var settingsCmd = new SelectorSettingsCmd();
                var result = settingsCmd.Execute(commandData, ref message, elements);
                if (result != Result.Succeeded)
                {
                    return null;
                }
            }
            Category category = null;
            var categories = commandData.Application.ActiveUIDocument.Document.Settings.Categories;
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
