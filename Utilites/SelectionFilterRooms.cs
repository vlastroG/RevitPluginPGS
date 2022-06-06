using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора элементов по заданной встроенной категории
    /// </summary>
    internal class SelectionFilterRooms : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Room;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }

    }
}
