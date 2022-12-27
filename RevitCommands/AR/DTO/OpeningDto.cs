using MS.RevitCommands.AR.Models;
using MS.RevitCommands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.DTO
{
    /// <summary>
    /// Dto для хранения данных из Revit модели о проеме и его перемычке внутри диалогового окна менеджера перемычек
    /// с последующим парсингом отредактированного Dto в изменения в модели Revit
    /// </summary>
    public class OpeningDto : IIdentifiable
    {
        /// <summary>
        /// Перемычка
        /// </summary>
        private Lintel _lintel;

        /// <summary>
        /// Идентификатор
        /// </summary>
        private Guid _guid;


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
            Guid guid,
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
            _guid = guid;
        }


        /// <summary>
        /// Марка перемычки
        /// </summary>
        public string Mark { get; set; }

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
        /// Строковое представление расстояния от проема до торца стены справа
        /// </summary>
        public string DistanceConditionToRightEnd => DistanceToRightEnd >= 250 ? "≥250" : "<250";

        /// <summary>
        /// Строковое представление расстояния от проема до торца стены слева
        /// </summary>
        public string DistanceConditionToLeftEnd => DistanceToLeftEnd >= 250 ? "≥250" : "<250";

        /// <summary>
        /// Название материала сердцевины стены
        /// </summary>
        public string WallMaterial { get; }

        /// <summary>
        /// Перемычка
        /// </summary>
        public Lintel Lintel { get => _lintel; set { _lintel = value; } }

        public Guid Guid => _guid;

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
