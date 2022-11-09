using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class RoomExtension
    {
        public static IList<CurveLoop> GetCurveLoops(
            this Room room,
            in SpatialElementBoundaryOptions spatialElementBoundaryOptions)
        {
            IList<CurveLoop> curveLoops = new List<CurveLoop>();

            IList<IList<BoundarySegment>> loops = room.GetBoundarySegments(spatialElementBoundaryOptions);

            foreach (IList<BoundarySegment> loop in loops)
            {
                CurveLoop curveLoop = new CurveLoop();
                for (int i = 0; i < loop.Count; i++)
                {
                    curveLoop.Append(loop[i].GetCurve());
                }
                curveLoops.Add(curveLoop);
            }
            return curveLoops;
        }
    }
}
