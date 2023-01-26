using Autodesk.Revit.DB;
using MS.Utilites;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.KR.Services.SelectionFilters
{
    public class SelectionFilterStairAnglePlane : SelectionFilterPlanarFacesOfElement
    {
        public SelectionFilterStairAnglePlane(Document doc, int elementId) : base(doc, elementId)
        {
        }

        /// <summary>
        /// Разрешен выбор только наклонных граней
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool AllowReference(Reference reference, XYZ position)
        {
            return _doc.GetElement(reference).GetGeometryObjectFromReference(reference) is PlanarFace pFace
                && pFace.IsAngle();
        }
    }
}
