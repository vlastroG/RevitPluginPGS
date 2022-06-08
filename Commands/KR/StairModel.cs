using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR
{
    internal class StairModel
    {
        public Solid StairSolid;

        private readonly List<Solid> _stairSolids = new List<Solid>();

        private static readonly Options _options = new Options() { DetailLevel = ViewDetailLevel.Coarse };

        public StairModel(Element element)
        {
            // Валидация входного элемента
            if (!(element is Stairs)
                && (BuiltInCategory)WorkWithParameters.GetCategoryIdAsInteger(element)
                != BuiltInCategory.OST_Stairs)
            {
                throw new ArgumentException(nameof(element));
            }

            GeometryElement geoElement = element.get_Geometry(_options);

            foreach (GeometryObject geoObject in geoElement)
            {
                GeometryInstance geoInstance = geoObject as GeometryInstance;
                if (geoInstance != null)
                {
                    GeometryElement instanceGeometryElement = geoInstance.GetInstanceGeometry();
                    List<Solid> allProtoGeoSolids = new List<Solid>();
                    foreach (GeometryObject protoGeoObject in instanceGeometryElement)
                    {

                        Solid solid = protoGeoObject as Solid;
                        if (solid != null && solid.Volume > 0)
                        {
                            allProtoGeoSolids.Add(solid);
                        }
                    }
                    allProtoGeoSolids.Sort((s1, s2) => s1.Volume.CompareTo(s2.Volume));

                    if (allProtoGeoSolids.Count == 1)
                    {
                        _stairSolids.Add(allProtoGeoSolids.First());
                    }
                    else if (allProtoGeoSolids.Count >= 2)
                    {
                        _stairSolids.AddRange(allProtoGeoSolids.GetRange(0, 2));
                    }
                }
            }

            var t = 0;

        }
    }
}
