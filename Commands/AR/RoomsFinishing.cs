using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RoomsFinishing : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            List<Room> rooms = new List<Room>(
              sel.PickElementsByRectangle()
              .Cast<Room>());

            if (1 != rooms.Count)
            {
                message = "Please select exactly one room.";

                return Result.Failed;
            }

            View3D view3d
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));

            if (null == view3d)
            {
                message = "No 3D view named '{3D}' found.";

                return Result.Failed;
            }

            foreach (Room room in rooms)
            {
                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(
                    new SpatialElementBoundaryOptions());

                int n = loops.Count;

                //Debug.Print(
                //  "Room {0} has {1} loop{2}{3}",
                //  room.Name, n, PluralSuffix(n),
                //  DotOrColon(n));

                int i = 0;

                string testOut = "";
                StringBuilder sb = new StringBuilder();
                foreach (IList<BoundarySegment> loop in loops)
                {
                    n = loop.Count;

                    //Debug.Print(
                    //  "  Loop {0} has {1} segment{2}{3}",
                    //  i++, n, PluralSuffix(n),
                    //  DotOrColon(n));

                    int j = 0;


                    foreach (BoundarySegment seg in loop)
                    {
                        Element e = doc.GetElement(seg.ElementId);

                        string s = "Element property";

                        if (null == e)
                        {
                            s = "GetElementByRay";

                            e = GetElementByRay(uiapp, doc, view3d,
                              seg.GetCurve());
                        }

                        ElementId eTypeId = e.GetTypeId();

                        ElementType type = doc.GetElement(eTypeId) as ElementType;


                        try
                        {
                            sb.Append(type.LookupParameter("Имя типа").AsString());


                        }
                        catch (Exception)
                        {

                            throw;
                        }

                        //Debug.Print(
                        //  "    Segment {0}: {1} element {2} returned by {3}",
                        //  j++, CurveString(seg.Curve),
                        //  ElementDescription(e), s);

                    }
                }
                testOut = sb.ToString();
                Transaction trans = new Transaction(doc);
                trans.Start("PGS_RoomsFinishing");

                room.LookupParameter("Отделка потолка").Set(testOut);

                trans.Commit();
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// left from given input vector.
        /// </summary>
        public XYZ GetLeftDirection(XYZ direction)
        {
            double x = -direction.Y;
            double y = direction.X;
            double z = direction.Z;
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// right from given input vector.
        /// </summary>
        public XYZ GetRightDirection(XYZ direction)
        {
            return GetLeftDirection(direction.Negate());
        }

        /// <summary>
        /// Return the neighbouring BIM element generating 
        /// the given room boundary curve c, assuming it
        /// is oriented counter-clockwise around the room
        /// if part of an interior loop, and vice versa.
        /// </summary>
        public Element GetElementByRay(
          UIApplication app,
          Document doc,
          View3D view3d,
          Curve c)
        {
            Element boundaryElement = null;

            // Tolerances

            const double minTolerance = 0.00000001;
            const double maxTolerance = 0.01;

            // Height of ray above room level:
            // ray starts from one foot above room level

            const double elevation = 1;

            // Ray starts not directly from the room border
            // but from a point offset slightly into it.

            const double stepInRoom = 0.1;

            // We could use Line.Direction if Curve c is a 
            // Line, but since c also might be an Arc, we 
            // calculate direction like this:

            XYZ lineDirection
              = (c.GetEndPoint(1) - c.GetEndPoint(0))
                .Normalize();

            XYZ upDir = elevation * XYZ.BasisZ;

            // Assume that the room is on the left side of 
            // the room boundary curve and wall on the right.
            // This is valid for both outer and inner room 
            // boundaries (outer are counter-clockwise, inner 
            // are clockwise). Start point is slightly inside 
            // the room, one foot above room level.

            XYZ toRoomVec = stepInRoom * GetLeftDirection(
              lineDirection);

            XYZ pointBottomInRoom = c.Evaluate(0.5, true)
              + toRoomVec;

            XYZ startPoint = pointBottomInRoom + upDir;

            // We are searching for walls only

            ElementFilter wallFilter
              = new ElementCategoryFilter(
                BuiltInCategory.OST_Walls);

            ReferenceIntersector intersector
              = new ReferenceIntersector(wallFilter,
                FindReferenceTarget.Element, view3d);

            // We don't want to find elements in linked files

            intersector.FindReferencesInRevitLinks = false;

            XYZ toWallDir = GetRightDirection(
              lineDirection);

            ReferenceWithContext context = intersector
              .FindNearest(startPoint, toWallDir);

            Reference closestReference = null;

            if (context != null)
            {
                if ((context.Proximity > minTolerance)
                  && (context.Proximity < maxTolerance
                    + stepInRoom))
                {
                    closestReference = context.GetReference();

                    if (closestReference != null)
                    {
                        boundaryElement = doc.GetElement(
                          closestReference);
                    }
                }
            }
            return boundaryElement;
        }
    }
}
