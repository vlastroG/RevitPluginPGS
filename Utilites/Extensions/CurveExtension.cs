using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class CurveExtension
    {
        /// <summary>
        /// Определяет, лежит ли плоская кривая в плоскости || XOY (горизонтальной плоскости).
        /// </summary>
        /// <param name="curve">Кривая</param>
        /// <returns>True, если Curve || XOY, иначе false</returns>
        public static bool IsHorizontal(this Curve curve)
        {
            if (curve is Line)
            {
                return Math.Round((curve as Line).Direction.Z, 5) == 0;
            }
            if (curve is Arc)
            {
                return (curve as Arc).Normal.IsAlmostEqualTo(XYZ.BasisZ);
            }
            else return false;
        }

        /// <summary>
        /// Создает одну прямую линию из двух, если они являются прямими, коллинеарными, 
        /// и если конечная точка первой совпадает с начальной точкой второй
        /// </summary>
        /// <param name="curve">Текущая первая кривая</param>
        /// <param name="addCurveNext">Слудующая вторая кривая</param>
        /// <returns>True, если слияние кривых возможно и прошло успешно, иначе false</returns>
        public static bool AppendCurve(ref Curve curve, Curve addCurveNext)
        {
            if ((curve != null) && (addCurveNext != null))
            {
                XYZ curveVector = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
                XYZ addCurveVector = (addCurveNext.GetEndPoint(1) - addCurveNext.GetEndPoint(0)).Normalize();
                bool areCollinear = (curve is Line) && curveVector.IsAlmostEqualTo(addCurveVector);
                bool areJoin = curve.GetEndPoint(1).IsAlmostEqualTo(addCurveNext.GetEndPoint(0));
                if (areCollinear && areJoin)
                {
                    curve = Line.CreateBound(curve.GetEndPoint(0), addCurveNext.GetEndPoint(1));
                    return true;
                }
            }
            return false;
        }
    }
}
