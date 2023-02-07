using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.SelectionFilters
{
    public class SelectionFilterElementInLink<T> : ISelectionFilter where T : Element
    {
        private Document _doc;

        public SelectionFilterElementInLink(Document doc)
        {
            _doc = doc;
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
            return element is T;
        }
    }
}
