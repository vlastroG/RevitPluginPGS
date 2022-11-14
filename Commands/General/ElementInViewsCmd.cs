using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.General;
using MS.GUI.ViewModels.General;
using MS.Utilites.SelectionFilters;
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
    public class ElementInViewsCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            var el = GetElement(commandData);

            var views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(v => !(v is ViewSchedule) && !(v is ViewSheet) && !(v is View3D))
                .Where(v => FilteredElementCollector.IsViewValidForElementIteration(doc, v.Id))
                .Where(v => IsElementVisibleInView(v, el));

            ElementInViewsViewModel vm = new ElementInViewsViewModel(views);
            ElementInViews ui = new ElementInViews(vm);
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return Result.Cancelled;
            }
            var viewModel = ui.DataContext as ElementInViewsViewModel;
            var selectedView = viewModel.SelectedView;
            if (selectedView is null)
            {
                return Result.Cancelled;
            }
            bool goToView = viewModel.GoToSheet;
            if (goToView)
            {
                try
                {
                    string sheetNumber = selectedView.SheetNumber;
                    ViewSheet sheet = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewSheet))
                    .WhereElementIsNotElementType()
                    .Cast<ViewSheet>()
                    .First(
                        v => v
                        .get_Parameter(BuiltInParameter.SHEET_NUMBER)
                        .AsValueString()
                        .Equals(sheetNumber));
                    uidoc.ActiveView = sheet;
                }
                catch (Exception)
                {
                    uidoc.ActiveView = selectedView.View;
                }
            }
            else
            {
                uidoc.ActiveView = selectedView.View;
            }


            uidoc.Selection.SetElementIds(new List<ElementId>() { el.Id });
            return Result.Succeeded;
        }

        /// <summary>
        /// Выбор стены пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <returns>Стена, или null, если операция отменена или не валидна</returns>
        private Element GetElement(in ExternalCommandData commandData)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;

            SelectionFilterElementsOfCategoryType filter = new SelectionFilterElementsOfCategoryType(CategoryType.Model);
            Element element = null;
            try
            {
                Reference elementRef = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    filter,
                    "Выберите элемент модели");
                element = doc.GetElement(elementRef);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбрать элементы модели",
                    "Ошибка");
                return null;
            }
            return element;
        }

        /// <summary>
        /// Determine whether an element is visible in a view, 
        /// by Colin Stark, described in
        /// http://stackoverflow.com/questions/44012630/determine-is-a-familyinstance-is-visible-in-a-view
        /// </summary>
        public static bool IsElementVisibleInView(
          View view,
          Element el)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (el == null)
            {
                throw new ArgumentNullException(nameof(el));
            }

            // Obtain the element's document.

            Document doc = el.Document;

            ElementId elId = el.Id;

            // Create a FilterRule that searches 
            // for an element matching the given Id.

            FilterRule idRule = ParameterFilterRuleFactory
              .CreateEqualsRule(
                new ElementId(BuiltInParameter.ID_PARAM),
                elId);

            var idFilter = new ElementParameterFilter(idRule);

            // Use an ElementCategoryFilter to speed up the 
            // search, as ElementParameterFilter is a slow filter.

            Category cat = el.Category;
            var catFilter = new ElementCategoryFilter(cat.Id);

            // Use the constructor of FilteredElementCollector 
            // that accepts a view id as a parameter to only 
            // search that view.
            // Also use the WhereElementIsNotElementType filter 
            // to eliminate element types.

            FilteredElementCollector collector =
                new FilteredElementCollector(doc, view.Id)
                  .WhereElementIsNotElementType()
                  .WherePasses(catFilter)
                  .WherePasses(idFilter);

            // If the collector contains any items, then 
            // we know that the element is visible in the
            // given view.

            return collector.Any();
        }
    }
}
