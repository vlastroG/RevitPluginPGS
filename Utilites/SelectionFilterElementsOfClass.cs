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
    public class SelectionFilterElementsOfCategory<T> : ISelectionFilter where T : Category
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category is T;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
