using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ExtensionCurveArray
    {
        /// <summary>
        /// Длина массива кривых в футах
        /// </summary>
        /// <param name="curveArr">Массив кривых</param>
        /// <returns>Суммарная длина кривых в футах</returns>
        public static double GetLength(this CurveArray curveArr)
        {
            double length = 0;
            foreach (Curve curve in curveArr)
            {
                length += curve.Length;
            }
            return length;
        }
    }
}
