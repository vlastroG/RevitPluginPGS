using MS.Commands.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    public class OpeningDto
    {
        /// <summary>
        /// Перемычка
        /// </summary>
        private Lintel _lintel;


        /// <summary>
        /// Конструктор проема
        /// </summary>
        /// <param name="width">Ширина проема</param>
        /// <param name="wallThick">Толщина стены</param>
        /// <param name="wallHeightOverOpening">Высота стены над проемом</param>
        /// <param name="distanceToRightEnd">Расстояние от правой стороны проема до торца стены в мм</param>
        /// <param name="distanceToLeftEnd">Расстояние от левой стороны проема до торца стены в мм</param>
        /// <param name="wallMaterial">Название материала сердцевины стены</param>
        public OpeningDto(
            double width,
            double wallThick,
            double wallHeightOverOpening,
            double distanceToRightEnd,
            double distanceToLeftEnd,
            string wallMaterial)
        {
            Width = width;
            WallThick = wallThick;
            WallHeightOverOpening = wallHeightOverOpening;
            DistanceToRightEnd = distanceToRightEnd;
            DistanceToLeftEnd = distanceToLeftEnd;
            WallMaterial = wallMaterial;
        }


        /// <summary>
        /// Ширина проема вмм
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Толщина стены в мм
        /// </summary>
        public double WallThick { get; }

        /// <summary>
        /// Высота участка стены над проемом в мм
        /// </summary>
        public double WallHeightOverOpening { get; }

        /// <summary>
        /// Расстояние от правой стороны проема до торца стены в мм
        /// </summary>
        public double DistanceToRightEnd { get; }

        /// <summary>
        /// Расстояние от левой стороны проема до торца стены в мм
        /// </summary>
        public double DistanceToLeftEnd { get; }

        /// <summary>
        /// Название материала сердцевины стены
        /// </summary>
        public string WallMaterial { get; }

        /// <summary>
        /// Перемычка
        /// </summary>
        public Lintel Lintel { get => _lintel; set { _lintel = value; } }

        /// <summary>
        /// Проемы равны, если все их параметры равны
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>True, если проемы имеют одинаковое значение параметров, иначе False</returns>
        public override bool Equals(object obj)
        {
            if ((obj is null) || !(obj is OpeningDto))
            {
                return false;
            }
            OpeningDto dto = obj as OpeningDto;
            return (Width == dto.Width)
                && (WallThick == dto.WallThick)
                && (WallHeightOverOpening == dto.WallHeightOverOpening)
                && (DistanceToRightEnd == dto.DistanceToRightEnd)
                && (DistanceToLeftEnd == dto.DistanceToLeftEnd)
                && WallMaterial.Equals(dto.WallMaterial);
        }

        /// <summary>
        /// Возвращает HashCode проема
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Width.GetHashCode() +
                    WallThick.GetHashCode() +
                    WallHeightOverOpening.GetHashCode() +
                    DistanceToRightEnd.GetHashCode() +
                    DistanceToLeftEnd.GetHashCode() +
                    WallMaterial.GetHashCode();
        }
    }
}
