using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ExtensionCurve
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
    }
}
