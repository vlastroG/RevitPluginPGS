using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ExtensionWall
    {
        /// <summary>
        /// Создает массив кривых, образующих прямоугольный профиль, для прямоугольной стены.
        /// Если ось стены в виде дуги, то профиль будет представлять линейчатую поверхность,
        /// образованную движением вдоль этой образующей отрезка с длиной равной высоте стены.
        /// </summary>
        /// <param name="wall">Прямоугольная стена</param>
        /// <returns>Массив кривых, образующих прямоугольный профиль</returns>
        public static CurveArray GetRectangularProfile(this Wall wall)
        {
            CurveArray loop = new CurveArray();
            Curve centerBottomLine = (wall.Location as LocationCurve).Curve;
            double height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
            int zero = 0;
            int one = 1;
            XYZ bottomStart = centerBottomLine.GetEndPoint(zero);
            XYZ bottomEnd = centerBottomLine.GetEndPoint(one);
            Transform moveUp = Transform.CreateTranslation(new XYZ(zero, zero, height));
            Curve centerTopLine = centerBottomLine.CreateTransformed(moveUp).CreateReversed();
            XYZ topStart = centerTopLine.GetEndPoint(zero);
            XYZ topEnd = centerTopLine.GetEndPoint(one);
            Curve verticalToBottomStart = Line.CreateBound(topEnd, bottomStart);
            Curve verticalFromBottomEnd = Line.CreateBound(bottomEnd, topStart);
            loop.Append(centerBottomLine);
            loop.Append(verticalFromBottomEnd);
            loop.Append(centerTopLine);
            loop.Append(verticalToBottomStart);
            return loop;
        }
    }
}
