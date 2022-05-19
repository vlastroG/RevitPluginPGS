using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MS.Utilites.WorkWithGeometry;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PipelineFittings : IExternalCommand
    {
        public List<FamilyInstance> GetElementsInTheRoom(Room room, Document doc)
        {
            List<FamilyInstance> elementsInTheRoom = new List<FamilyInstance>();
            SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
            SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(room); // compute the room geometry 
            Solid roomSolid = results.GetGeometry(); // get the solid representing the room's geometry

            elementsInTheRoom = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WherePasses(new ElementIntersectsSolidFilter(roomSolid)).Cast<FamilyInstance>().ToList();
            return elementsInTheRoom;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter_links = new FilteredElementCollector(doc);
            var linked_docs = filter_links
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach (var link in linked_docs)
            {

            }
        }

        public Result Execute1(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter_pipe_acs = new FilteredElementCollector(doc);
            var pipe_accessories = filter_pipe_acs
                .OfCategory(BuiltInCategory.OST_PipeAccessory)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(e => e as FamilyInstance);

            var filter_links = new FilteredElementCollector(doc);
            //IList<Element> links = filter_links
            //                       .OfCategory(BuiltInCategory.OST_RvtLinks)
            //                       .WhereElementIsNotElementType()
            //                       .ToElements(); 
            IList<Element> links = filter_links
                                   .OfCategory(BuiltInCategory.OST_Walls)
                                   .WhereElementIsNotElementType()
                                   .ToElements();

            foreach (var pipe_acs in pipe_accessories)
            {
                Solid pipe_acs_solid = SolidFromBoundingBox(pipe_acs.get_BoundingBox(doc.ActiveView));

                foreach (Element link in links)
                {
                    //var revit_link = link as RevitLinkInstance;

                    //Transform transform = revit_link.GetTransform();
                    //if (!transform.AlmostEqual(Transform.Identity))
                    //{
                    //    pipe_acs_solid = SolidUtils
                    //        .CreateTransformed(pipe_acs_solid, transform.Inverse);
                    //}
                    ElementIntersectsSolidFilter filter_intersects
                      = new ElementIntersectsSolidFilter(pipe_acs_solid);

                    //var linked_doc = revit_link.GetLinkDocument();
                    //var room_filter = new FilteredElementCollector(linked_doc);
                    //var room = room_filter
                    var walls = filter_links
                        .OfCategory(BuiltInCategory.OST_Walls)
                        .WhereElementIsNotElementType()
                        .WherePasses(filter_intersects)
                        .ToElements();

                }

            }



            //Solid solid = GetSolid(e);

            //foreach (RevitLinkInstance i in links)
            //{
            //    // GetTransform or GetTotalTransform or what?
            //    Transform transform = i.GetTransform();
            //    if (!transform.AlmostEqual(Transform.Identity))
            //    {
            //        solid = SolidUtils.CreateTransformed(
            //          solid, transform.Inverse);
            //    }
            //    ElementIntersectsSolidFilter filter_intersector
            //      = new ElementIntersectsSolidFilter(solid);

            //    FilteredElementCollector intersecting
            //      = new FilteredElementCollector(i.GetLinkDocument())
            //        .WherePasses(filter_intersector);
            //}

            //foreach (Element element in links)
            //{
            //    Document linked_doc = element.Document;
            //foreach (Element linked_doc_elem in linked_doc)
            //{
            //    if (linkedDoc.Title.Equals(linkType.Name))
            //    {
            //        FilteredElementCollector collLinked = new FilteredElementCollector(linkedDoc);
            //        IList<Element> linkedWalls = collLinked.OfClass(typeof(Wall)).WherePasses(filter).ToElements();
            //        if (linkedWalls.Count != 0)
            //        {
            //            foreach (Element eleWall in linkedWalls)
            //            {
            //                walls.Add(eleWall);
            //            }
            //        }
            //    }
            //}
            //}

            //var rooms = filter
            //    .OfCategory(BuiltInCategory.OST_Rooms)
            //    .WhereElementIsNotElementType()
            //    .ToElements()
            //    .ToList();




            //foreach (var pipe_acc in pipe_accessories)
            //{
            //    var t = pipe_acc;
            //}

            return Result.Succeeded;
        }
    }
}
