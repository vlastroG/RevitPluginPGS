using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class FaceArrayExtension
    {
        /// <summary>
        /// Возвращает перечисление плоских поверностей из массива поверхностей
        /// </summary>
        /// <param name="faceArray">Массив поверхностей</param>
        /// <returns>Перечисление плоских поверхностей</returns>
        public static IEnumerable<PlanarFace> GetPlanarFaces(this FaceArray faceArray)
        {
            foreach (var face in faceArray)
            {
                if (face is PlanarFace)
                    yield return (PlanarFace)face;
            }
        }
    }
}
