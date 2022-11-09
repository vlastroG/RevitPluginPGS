using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.GUI.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.General
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class SelectorSettingsCmd : IExternalCommand
    {
        /// <summary>
        /// Название категории по умолчанию
        /// </summary>
        private static string _category = "Помещения";

        /// <summary>
        /// Категория, заданная при выборе пользователем
        /// </summary>
        public static Category Category { get; private set; }

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
            Category = form.Category;
            if (Category == null)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}
