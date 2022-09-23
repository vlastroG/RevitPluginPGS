using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class CeilingExtension
    {
        /// <summary>
        /// Возвращает точку, которая должна принадлежать помещению под потолком
        /// </summary>
        /// <param name="ceiling">Потолок</param>
        /// <returns>Точка, принадлежащая помещению под потолком, или null в случае неправильной геометрии потолка</returns>
        public static XYZ GetRoomPoint(this Ceiling ceiling)
        {
            Solid ceilingSolid = null;
            GeometryElement geomElem = ceiling.get_Geometry(new Options());
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        ceilingSolid = solid;
                        break;
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                ceilingSolid = solid;
                                break;
                            }
                        }
                    }
                }
            }

            if (ceilingSolid == null) return null;
            var faces = ceilingSolid.Faces;
            XYZ roomPoint;
            foreach (var face in faces)
            {
                PlanarFace planarFace;
                if (face is PlanarFace face1)
                {
                    planarFace = face1;
                    XYZ normal = planarFace.FaceNormal;
                    if (Math.Round(normal.Z, 5) == 0)
                    {
                        roomPoint = planarFace.Origin + normal.Negate() + XYZ.BasisZ.Negate();
                        return roomPoint;
                    }
                }
            }
            return null;
        }
    }
}
