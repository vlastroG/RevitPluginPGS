using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using static MS.Utilites.WorkWithGeometry;
using BoundarySegment = Autodesk.Revit.DB.BoundarySegment;

namespace MS.Utilites
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetBoundarySegmentElement : IExternalCommand
    {
        /// <summary>
        /// Return an English plural suffix for the given
        /// number of items, i.e. 's' for zero or more
        /// than one, and nothing for exactly one.
        /// </summary>
        public static string PluralSuffix(int n)
        {
            return 1 == n ? "" : "s";
        }

        /// <summary>
        /// Return a dot (full stop) for zero
        /// or a colon for more than zero.
        /// </summary>
        public static string DotOrColon(int n)
        {
            return 0 < n ? ":" : ".";
        }

        /// <summary>
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        /// <summary>
        /// Return a string for an XYZ point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// Return a string representing the data of a
        /// curve. Currently includes detailed data of
        /// line and arc elements only.
        /// </summary>
        public static string CurveString(Curve c)
        {
            string s = c.GetType().Name.ToLower();

            XYZ p = c.GetEndPoint(0);
            XYZ q = c.GetEndPoint(1);

            s += string.Format(" {0} --> {1}",
              PointString(p), PointString(q));

            // To list intermediate points or draw an
            // approximation using straight line segments,
            // we can access the curve tesselation, cf.
            // CurveTessellateString:

            //foreach( XYZ r in lc.Curve.Tessellate() )
            //{
            //}

            // List arc data:

            Arc arc = c as Arc;

            if (null != arc)
            {
                s += string.Format(" center {0} radius {1}",
                  PointString(arc.Center), arc.Radius);
            }

            // Todo: add support for other curve types
            // besides line and arc.

            return s;
        }

        /// <summary>
        /// Return a string describing the given element:
        /// .NET type name,
        /// category name,
        /// family and symbol name for a family instance,
        /// element id and element name.
        /// </summary>
        public static string ElementDescription(
          Element e)
        {
            if (null == e)
            {
                return "<null>";
            }

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            FamilyInstance fi = e as FamilyInstance;

            string typeName = e.GetType().Name;

            string categoryName = (null == e.Category)
              ? string.Empty
              : e.Category.Name + " ";

            string familyName = (null == fi)
              ? string.Empty
              : fi.Symbol.Family.Name + " ";

            string symbolName = (null == fi
              || e.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : fi.Symbol.Name + " ";

            return string.Format("{0} {1}{2}{3}<{4} {5}>",
              typeName, categoryName, familyName,
              symbolName, e.Id.IntegerValue, e.Name);
        }


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

            var filter_rooms = new SelectionFilterRooms();
            var rooms = uidoc.Selection.PickElementsByRectangle(filter_rooms, "Выберите 1 помещение.").Select(e => e as Room).ToList();

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
                SpatialElementBoundaryOptions opt
                  = new SpatialElementBoundaryOptions
                  {
                      SpatialElementBoundaryLocation
                  = SpatialElementBoundaryLocation.Finish
                  };

                IList<IList<BoundarySegment>> loops
                  = room.GetBoundarySegments(opt);

                int n = loops.Count;

                foreach (IList<BoundarySegment> loop in loops)
                {
                    n = loop.Count;

                    foreach (BoundarySegment seg in loop)
                    {
                        Element e = doc.GetElement(seg.ElementId);

                        if (null == e)
                        {
                            e = GetElementByRay(doc, view3d,
                              seg.GetCurve());
                        }
                    }
                }
            }
            return Result.Succeeded;
        }
    }
}
