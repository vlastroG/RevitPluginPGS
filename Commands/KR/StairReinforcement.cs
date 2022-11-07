using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.KR.Services;
using MS.Commands.KR.Services.SelectionFilters;
using MS.Utilites;
using MS.Utilites.Extensions;
using MS.Utilites.SelectionFilters;
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

            try
            {
                (Curve curve, PlanarFace planarFace, Element elem) = GetCurveAndFaceFromUser(doc, uidoc);
                if (curve is null || planarFace is null || elem is null)
                {
                    return Result.Cancelled;
                }
                BarsCreation.CreateStairStepBarsFrame(
                    elem,
                    curve,
                    planarFace,
                    6,
                    25,
                    100,
                    200,
                    XYZ.BasisZ.Negate());
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Возвращает кортеж линии ступени и наклонной плоскости марша, выбранные пользователем
        /// с учетом трансформирования, если это модель в контексте
        /// </summary>
        /// <param name="doc">Документ, в котором выбираются элементы</param>
        /// <param name="uidoc">Интерфейс документа, в котором выбираются элементы</param>
        /// <returns></returns>
        private static (Curve curve, PlanarFace planarFacem, Element elem) GetCurveAndFaceFromUser(
            in Document doc,
            in UIDocument uidoc)
        {
            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_Stairs,
                    BuiltInCategory.OST_GenericModel,
                    BuiltInCategory.OST_StructuralFraming
                },
                false);
            // Пользователь лестницу
            Element elem = doc.GetElement(uidoc.Selection
                .PickObject(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    filter,
                    "Выберите лестницу, или нажмите Esc для отмены"));

            Reference edgeRef = uidoc.Selection.PickObject(
                ObjectType.Edge,
                new SelectionFilterStairStepEdges(doc, elem.Id.IntegerValue),
                "Выберите ребро ступени лестницы, или нажмите Esc для отмены");
            GeometryObject geoObjectEdge = doc.GetElement(edgeRef).GetGeometryObjectFromReference(edgeRef);
            Edge edge = geoObjectEdge as Edge;

            Reference faceRef = uidoc.Selection.PickObject(
                ObjectType.Face,
                new SelectionFilterStairAnglePlane(doc, elem.Id.IntegerValue),
                "Выберите наклонную грань лестничного марша, или нажмите Esc для отмены");
            GeometryObject geoObject = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
            PlanarFace planarFace = geoObject as PlanarFace;

            Curve curveTrans = GetSelectedEdgeTransformed(elem, edge);
            PlanarFace planarFaceTrans = GetSelectedPlaneTransformed(elem, planarFace);

            return (curveTrans, planarFaceTrans, elem);
        }

        /// <summary>
        /// Возвращает линию, с учетом трансформации элемента, эсли он - модель в контексте
        /// </summary>
        /// <param name="element">Элемент, которрому принадлежит ребро</param>
        /// <param name="edge">Ребро для получения линии</param>
        /// <returns>Линия ребра с учетом трансформации элемента</returns>
        private static Curve GetSelectedEdgeTransformed(in Element element, in Edge edge)
        {
            Transform trans = WorkWithGeometry.GetPointAndRotation(element);
            return edge.AsCurve().CreateTransformed(trans);
        }

        /// <summary>
        /// Возвращает плоскость, выбранную пользователем, с учетом перемещения,
        /// если плоскость принадлежит элементу - модели в контексте
        /// </summary>
        /// <param name="element">Элемент, которому принадлежит плоскость</param>
        /// <returns>Плоская поверхность, с учетом трансформации</returns>
        private static PlanarFace GetSelectedPlaneTransformed(in Element element, PlanarFace planarFace)
        {
            Transform trans = WorkWithGeometry.GetPointAndRotation(element);
            Solid elementSolid = null;
            GeometryElement geomElem = element.get_Geometry(new Options()
            {
                ComputeReferences = true
            });
            List<PlanarFace> faceList = new List<PlanarFace>();
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = SolidUtils.CreateTransformed((Solid)geomObj, trans);
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        elementSolid = solid;
                        faceList.AddRange(solid.Faces.GetPlanarFaces());
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetSymbolGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = SolidUtils.CreateTransformed((Solid)instGeomObj, trans);
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                elementSolid = solid;
                                faceList.AddRange(solid.Faces.GetPlanarFaces());
                            }
                        }
                    }
                }
            }

            if (faceList.Count == 0) return null;
            foreach (var face in faceList)
            {
                if (face.Id == planarFace.Id)
                {
                    return face;
                }
            }
            return null;
        }
    }
}
