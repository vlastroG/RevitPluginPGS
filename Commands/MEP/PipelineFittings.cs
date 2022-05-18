using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PipelineFittings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var filter = new FilteredElementCollector(doc);

            IList<Element> links = filter
                                   .OfCategory(BuiltInCategory.OST_RvtLinks)
                                   .WhereElementIsNotElementType()
                                   .ToElements();

            foreach (Element link in links)
            {
                var revit_link = link as RevitLinkInstance;
                var linked_doc = revit_link.GetLinkDocument();
                var room_filter = new FilteredElementCollector(linked_doc);
                var linked_rooms = room_filter
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .WhereElementIsNotElementType()
                    .ToElements();

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

            //var pipe_accessories = filter
            //    .OfCategory(BuiltInCategory.OST_PipeAccessory)
            //    .WhereElementIsNotElementType()
            //    .ToElements()
            //    .Select(e => e as FamilyInstance);


            //foreach (var pipe_acc in pipe_accessories)
            //{
            //    var t = pipe_acc;
            //}

            return Result.Succeeded;
        }
    }
}
