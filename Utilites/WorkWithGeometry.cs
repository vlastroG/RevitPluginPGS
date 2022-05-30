using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites
{
    public static class WorkWithGeometry
    {
        /// <summary>
        /// Параметр ширины экземпляра семейства
        /// </summary>
        private static readonly Guid par_width = Guid.Parse("8f2e4f93-9472-4941-a65d-0ac468fd6a5d");

        /// <summary>
        /// Параметр высоты экземпляра семейства
        /// </summary>
        private static readonly Guid par_height = Guid.Parse("da753fe3-ecfa-465b-9a2c-02f55d0c2ff1");

        /// <summary>
        /// Коэффициент для перевода квадратных футов в квадратные метры
        /// </summary>
        private static readonly double _sqFeetToMeters = 0.3048 * 0.3048;

        /// <summary>
        /// Создание Solid из BoundingBox
        /// </summary>
        /// <param name="bbox">Входной BoundingBoxXYZ</param>
        /// <returns>Созданный Solid</returns>
        public static Solid SolidFromBoundingBox(BoundingBoxXYZ bbox)
        {
            // corners in BBox coords
            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);
            //edges in BBox coords
            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);
            //create loop, still in BBox coords
            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);
            Double height = bbox.Max.Z - bbox.Min.Z;
            CurveLoop baseLoop = CurveLoop.Create(edges);
            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);
            Solid preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);

            Solid transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);

            return transformBox;
        }


        /// <summary>
        /// Список параметров длина и ширины проемов
        /// </summary>
        private static readonly List<string[]> _parsWidthHeight = new List<string[]>
        {
            new string[] {"Высота", "Ширина", "Э"},
            new string[] {"Рзм.Высота", "Рзм.Ширина", "Э"},
            new string[] {"Примерная высота", "Примерная ширина", "Э"},
            new string[] {"Высота", "Ширина", "Т"},
            new string[] {"Рзм.Высота", "Рзм.Ширина", "Т" },
            new string[] {"Примерная высота", "Примерная ширина", "Т" }
        };

        /// <summary>
        /// Расчитывает площадь прямоугольного проема, выполненного загружаемым семейством.
        /// Расчитывается по экземпляру/типу
        /// по параметрам:
        ///               "Высота",           "Ширина";
        ///               "Рзм.Высота",       "Рзм.Ширина";
        ///               "Примерная высота", "Примерная ширина".
        /// Работает только для русского языка, с семействами,
        /// в которых присутствует хотя бы одна пара параметров.
        /// </summary>
        /// <param name="doc">Документ, в котороим расчитывается площадь</param>
        /// <param name="opening">Элемент проема для расчета</param>
        /// <returns></returns>
        public static double GetOpeningArea(Document doc, FamilyInstance opening)
        {
            var opening_type = doc.GetElement(opening.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());
            var element = opening;

            Parameter par_width = null;
            Parameter par_height = null;
            double opening_area = 0;

            for (int i = 0; i < _parsWidthHeight.Count; i++)
            {

                if (_parsWidthHeight[i][2] == "Т")
                {
                    par_height = opening_type.LookupParameter(_parsWidthHeight[i][0]);
                    par_width = opening_type.LookupParameter(_parsWidthHeight[i][1]);
                }
                else
                {
                    par_height = opening.LookupParameter(_parsWidthHeight[i][0]);
                    par_width = opening.LookupParameter(_parsWidthHeight[i][1]);
                }
                if (par_width != null && par_height != null && par_height.AsDouble() > 0 && par_width.AsDouble() > 0)
                {
                    var w = par_width.AsDouble();
                    var h = par_height.AsDouble();
                    opening_area = w * h * _sqFeetToMeters;
                    break;
                }
            }

            return opening_area;
        }
    }
}
