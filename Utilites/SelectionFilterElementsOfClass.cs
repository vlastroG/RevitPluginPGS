using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора элементов по заданной категории Revit API
    /// </summary>
    /// <typeparam name="T">Заданная категория Revit</typeparam>
    public class SelectionFilterElementsOfCategory<TElementClass> : ISelectionFilter where TElementClass : Element
    {
        private BuiltInCategory _builtInCategory;

        private bool _addFilterByElementClass;


        public SelectionFilterElementsOfCategory(BuiltInCategory BuiltInCategory, bool AddFilterByElementClass)
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

            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

            if (_addFilterByElementClass)
            {
                if (builtInCategory == _builtInCategory || elem is TElementClass)
                    return true;
            }
            else
            {
                if (builtInCategory == _builtInCategory)
                    return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

        private int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id.IntegerValue ?? 0;
        }
    }
}
