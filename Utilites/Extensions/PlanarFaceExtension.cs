using Autodesk.Revit.DB;
using System;

namespace MS.Utilites
{
    public static class PlanarFaceExtension
    {
        /// <summary>
        /// Возвращает самый длинный контур грани.
        /// </summary>
        /// <param name="face">Плоская грань.</param>
        /// <returns>Самый длинный контур грани.</returns>
        public static EdgeArray GetMaxLengthEdgeArray(this PlanarFace face)
        {
            EdgeArrayArray edgeLoopArray = face.EdgeLoops;
            double loopMaxLength = 0;
            EdgeArray outerEdgeArray = new EdgeArray();
            foreach (EdgeArray loop in edgeLoopArray)
            {
                double loopLength = 0;
                foreach (Edge edge in loop)
                {
                    loopLength += edge.AsCurve().Length;
                }
                if (loopLength > loopMaxLength)
                {
                    loopMaxLength = loopLength;
                    outerEdgeArray = loop;
                }
            }
            return outerEdgeArray;
        }

        /// <summary>
        /// return signed distance of point from the plane. 
        /// If the point is above the plane (in positive side of the plane) 
        /// the result is positive and if the point
        /// is below the plane the result is negetive
        /// </summary>
        /// <param name="planarFace">Input plane</param>
        /// <param name="point">Point to measure the distance from</param>
        /// <param name="projection">Projection of input point on this plane</param>
        /// <returns></returns>
        public static double DistanceTo(this PlanarFace planarFace, XYZ point, out XYZ projection)
        {
            //build a transformation matrix based on the plane coordinate system
            Transform tr = Transform.CreateTranslation(planarFace.Origin);
            tr.BasisX = (planarFace.GetSurface() as Plane).XVec.Normalize();
            tr.BasisY = (planarFace.GetSurface() as Plane).YVec.Normalize();
            tr.BasisZ = (planarFace.GetSurface() as Plane).Normal.Normalize();
            //Find the local coordinates of the given point with resepct to plabe coordinate system
            XYZ local = tr.Inverse.OfPoint(point);
            //project the local point to the XY plane (Z=0)
            projection = new XYZ(local.X, local.Y, 0);
            // transform the projection back to the global coordSYS
            projection = tr.OfPoint(projection);
            //The Z coordinate of the local point is the signed distance from the plane 
            return local.Z;
        }

        /// <summary>
        /// Определяет, является ли плоская поверхность наклонной или нет
        /// </summary>
        /// <param name="planarFace">Плоская поверхность</param>
        /// <returns>True, если поверхность наклонена, иначе false</returns>
        public static bool IsAngle(this PlanarFace planarFace)
        {
            XYZ normal = planarFace.ComputeNormal(new UV()).Normalize();
            var X = Math.Round(normal.X, 6);
            var Y = Math.Round(normal.Y, 6);
            var Z = Math.Round(normal.Z, 6);
            return Z != 0 && (Y != 0 || X != 0);
        }
    }
}
