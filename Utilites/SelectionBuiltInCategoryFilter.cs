using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора элементов по заданной встроенной категории
    /// </summary>
    internal class SelectionBuiltInCategoryFilter : ISelectionFilter
    {
        /// <summary>
        /// Категория для выбора элементов
        /// </summary>
        private readonly BuiltInCategory _category = BuiltInCategory.OST_Rooms;


        /// <summary>
        /// Конструктор фильтра выбора экземпляров семейств заданной категории
        /// </summary>
        /// <param name="category">Заданная встроенная категория</param>
        public SelectionBuiltInCategoryFilter(BuiltInCategory category)
        {
            _category = category;
        }


        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance instance && instance.Category.Id == new ElementId(_category);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

    }
}
