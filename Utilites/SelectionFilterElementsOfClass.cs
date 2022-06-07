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
    /// Фильтр выбора элементов по заданному классу Revit API
    /// </summary>
    /// <typeparam name="T">Заданный класс Revit</typeparam>
    public class SelectionFilterElementsOfClass<T> : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is T;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
