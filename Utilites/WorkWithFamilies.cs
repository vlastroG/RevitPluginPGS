using Autodesk.Revit.DB;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Utilites
{
    public static class WorkWithFamilies
    {
        public static string GetSymbolDescription(FamilyInstance lintel)
        {
            return lintel.Symbol
                .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                .AsValueString();
        }

        public static List<string> GetSubComponentsAdskNames(FamilyInstance lintel)
        {
            Document doc = lintel.Document;
            var subComponentsIds = lintel.GetSubComponentIds();
            var facingNormal = lintel.FacingOrientation.Normalize();
            List<string> adskNames = new List<string>();
            foreach (var subCompId in subComponentsIds)
            {
                var elem = doc.GetElement(subCompId);
                if (elem != null)
                {
                    var elemFacing = (elem as FamilyInstance).FacingOrientation.Normalize();
                    bool isElemAlong = facingNormal.IsAlmostEqualTo(elemFacing);
                    if (isElemAlong && elem.get_Parameter(SharedParams.ADSK_Name) != null)
                    {
                        var adskName = elem.get_Parameter(SharedParams.ADSK_Name).AsValueString();
                        if (!String.IsNullOrEmpty(adskName))
                        {
                            adskNames.Add(adskName);
                        };
                    }
                }
            }
            return adskNames;
        }

        private static bool ValidateLintel(FamilyInstance lintel)
        {
            if (((BuiltInCategory)lintel.Category.Id.IntegerValue == BuiltInCategory.OST_GenericModel)
                && (GetSymbolDescription(lintel) == SharedValues.LintelDescription))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static List<FamilyInstance> GetSubComponentsAlong(FamilyInstance lintel)
        {
            List<FamilyInstance> result = new List<FamilyInstance>();
            Document doc = lintel.Document;
            var subComponentsIds = lintel.GetSubComponentIds();
            var facingNormal = lintel.FacingOrientation.Normalize();
            foreach (var subCompId in subComponentsIds)
            {
                var elem = doc.GetElement(subCompId);
                if (elem != null)
                {
                    var famInst = elem as FamilyInstance;
                    var elemFacing = famInst.FacingOrientation.Normalize();
                    bool isElemAlong = facingNormal.IsAlmostEqualTo(elemFacing);
                    if (isElemAlong)
                    {
                        result.Add(famInst);
                    }
                }
            }
            return result;
        }

        public static double GetMaxWidthOfLintel(FamilyInstance lintel)
        {
            XYZ origin = (lintel.Location as LocationPoint).Point;
            XYZ normal = lintel.FacingOrientation.Normalize();
            Plane centerPlane = Plane.CreateByNormalAndOrigin(normal, origin);
            SortedList<double, XYZ> distanceFamiliesPairs = new SortedList<double, XYZ>();
            List<double> distances = new List<double>();

            var subComps = GetSubComponentsAlong(lintel);
            foreach (var subEl in subComps)
            {
                XYZ subElPoint = (subEl.Location as LocationPoint).Point;
                var distance = WorkWithGeometry.SignedDistanceTo(centerPlane, subElPoint);
                if (distanceFamiliesPairs.ContainsKey(distance))
                {
                    continue;
                }
                else
                {
                    distanceFamiliesPairs.Add(distance, subElPoint);
                    distances.Add(distance);
                }
            }
            var maxPoint = distanceFamiliesPairs[distances.Max()];
            var minPoint = distanceFamiliesPairs[distances.Min()];
            var lengthVector = maxPoint - minPoint;
            var widthOfLintel = Math.Abs(normal.DotProduct(lengthVector));
            return widthOfLintel;
        }


        public static double GetWallWidth(FamilyInstance lintel)
        {
            double wallWidth = 0;
            if (lintel.get_Parameter(SharedParams.ADSK_ThicknessOfWall) != null)
            {
                wallWidth = lintel.get_Parameter(SharedParams.ADSK_ThicknessOfWall).AsDouble();
            }
            else if (lintel.SuperComponent != null && lintel.SuperComponent.get_Parameter(SharedParams.ADSK_ThicknessOfWall) != null)
            {
                wallWidth = lintel.SuperComponent.get_Parameter(SharedParams.ADSK_ThicknessOfWall).AsDouble();
            }

            return wallWidth;
        }
    }
}
