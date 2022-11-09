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
    /// Фильтр выбора ребер, которые образованы геометрией заданного элемента
    /// </summary>
    public class SelectionFilterEdgesOfElement : ISelectionFilter
    {
        private protected Document _doc = null;

        private protected int _elementId = -1;

        /// <summary>
        /// Конструктор выбора ребер по документу и Id элемента
        /// </summary>
        /// <param name="doc">Документ, в котором происходит выбор ребер</param>
        /// <param name="elementId">Id элемента, ребра которого разрешены для выбора</param>
        public SelectionFilterEdgesOfElement(in Document doc, int elementId)
        {
            _doc = doc;
            _elementId = elementId;
        }


        public bool AllowElement(Element elem)
        {
            return elem.Id.IntegerValue == _elementId;
        }

        public virtual bool AllowReference(Reference reference, XYZ position)
        {
            return _doc.GetElement(reference).GetGeometryObjectFromReference(reference) is Edge;
        }
    }
}
