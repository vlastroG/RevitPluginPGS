using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.SelectionFilters
{
    /// <summary>
    /// Фильтр выбора элементов по типу категории
    /// </summary>
    public class SelectionFilterElementsOfCategoryType : ISelectionFilter
    {
        private readonly CategoryType _categoryType;

        /// <summary>
        /// Конструктор фильтра выбора элементов по типу категории элементов
        /// </summary>
        /// <param name="categoryType"></param>
        public SelectionFilterElementsOfCategoryType(CategoryType categoryType)
        {
            _categoryType = categoryType;
        }

        public bool AllowElement(Element elem)
        {
            if (elem is null)
                return false;
            if (elem.Category is null)
                return false;

            return elem.Category.CategoryType == _categoryType;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
