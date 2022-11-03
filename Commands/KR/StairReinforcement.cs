using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.KR.Services;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StairReinforcement : IExternalCommand
    {
        /// <summary>
        /// Армирование лестничного марша
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            Reference edgeRef = uidoc.Selection.PickObject(ObjectType.Edge, new SelectionFilterEdges(doc), "Выберите ребро ступени лестницы");
            GeometryObject geoObjectEdge = doc.GetElement(edgeRef).GetGeometryObjectFromReference(edgeRef);
            Edge edge = geoObjectEdge as Edge;

            Element elem = doc.GetElement(edgeRef);

            BarsCreation.CreateStairStepBarsFrame(
                elem,
                edge.AsCurve(),
                6,
                25,
                100,
                200,
                XYZ.BasisZ.Negate());

            return Result.Succeeded;
        }
    }
}
