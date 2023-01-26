using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.SelectionFilters
{
    public class SelectionFilterMulticategoryInLink : ISelectionFilter
    {
        private Document _doc;

        private IEnumerable<BuiltInCategory> _categories;

        public SelectionFilterMulticategoryInLink(Document doc, IEnumerable<BuiltInCategory> categories)
        {
            _doc = doc;
            _categories = categories;
        }

        public Document LinkedDocument { get; private set; } = null;


        public bool LastCheckedWasFromLink
        {
            get { return null != LinkedDocument; }

        }
        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            LinkedDocument = null;

            Element element = _doc.GetElement(reference);

            if (element is RevitLinkInstance)
            {
                RevitLinkInstance link = element as RevitLinkInstance;

                LinkedDocument = link.GetLinkDocument();

                element = LinkedDocument.GetElement(reference.LinkedElementId);
            }
            return _categories.Contains((BuiltInCategory)element.Category.Id.IntegerValue);
        }
    }
}
