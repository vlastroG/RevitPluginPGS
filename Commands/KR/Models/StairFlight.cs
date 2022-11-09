using Autodesk.Revit.DB;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MS.Commands.KR.Models
{
    internal sealed class StairFlight
    {
        /// <summary>
        /// Лестничный марш.
        /// </summary>
        private readonly Solid _stairFlightSolid;

        /// <summary>
        /// Боковые грани лестничного марша: 2 грани, равных по площади.
        /// </summary>
        private readonly List<PlanarFace> _facesSide = new List<PlanarFace>();

        /// <summary>
        /// Грани проступей: все грани, нормаль которых совпадает по направлению с осью Z.
        /// </summary>
        private readonly List<PlanarFace> _facesTread = new List<PlanarFace>();

        /// <summary>
        /// Грани подступей, одинаковые по площади с округлением до заданного знака после запятой.
        /// </summary>
        private readonly List<PlanarFace> _facesRiser = new List<PlanarFace>();

        private PlanarFace _faceAngular;

        /// <summary>
        /// Количество знаков после запятой для проверки чисел на равенство.
        /// </summary>
        private readonly int __roundTolerance = 5;


        /// <summary>
        /// Список поверхностей всех поверхностей solid  лестничного марша.
        /// </summary>
        private readonly List<PlanarFace> _facesAll = new List<PlanarFace>();


        public StairFlight(Solid stairFlightSolid)
        {
            _stairFlightSolid = stairFlightSolid;

            FillFaces(_stairFlightSolid);

            AnalyseFaces(_facesAll);
        }


        /// <summary>
        /// Заполняет список всех поверхностей лестничного марша.
        /// </summary>
        /// <param name="solid">Лестничный марш.</param>
        private void FillFaces(Solid solid)
        {
            FaceArray solidSurfaces = solid.Faces;
            foreach (PlanarFace face in solidSurfaces)
            {
                _facesAll.Add(face);
            }
        }

        /// <summary>
        /// Заполнение списков поверхностей.
        /// </summary>
        /// <param name="faces"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void AnalyseFaces(List<PlanarFace> faces)
        {
            int sideFacesLeftOverCount = 2;
            int angularFacesCount = 1;
            foreach (PlanarFace face in faces)
            {
                EdgeArray edgeLoop = face.GetMaxLengthEdgeArray();
                XYZ normal = face.FaceNormal.Normalize();
                // Заполнение списка боковых граней.
                if (edgeLoop.Size > 4 && sideFacesLeftOverCount > 0 && Math.Round(normal.Z, __roundTolerance) == 0)
                {
                    _facesSide.Add(face);
                    sideFacesLeftOverCount--;
                    continue;
                }

                // Заполнение списка проступей.
                if (normal.IsAlmostEqualTo(XYZ.BasisZ))
                {
                    _facesTread.Add(face);
                    continue;
                }

                // Первичное заполнение списка подступей вертикальными гранями.
                if (Math.Round(normal.Z, __roundTolerance) == 0)
                {
                    _facesRiser.Add(face);
                    continue;
                }

                // Получение наклонной четырехугольной грани (первой, которая попадется).
                if (0 < Math.Abs(Math.Round(normal.Z, __roundTolerance))
                    && Math.Abs(Math.Round(normal.Z, __roundTolerance)) < 1
                    && edgeLoop.Size == 4
                    && angularFacesCount == 1)
                {
                    _faceAngular = face;
                    angularFacesCount--;
                }
            }

            XYZ[] verticalFacesNormals = _facesRiser
                .Select(face => face.FaceNormal.Normalize())
                .Distinct()
                .ToArray();
            int normalsMaxCount = 0;
            // Нормаль к граням подступей.
            XYZ facesRiserNormal = new XYZ();
            foreach (XYZ normal in verticalFacesNormals)
            {
                int facesRiserOfThisNormalCount = _facesRiser
                    .Where(face => face.FaceNormal.Normalize().IsAlmostEqualTo(normal))
                    .Count();
                if (facesRiserOfThisNormalCount > normalsMaxCount)
                {
                    facesRiserNormal = normal;
                }
            }
            // Удаление лишних граней, нормаль которых не совпадает с нормалью подступей, из списка подступей.
            _facesRiser.RemoveAll(face => !face.FaceNormal.Normalize().IsAlmostEqualTo(facesRiserNormal));

            double faceRiserArea = 0;
            int faceRiserCount = 0;
            double[] faceRiserAreaArray = _facesRiser
                .Select(face => Math.Round(face.Area, __roundTolerance))
                .Distinct()
                .ToArray();
            foreach (double area in faceRiserAreaArray)
            {
                int facesRiserOfThisArea = _facesRiser.Where(face => Math.Round(face.Area, __roundTolerance) == area).Count();
                if (facesRiserOfThisArea > faceRiserCount)
                {
                    faceRiserArea = area;
                }
            }
            // Удаление граней НЕ с самой частовстречающейся площадью.
            _facesRiser.RemoveAll(face => Math.Round(face.Area, __roundTolerance) != faceRiserArea);

            FacesValidation();
        }

        /// <summary>
        /// Проверка списков граней лестничного марша на корректное заполнение.
        /// </summary>
        /// <exception cref="InvalidOperationException">Списки некорректные.</exception>
        private void FacesValidation()
        {
            // Исключение, если списки граней лестницы не заполнены или заполнены некорректно.
            if (_facesSide.Count != 2
                || _facesTread.Count < 2
                || _facesRiser.Count < 1
                || (Math.Round(_facesSide[0].Area, __roundTolerance) != Math.Round(_facesSide[1].Area, __roundTolerance))
                || _faceAngular == null)
            {
                throw new InvalidOperationException("Списки граней лестничного марша заполнены некорректно.");
            }
        }
    }
}
