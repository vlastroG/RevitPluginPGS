using Autodesk.Revit.DB;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.KR.Services.SelectionFilters
{
    public class SelectionFilterStairStepEdges : SelectionFilterEdgesOfElement
    {
        public SelectionFilterStairStepEdges(in Document doc, int elementId) : base(doc, elementId)
        {
        }

        /// <summary>
        /// Разрешить выбор только горизонтальных ребер
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public override bool AllowReference(Reference reference, XYZ position)
        {
            if (_doc.GetElement(reference).GetGeometryObjectFromReference(reference) is Edge edge)
            {
                return (edge.AsCurve() as Line).Direction.Normalize().Z == 0;
            };
            return false;
        }
    }
}
