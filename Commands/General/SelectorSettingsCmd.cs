using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.GUI.General;
using MS.GUI.ViewModels.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.General
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectorSettingsCmd : IExternalCommand
    {
        private static SelectorViewModel _settings = new SelectorViewModel();

        /// <summary>
        /// Вывести окно для выбора категории пользователем
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            string docPath = doc.PathName;

            var categories = doc.Settings.Categories;
            List<Category> categoriesList = new List<Category>();
            foreach (Category category in categories)
            {
                categoriesList.Add(category);
            }
            categoriesList.Sort((x, y) => x.Name.CompareTo(y.Name));

            _settings = new SelectorViewModel(categoriesList, docPath);


            var form = new CategoryInput();
            form.ShowDialog();
            if (form.DialogResult != true || _settings.SelectedCategory is null)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
