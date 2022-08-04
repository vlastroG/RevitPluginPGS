using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace MS.Utilites
{
    /// <summary>
    /// Фильтр выбора для плоских граней
    /// </summary>
    public class SelectionFilterPlanarFaces : ISelectionFilter
    {
        private Document _doc = null;

        public SelectionFilterPlanarFaces(Document doc)
        {
            _doc = doc;
        }


        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (_doc.GetElement(reference).GetGeometryObjectFromReference(reference) is PlanarFace)
            {
                // Only return true for planar faces. Non-planar faces will not be selectable
                return true;
            }
            return false;
        }
    }
}
