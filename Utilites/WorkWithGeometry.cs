using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
        /// Расчитывает площадь прямоугольного проема в размерах ревита (футах), выполненного загружаемым семейством.
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
                    opening_area = w * h;
                    break;
                }
            }

            return opening_area;
        }

        /// <summary>
        /// Расчитывает площадь стены постоянной высоты (прямоугольной на фасаде)
        /// </summary>
        /// <param name="wall">Стена для расчета</param>
        /// <returns>Площадь в единицах Revit - футах</returns>
        public static double GetRectangWallArea(Wall wall)
        {
            var length = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            var height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();

            var wall_area = length * height;

            return wall_area;
        }

        /// <summary>
        /// Создает отрезок (Curve), перпендикулярный направлению заданной кривой (Curve)
        /// длиной 2*sideIndent футов, центр которого находится над центром заданной кривой (Curve),
        /// смещенный на zMove футов вверх (если zMove положительно).
        /// Направление заданной кривой расчитывается как вектор,
        /// направленный из начальной точки заданной кривой в конечную.
        /// </summary>
        /// <param name="baseCurve">Заданная кривая</param>
        /// <param name="zMove">Смещение вверх</param>
        /// <param name="sideIndent">Отступ в сторону</param>
        /// <returns></returns>
        public static Curve CreateNormalCenterCurve(Curve baseCurve, double zMove, double sideIndent)
        {
            XYZ z_vector = new XYZ(0, 0, zMove);
            var baseCurve_direction = (baseCurve.GetEndPoint(1) - baseCurve.GetEndPoint(0))
                .Normalize();

            var toLeftVector = sideIndent
                * WorkWithGeometry.GetLeftDirection(baseCurve_direction);
            var toRightVector = sideIndent
                * WorkWithGeometry.GetRightDirection(baseCurve_direction);

            var baseCurve_center = baseCurve.Evaluate(0.5, true);

            var startPoint = baseCurve_center + toLeftVector + z_vector;
            var endPoint = baseCurve_center + toRightVector + z_vector;

            var normalCurve = Line.CreateBound(startPoint, endPoint) as Curve;

            return normalCurve;
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// left from given input vector.
        /// </summary>
        public static XYZ GetLeftDirection(XYZ direction)
        {
            double x = -direction.Y;
            double y = direction.X;
            double z = direction.Z;
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Return direction turning 90 degrees 
        /// right from given input vector.
        /// </summary>
        public static XYZ GetRightDirection(XYZ direction)
        {
            return GetLeftDirection(direction.Negate());
        }

        /// <summary>
        /// Return the neighbouring BIM element generating 
        /// the given room boundary curve c, assuming it
        /// is oriented counter-clockwise around the room
        /// if part of an interior loop, and vice versa.
        /// </summary>
        public static Element GetElementByRay(
          UIApplication app,
          Document doc,
          View3D view3d,
          Curve c)
        {
            Element boundaryElement = null;

            // Tolerances

            const double minTolerance = 0.00000001;
            const double maxTolerance = 0.01;

            // Height of ray above room level:
            // ray starts from one foot above room level

            const double elevation = 1;

            // Ray starts not directly from the room border
            // but from a point offset slightly into it.

            const double stepInRoom = 0.1;

            // We could use Line.Direction if Curve c is a 
            // Line, but since c also might be an Arc, we 
            // calculate direction like this:

            XYZ lineDirection
              = (c.GetEndPoint(1) - c.GetEndPoint(0))
                .Normalize();

            XYZ upDir = elevation * XYZ.BasisZ;

            // Assume that the room is on the left side of 
            // the room boundary curve and wall on the right.
            // This is valid for both outer and inner room 
            // boundaries (outer are counter-clockwise, inner 
            // are clockwise). Start point is slightly inside 
            // the room, one foot above room level.

            XYZ toRoomVec = stepInRoom * GetLeftDirection(
              lineDirection);

            XYZ pointBottomInRoom = c.Evaluate(0.5, true)
              + toRoomVec;

            XYZ startPoint = pointBottomInRoom + upDir;

            // We are searching for walls only

            ElementFilter wallFilter
              = new ElementCategoryFilter(
                BuiltInCategory.OST_Walls);

            ReferenceIntersector intersector
              = new ReferenceIntersector(wallFilter,
                FindReferenceTarget.Element, view3d);

            // We don't want to find elements in linked files

            intersector.FindReferencesInRevitLinks = false;

            XYZ toWallDir = GetRightDirection(
              lineDirection);

            ReferenceWithContext context = intersector
              .FindNearest(startPoint, toWallDir);

            Reference closestReference = null;

            if (context != null)
            {
                if ((context.Proximity > minTolerance)
                  && (context.Proximity < maxTolerance
                    + stepInRoom))
                {
                    closestReference = context.GetReference();

                    if (closestReference != null)
                    {
                        boundaryElement = doc.GetElement(
                          closestReference);
                    }
                }
            }
            return boundaryElement;
        }

        /// <summary>
        /// Возвращает элемент, найденный через пересечение геометрии этого элемента и отрезка,
        /// построенного как смещенная вверх (относительно заданной линии) нормаль,
        /// продленная на равное расстояние в обе стороны от заданной линии.
        /// </summary>
        public static Element GetElementByRay_switch(
          UIApplication app,
          Document doc,
          View3D view3d,
          Curve c,
          bool isLeft)
        {
            Element boundaryElement = null;

            // Tolerances

            const double minTolerance = 0.00000001;
            const double maxTolerance = 1;

            // Height of ray above room level:
            // ray starts from one foot above room level

            const double elevation = 1;

            // Ray starts not directly from the room border
            // but from a point offset slightly into it.

            const double stepInRoom = 0.1;

            // We could use Line.Direction if Curve c is a 
            // Line, but since c also might be an Arc, we 
            // calculate direction like this:

            XYZ lineDirection
              = (c.GetEndPoint(1) - c.GetEndPoint(0))
                .Normalize();

            XYZ upDir = elevation * XYZ.BasisZ;

            // Assume that the room is on the left side of 
            // the room boundary curve and wall on the right.
            // This is valid for both outer and inner room 
            // boundaries (outer are counter-clockwise, inner 
            // are clockwise). Start point is slightly inside 
            // the room, one foot above room level.

            XYZ toRoomVec;
            if (isLeft)
            {
                toRoomVec = stepInRoom * GetLeftDirection(
                  lineDirection);
            }
            else
            {
                toRoomVec = stepInRoom * GetRightDirection(
                lineDirection);
            }

            XYZ pointBottomInRoom = c.Evaluate(0.5, true)
              + toRoomVec;

            XYZ startPoint = pointBottomInRoom + upDir;

            // Категории элементов, которые могут возвращаться методом
            ElementMulticategoryFilter multicategoryFilter
                 = new ElementMulticategoryFilter(new Collection<BuiltInCategory> {
                     BuiltInCategory.OST_CurtainWallMullions,
                     BuiltInCategory.OST_CurtainWallPanels,
                     BuiltInCategory.OST_Walls,
                     BuiltInCategory.OST_Doors,
                     BuiltInCategory.OST_Windows,
                     BuiltInCategory.OST_Columns,
                     BuiltInCategory.OST_GenericModel,
                     BuiltInCategory.OST_Roofs,
                     BuiltInCategory.OST_Floors,
                     BuiltInCategory.OST_Rooms});

            ReferenceIntersector intersector
              = new ReferenceIntersector(multicategoryFilter,
                FindReferenceTarget.Element, view3d);

            // We don't want to find elements in linked files

            intersector.FindReferencesInRevitLinks = false;

            XYZ toWallDir;
            if (isLeft)
            {
                toWallDir = GetRightDirection(
                  lineDirection);
            }
            else
            {
                toWallDir = GetLeftDirection(
                  lineDirection);
            }

            ReferenceWithContext context = intersector
              .FindNearest(startPoint, toWallDir);

            Reference closestReference = null;

            if (context != null)
            {
                if ((context.Proximity > minTolerance)
                  && (context.Proximity < maxTolerance
                    + stepInRoom))
                {
                    closestReference = context.GetReference();

                    if (closestReference != null)
                    {
                        boundaryElement = doc.GetElement(
                          closestReference);
                    }
                }
            }
            return boundaryElement;
        }

        /// <summary>
        /// Создает прямоугольник с заданной заливкой rgb, шириной и высотой;
        /// после чего сохраняет его как растровое изображение с заданным разрешением dpi
        /// по заданному пути.
        /// </summary>
        /// <param name="width">Ширина прямоугольника.</param>
        /// <param name="height">Высота прямоугольника.</param>
        /// <param name="dpi">Плотность пикселей на дюйм.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void CreateColoredRectanglePng(
            int width,
            int height,
            float dpi,
            int red,
            int green,
            int blue,
            string @dirPath,
            string @filePath)
        {
            int zero = 0; // Прямоугольник создается в начале координат изображения.

            using (Bitmap b = new Bitmap(width, height))
            {
                b.SetResolution(dpi, dpi);
                using (Graphics g = Graphics.FromImage(b))
                {
                    Brush brush = new SolidBrush(System.Drawing.Color.FromArgb(red, green, blue));
                    g.FillRectangle(brush, new System.Drawing.Rectangle(zero, zero, width, height));
                }
                Directory.CreateDirectory(@dirPath);
                b.Save(@filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
