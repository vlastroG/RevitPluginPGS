using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using MS.Shared;
using System.Linq;

namespace MS.Utilites
{
    public static class WorkWithGeometry
    {
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
            List<Curve> edges = new List<Curve>
            {
                edge0,
                edge1,
                edge2,
                edge3
            };
            Double height = bbox.Max.Z - bbox.Min.Z;
            CurveLoop baseLoop = CurveLoop.Create(edges);
            List<CurveLoop> loopList = new List<CurveLoop>
            {
                baseLoop
            };
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

            Parameter par_width;
            Parameter par_height;
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
        /// Функция находит ширину и высоту вырезаемой проемом части стены,
        /// выполненным прямоугольным семейством или прямоугольным вложенным витражом.
        /// Релизовано 5 случаев:
        /// 1. Проем по вертикали полностью внутри стены, или нижней/верхней гранью касается нижней/вержней грани стены;
        /// 2. Проем снизу и сверху вылазит из стены;
        /// 3. Проем вылазит только снизу стены;
        /// 4. Проем вылазит только сверху стены;
        /// 5. Проем находится вне стены.
        /// </summary>
        /// <param name="insertedElement">Проем внутри стены: семейство, размещенной в стене или витраж.</param>
        /// <param name="hostWall">Стена, в которой размещен проем.</param>
        /// <returns>Кортеж высоты и ширины проема</returns>
        /// <exception cref="ArgumentException">Исключение, если элемент и стена в разных документах.</exception>
        /// <exception cref="ArgumentNullException">Исключение, если не найден 3D вид по умолчанию.</exception>
        public static (double Height, double Width) GetWidthAndHeightOfInsertElement(
            Element insertedElement,
            Wall hostWall)
        {
            if (insertedElement.Document.PathName != hostWall.Document.PathName)
            {
                throw new ArgumentException(
                    $"Elements with ids: {insertedElement.Id} & {hostWall.Id} aren't in the same document!");
            }
            Document doc = insertedElement.Document;
            View3D view3d;
            BoundingBoxXYZ wallBox;
            BoundingBoxXYZ elemBox;
            using (var collector = new FilteredElementCollector(doc))
            {
                view3d = collector
                                .OfClass(typeof(View3D))
                                .Cast<View3D>()
                                .FirstOrDefault(v => v.Name == "{3D}");
            }
            try
            {
                wallBox = hostWall.get_BoundingBox(view3d);
                elemBox = insertedElement.get_BoundingBox(view3d);
            }
            catch (Exception)
            {
                throw new ArgumentNullException("Не найден 3D вид по умолчанию!");
            }
            double wallBottom = wallBox.Min.Z;
            double wallTop = wallBox.Max.Z;
            double elemBottom = elemBox.Min.Z;
            double elemTop = elemBox.Max.Z;

            double deltaBottom = elemBottom - wallBottom;
            double deltaTop = wallTop - elemTop;
            (double heightEl, double widthEl) = GetWidthAndHeightOfElement(insertedElement);
            if (deltaBottom >= 0 && deltaTop >= 0)// проем внутри стены
            {
                return (heightEl, widthEl);
            }
            if (deltaBottom < 0 && deltaTop < 0) // проем выходит за пределы стены сверху и снизу
            {
                double height = hostWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                return (height, widthEl);
            }
            if (deltaBottom < 0 && deltaTop > 0)// проем выходит за пределы стены только снизу
            {
                double height = elemTop - wallBottom;
                return (height, widthEl);
            }
            if (deltaBottom > 0 && deltaTop < 0)// проем выходит за пределы стены только сверху
            {
                double height = wallTop - elemBottom;
                return (height, widthEl);
            }
            // иначе проем не заходит на стену
            return (0, 0);
        }

        /// <summary>
        /// Возвращает кортеж двух чисел: высоты и ширины элемента в футах
        /// </summary>
        /// <param name="element">Проем: либо <see cref="Autodesk.Revit.DB.FamilyInstance"/>, либо <see cref="Wall"/></param>
        /// <returns></returns>
        public static (double Height, double Width) GetWidthAndHeightOfElement(Element element)
        {
            double height = 0;
            double width = 0;
            if (element is FamilyInstance)
            {
                // Если элемент - загружаемое семейство
                var famInst = element as FamilyInstance;
                try
                {
                    if (famInst.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM) != null
                        && famInst.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM).AsDouble() > 0)
                    {
                        //"Высота", "Ширина", "Э"
                        height = famInst.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM).AsDouble();
                        width = famInst.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble();
                    }
                    else if (famInst.get_Parameter(SharedParams.ADSK_DimensionHeight) != null
                        && famInst.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble() > 0)
                    {
                        //"Рзм.Высота", "Рзм.Ширина", "Э"
                        height = famInst.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble();
                        width = famInst.get_Parameter(SharedParams.ADSK_DimensionWidth).AsDouble();
                    }
                    else if (famInst.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM) != null
                        && famInst.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM).AsDouble() > 0)
                    {
                        //"Примерная высота", "Примерная ширина", "Э"
                        height = famInst.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM).AsDouble();
                        width = famInst.get_Parameter(BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM).AsDouble();
                    }
                    else if (famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM) != null
                        && famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM).AsDouble() > 0)
                    {
                        //"Высота", "Ширина", "Т"
                        height = famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_HEIGHT_PARAM).AsDouble();
                        width = famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM).AsDouble();
                    }
                    else if (famInst.Symbol.get_Parameter(SharedParams.ADSK_DimensionHeight) != null
                        && famInst.Symbol.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble() > 0)
                    {
                        //"Рзм.Высота", "Рзм.Ширина", "Т"
                        height = famInst.Symbol.get_Parameter(SharedParams.ADSK_DimensionHeight).AsDouble();
                        width = famInst.Symbol.get_Parameter(SharedParams.ADSK_DimensionWidth).AsDouble();
                    }
                    else if (famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM) != null
                        && famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM).AsDouble() > 0)
                    {
                        //"Примерная высота", "Примерная ширина", "Т"
                        height = famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM).AsDouble();
                        width = famInst.Symbol.get_Parameter(BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM).AsDouble();
                    }
                }
                catch (NullReferenceException)
                {
                    height = 0;
                    width = 0;
                }
            }
            else if (element is Wall)
            {
                // Если элемент - стена
                height = element.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                width = element.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            }
            return (height, width);
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
                FindReferenceTarget.Element, view3d)
              {
                  // We don't want to find elements in linked files

                  FindReferencesInRevitLinks = false
              };

            XYZ toWallDir = GetRightDirection(
              lineDirection);

            ReferenceWithContext context = intersector
              .FindNearest(startPoint, toWallDir);

            Reference closestReference;

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
                FindReferenceTarget.Element, view3d)
              {
                  // We don't want to find elements in linked files

                  FindReferencesInRevitLinks = false
              };

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

            Reference closestReference;

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
                WorkWithPath.CreateTempDir(@dirPath);
                b.Save(@filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// Возвращает расстояние со знаком от точки до плоскости
        /// </summary>
        /// <param name="plane">Плоскость</param>
        /// <param name="point">Точка</param>
        /// <returns>Расстояние со знаком</returns>
        public static double SignedDistanceTo(
            Plane plane,
            XYZ point)
        {
            XYZ vector = point - plane.Origin;

            return plane.Normal.DotProduct(vector);
        }
    }
}
