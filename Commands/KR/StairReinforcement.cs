using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.Commands.KR.Services;
using MS.Commands.KR.Services.SelectionFilters;
using MS.GUI.KR;
using MS.GUI.ViewModels.KR;
using MS.Utilites;
using MS.Utilites.Extensions;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StairReinforcement : IExternalCommand
    {
        /// <summary>
        /// Настройки команды
        /// </summary>
        private StairReinforcementViewModel _settings = new StairReinforcementViewModel();


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
            string docPath = doc.PathName;
            var rebarTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .WhereElementIsElementType()
                .Cast<RebarBarType>()
                .OrderBy(r => r.BarNominalDiameter);
            var rebarTypesCount = rebarTypes.Count();
            if (rebarTypesCount == 0)
            {
                MessageBox.Show(
                    "В проект не загружен ни один тип арматурного стержня, загрузите их и повторите команду.",
                    "Ошибка");
                return Result.Cancelled;
            }

            if (!_settings.DocPath.Equals(docPath)
                || (StairReinforcementViewModel.RebarTypesMain.Count != rebarTypesCount))
            {
                _settings = new StairReinforcementViewModel(rebarTypes, docPath);
            }

            try
            {
                (List<Curve> curves, Edge edge, PlanarFace planarFace, Element elem) = GetCurveAndFaceFromUser(uidoc);
                if (curves is null || curves.Count == 0 || planarFace is null || elem is null)
                {
                    return Result.Cancelled;
                }

                var ui = new StairReinforcementView();
                ui.ShowDialog();
                if (ui.DialogResult != true)
                {
                    return Result.Cancelled;
                }

                if (_settings.SelectedRebarTypeMain is null
                    || (_settings.CreateStepFrames && (_settings.SelectedRebarTypeSteps is null)))
                {
                    return Result.Cancelled;
                }

                BarsCreation.CreateStairReinforcement(
                    elem,
                    curves,
                    planarFace,
                    edge,
                    _settings.CreateStepFrames,
                    _settings.SelectedRebarTypeSteps,
                    _settings.SelectedRebarTypeMain,
                    _settings.RebarCoverEnd,
                    _settings.RebarCoverSteps,
                    _settings.RebarCoverMainAngle,
                    _settings.RebarCoverMainHoriz,
                    _settings.BarsStepStepsHorizont,
                    _settings.BarsStepStepsVert,
                    _settings.BarsStepMainHorizont,
                    _settings.BarsStepMainAngle);
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
        /// <param name="uidoc">Интерфейс документа, в котором выбираются элементы</param>
        /// <returns></returns>
        private static (List<Curve> curves, Edge edge, PlanarFace planarFacem, Element elem) GetCurveAndFaceFromUser(
            in UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            var filter = new SelectionFilterElementsOfCategory<Element>(
                new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_Stairs,
                    BuiltInCategory.OST_GenericModel,
                    BuiltInCategory.OST_StructuralFraming
                },
                false);
            try
            {
                // Пользователь выбирает лестницу
                Element elem = doc.GetElement(uidoc.Selection
                    .PickObject(
                        Autodesk.Revit.UI.Selection.ObjectType.Element,
                        filter,
                        "Выберите лестницу, или нажмите Esc для отмены"));

                var edges = uidoc.Selection.PickObjects(
                    ObjectType.Edge,
                    new SelectionFilterStairStepEdges(doc, elem.Id.IntegerValue),
                    "Выберите ребра ступени лестницы и нажмите Готово, или нажмите Отмена")
                    .Select(r => doc.GetElement(r).GetGeometryObjectFromReference(r) as Edge);

                List<Curve> curves = edges
                    .Select(edg => GetSelectedEdgeTransformed(elem, edg))
                    .ToList();

                var edge = edges.Last();

                Reference faceRef = uidoc.Selection.PickObject(
                    ObjectType.Face,
                    new SelectionFilterStairAnglePlane(doc, elem.Id.IntegerValue),
                    "Выберите наклонную грань лестничного марша, или нажмите Esc для отмены");
                GeometryObject geoObject = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                PlanarFace planarFace = geoObject as PlanarFace;

                PlanarFace planarFaceTrans = GetSelectedPlaneTransformed(elem, planarFace);

                return (curves, edge, planarFaceTrans, elem);
            }
            catch (System.InvalidOperationException)
            {
                return (null, null, null, null);
            }
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
