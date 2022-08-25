using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора для ребер граней
    /// </summary>
    public class SelectionFilterEdges : ISelectionFilter
    {
        private Document _doc = null;

        public SelectionFilterEdges(Document doc)
        {
            _doc = doc;
        }


        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (_doc.GetElement(reference).GetGeometryObjectFromReference(reference) is Edge)
            {
                // Only return true for edges. Non-edges will not be selectable
                return true;
            }
            return false;
        }
    }
}
