using Autodesk.Revit.DB;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Utilites
{
    public static class FamiliesMethods
    {
        public static string GetSymbolDescription(in FamilyInstance lintel)
        {
            return lintel.Symbol
                .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                .AsValueString();
        }

        public static List<string> GetSubComponentsAdskNames(in FamilyInstance lintel)
        {
            var subComponents = GetSubComponentsAlong(lintel);
            List<string> adskNames = new List<string>();
            foreach (var subComp in subComponents)
            {
                if (subComp.get_Parameter(SharedParams.ADSK_Name) != null)
                {
                    var adskName = subComp.get_Parameter(SharedParams.ADSK_Name).AsValueString();
                    if (!String.IsNullOrEmpty(adskName))
                    {
                        adskNames.Add(adskName);
                    };
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

        private static List<FamilyInstance> GetSubComponentsAlong(in FamilyInstance lintel)
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
                    bool isElemAlong = facingNormal.IsAlmostEqualTo(elemFacing)
                        || facingNormal.IsAlmostEqualTo(elemFacing.Negate());
                    if (isElemAlong)
                    {
                        result.Add(famInst);
                    }
                }
            }
            return result;
        }

        public static double GetMaxWidthOfLintel(in FamilyInstance lintel)
        {
            XYZ origin = (lintel.Location as LocationPoint).Point;
            XYZ normal = lintel.FacingOrientation.Normalize();
            Plane centerPlane = Plane.CreateByNormalAndOrigin(normal, origin);
            SortedList<double, XYZ> distanceFamiliesPairs = new SortedList<double, XYZ>();
            List<double> distances = new List<double>();

            var subComps = GetSubComponentsAlong(lintel);
            if (subComps.Count == 0)
            {
                return 0;
            }
            foreach (var subEl in subComps)
            {
                XYZ subElPoint = (subEl.Location as LocationPoint).Point;
                var distance = GeometryMethods.SignedDistanceTo(centerPlane, subElPoint);
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

        /// <summary>
        /// Получение ADSK_Толщина стены
        /// </summary>
        /// <param name="lintel"></param>
        /// <returns></returns>
        public static double GetWallWidth(in FamilyInstance lintel)
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

        /// <summary>
        /// Получение уникального названия перемычки, зависящее только от конфигурации поперечного сечения перемычки.
        /// </summary>
        /// <param name="lintel">Перемычка</param>
        /// <param name="addLevel">Считать одинаковые перемычки на разных уровнях разными: True/False</param>
        /// <returns>Уникальное название по поперечному сечению перемычки</returns>
        public static string GetLintelUniqueName(in FamilyInstance lintel, bool addLevel)
        {
            StringBuilder sb = new StringBuilder();

            var wallWidth = FamiliesMethods.GetWallWidth(lintel) * SharedValues.FootToMillimeters;
            sb.Append(wallWidth);
            sb.Append('_');

            var subCompsAdskNames = FamiliesMethods.GetSubComponentsAdskNames(lintel);
            foreach (var adskName in subCompsAdskNames)
            {
                sb.Append(adskName);
                sb.Append('_');
            }

            var widthOfLintel = Math.Round(FamiliesMethods.GetMaxWidthOfLintel(lintel) * SharedValues.FootToMillimeters, 0);
            sb.Append(widthOfLintel);

            if (addLevel)
            {
                sb.Append('_');
                Document doc = lintel.Document;
                var levelName = doc.GetElement(lintel.LevelId).Name;
                sb.Append(levelName);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Размещает семейство адаптивного компонента в плоскости || XoY в виде правильного многоугольника
        /// с центром в заданной точке. Если адаптивное семейство содержит менее 3 точек, 
        /// то центр также определяется геометрически.
        /// Координаты центра в ФУТАХ!
        /// </summary>
        /// <param name="document">Документ Revit</param>
        /// <param name="symbol">Типоразмер семейства</param>
        /// <param name="center">Координаты центра в ФУТАХ</param>
        /// <returns>Созданный элемент</returns>
        public static Element CreateAdaptiveComponentInstance(in Document document, in FamilySymbol symbol, in XYZ center)
        {
            // Create a new instance of an adaptive component family
            FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(document, symbol);

            // Get the placement points of this instance
            IList<ElementId> placePointIds = new List<ElementId>();
            placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);
            double x = 0;

            // Set the position of each placement point

            foreach (ElementId id in placePointIds)
            {
                double xCenter = 0;
                double yCenter = 0;
                double zCenter = 0;
                if (placePointIds.Count == 1)
                {
                    xCenter = center.X;
                    yCenter = center.Y;
                    zCenter = center.Z;
                }
                else
                {
                    xCenter += 10 * x + center.X;
                    yCenter += 10 * Math.Cos(x) + center.Y;
                    zCenter += 0 + center.Z;
                }
                ReferencePoint point = document.GetElement(id) as ReferencePoint;
                point.Position = new Autodesk.Revit.DB.XYZ(xCenter, yCenter, zCenter);
                x += Math.PI / 6;
            }

            return instance;
        }
    }
}
