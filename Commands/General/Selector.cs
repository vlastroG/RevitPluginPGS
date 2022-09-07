using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using MS.GUI.General;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Selector : IExternalCommand
    {
        private static string _category = "Помещения";

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

            var categories = doc.Settings.Categories;
            List<Category> categoriesList = new List<Category>();
            foreach (Category category in categories)
            {
                categoriesList.Add(category);
            }
            categoriesList.Sort((x, y) => x.Name.CompareTo(y.Name));

            var form = new CategoryInput(categoriesList, _category);
            form.ShowDialog();
            if (form.DialogResult != true)
            {
                return Result.Cancelled;
            }
            _category = form.Category.Name;
            var categorySelected = form.Category;
            if (categorySelected == null)
            {
                return Result.Cancelled;
            }

            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory> { (BuiltInCategory)categorySelected.Id.IntegerValue },
                false);
            List<Element> elems = null;
            try
            {
                elems = uidoc.Selection
                    .PickObjects(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        $"Выберите элементы категории {_category}")
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
