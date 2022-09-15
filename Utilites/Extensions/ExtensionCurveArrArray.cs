using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ExtensionCurveArrArray
    {
        /// <summary>
        /// Возвращается массив кривых, сумма длин которых наибольшая.
        /// Если в массиве присутствует несколько массивов с одинаковой суммой длин кривых,
        /// то возвращается первый из них.
        /// </summary>
        /// <param name="curveArrArray"></param>
        /// <returns></returns>
        public static CurveArray GetLongestCurveArray(this CurveArrArray curveArrArray)
        {
            CurveArray longestCurveArray = curveArrArray.get_Item(0);
            double longestCurveArrayLength = longestCurveArray.GetLength();
            for (var i = 0; i < curveArrArray.Size; i++)
            {
                CurveArray curveArray = curveArrArray.get_Item(i);
                double length = curveArray.GetLength();
                if (length > longestCurveArrayLength)
                {
                    longestCurveArrayLength = length;
                    longestCurveArray = curveArray;
                }
            }
            return longestCurveArray;
        }
    }
}
