using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class CurveLoopExtension
    {
        /// <summary>
        /// Соединяет кривые у Loop, лежащие на одной прямой, и возвращает новую упрощенную путлю
        /// </summary>
        /// <param name="curveLoop">Замкнутая петля из кривых</param>
        /// <returns>Новая петля с "схлопнутыми" в одну кривыми, которые лежали на одной прямой друг за другм</returns>
        public static CurveLoop Simplify(this CurveLoop curveLoop)
        {
            CurveLoop simplifiedCurves = new CurveLoop();
            List<Curve> curveLoopList = curveLoop.ToList();
            Curve curvePrev = null;
            for (int i = 0; i < curveLoopList.Count; i++)
            {
                Curve curveCurrent = curveLoopList[i];
                bool isCurveAdded = CurveExtension.AppendCurve(ref curvePrev, curveCurrent);
                if (curvePrev == null)
                {
                    curvePrev = curveCurrent;
                    continue;
                }
                if (i != 0 && i != (curveLoopList.Count - 1) && !isCurveAdded)
                {
                    simplifiedCurves.Append(curvePrev);
                    curvePrev = curveCurrent;
                    continue;
                }
                if (i == (curveLoopList.Count - 1))
                {
                    if (isCurveAdded)
                    {
                        simplifiedCurves.Append(curvePrev);
                    }
                    else
                    {
                        simplifiedCurves.Append(curvePrev);
                        simplifiedCurves.Append(curveCurrent);
                    }
                }
            }
            return simplifiedCurves;
        }
    }
}
