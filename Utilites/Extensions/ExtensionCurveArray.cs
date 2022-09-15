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

        /// <summary>
        /// Конвертирует CurveArr в список
        /// </summary>
        /// <param name="curveArr">Массив кривых Revit</param>
        /// <returns>Обычный список кривых</returns>
        public static List<Curve> ToList(this CurveArray curveArr)
        {
            List<Curve> list = new List<Curve>();
            for (int i = 0; i < curveArr.Size; i++)
            {
                list.Add(curveArr.get_Item(i));
            }
            return list;
        }
    }
}
