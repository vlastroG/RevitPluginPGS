using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StairReinforcement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Reference faceRef = uidoc.Selection.PickObject(ObjectType.Face, new SelectionFilterPlanarFaces(doc), "Please pick a planar face to set the work plane. ESC for cancel.");
            //GeometryObject geoObject = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
            //PlanarFace planarFace = geoObject as PlanarFace;

            Reference edgeRef = uidoc.Selection.PickObject(ObjectType.Edge, new SelectionFilterEdges(doc), "Please pick an edge to set the work plane. ESC for cancel.");
            GeometryObject geoObjectEdge = doc.GetElement(edgeRef).GetGeometryObjectFromReference(edgeRef);
            Edge edge = geoObjectEdge as Edge;

            //var bounds = planarFace.EdgeLoops.get_Item(0);

            //foreach (Edge edge in bounds)
            //{
            //    var curve = edge.AsCurve();
            //    var origin = curve.Evaluate(0.5, true);
            //    var start = curve.GetEndPoint(0);
            //    var end = curve.GetEndPoint(1);
            //    var direction = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();

            //}


            return Result.Succeeded;
        }
    }
}
