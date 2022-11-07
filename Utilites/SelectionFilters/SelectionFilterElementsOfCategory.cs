using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора элементов по заданной категории Revit API
    /// </summary>
    /// <typeparam name="T">Заданная категория Revit</typeparam>
    public class SelectionFilterElementsOfCategory<TElementClass> : ISelectionFilter where TElementClass : Element
    {
        private List<BuiltInCategory> _builtInCategory;

        private bool _addFilterByElementClass;


        public SelectionFilterElementsOfCategory(List<BuiltInCategory> BuiltInCategory, bool AddFilterByElementClass)
        {
            _builtInCategory = BuiltInCategory;
            _addFilterByElementClass = AddFilterByElementClass;
        }


        public bool AllowElement(Element elem)
        {
            if (elem is null)
                return false;

            //if (!(elem is FamilyInstance)) 
            //    return false;

            BuiltInCategory builtInCategory = (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(elem);

            if (_addFilterByElementClass)
            {
                if (_builtInCategory.Contains(builtInCategory) || elem is TElementClass)
                    return true;
            }
            else
            {
                if (_builtInCategory.Contains(builtInCategory))
                    return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
