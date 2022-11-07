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
    /// Фильтр выбора граней заданного элемента
    /// </summary>
    public class SelectionFilterPlanarFacesOfElement : ISelectionFilter
    {
        private Document _doc = null;

        private int _elementId = -1;

        /// <summary>
        /// Конструктор фильтра по документу и Id элемента, грани которого разрешены для выбора
        /// </summary>
        /// <param name="doc">Документ, в котором происзодит выбор элементов</param>
        /// <param name="elementId">Id элемента, грани которого разрешены для выбора</param>
        public SelectionFilterPlanarFacesOfElement(Document doc, int elementId)
        {
            _doc = doc;
            _elementId = elementId;
        }


        public bool AllowElement(Element elem)
        {
            return elem.Id.IntegerValue == _elementId;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return _doc.GetElement(reference).GetGeometryObjectFromReference(reference) is PlanarFace;
        }
    }
}
