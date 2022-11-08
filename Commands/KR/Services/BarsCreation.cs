using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR.Services
{
    public static class BarsCreation
    {
        /// <summary>
        /// Защитный слой торцов стержней = 20 мм
        /// </summary>
        private static readonly int _rebarCoverStepsEnd = 20;

        /// <summary>
        /// Название типа арматурного стрежния ø6 A240
        /// </summary>
        private static readonly string _rbtD6A240 = "ø6 A240";

        /// <summary>
        /// Название типа арматурного стержня ø12
        /// </summary>
        private static readonly string _rbtD12 = "ø12";


        /// <summary>
        /// Создает армирование лестничного марша
        /// </summary>
        /// <param name="host">Основа арматуры</param>
        /// <param name="curves">Ребра ступеней, выбранные пользователем</param>
        /// <param name="anglePlane">Нижняя наклонная плоскость марша, выбранная пользователем</param>
        /// <param name="rebarDiameterSteps">Диаметр стержней каркасов ступеней</param>
        /// <param name="rebarDiameterMain">Диаметр рабочих стержней марша</param>
        /// <param name="rebarCoverSteps">Защитный слой для арматуры каркасов ступеней</param>
        /// <param name="rebarCoverMainAngle">Защитный слой рабочей арматуры у наклонной грани</param>
        /// <param name="rebarCoverMainHoriz">Защитный слой рабочей арматуры у горизонтальных граней</param>
        /// <param name="barsStepStepsHorizont">Шаг горизонтальных прямых стержней каркасов ступеней</param>
        /// <param name="barsStepStepsVert">Шаг вертикальных Г - стержней каркасов ступеней</param> 
        /// <param name="barsStepMainHorizont">Шаг рабочих горизонтальных прямых стержней</param>
        /// <param name="barsStepMainAngle">Шаг рабочих наклонных Z - стержней</param>
        public static void CreateStairReinforcement(
            in Element host,
            in List<Curve> curves,
            in PlanarFace anglePlane,
            int rebarDiameterSteps,
            int rebarDiameterMain,
            int rebarCoverSteps,
            int rebarCoverMainAngle,
            int rebarCoverMainHoriz,
            int barsStepStepsHorizont,
            int barsStepStepsVert,
            int barsStepMainHorizont,
            int barsStepMainAngle)
        {
            using (Transaction transSteps = new Transaction(host.Document))
            {
                transSteps.Start("Каркасы ступеней");
                foreach (var curve in curves)
                {
                    CreateStairStepBarsFrame(
                         host,
                         curve,
                         anglePlane,
                         rebarDiameterSteps,
                         rebarCoverSteps,
                         barsStepStepsHorizont,
                         barsStepStepsVert);
                }
                transSteps.Commit();
            }

            using (Transaction transMain = new Transaction(host.Document))
            {
                transMain.Start("Рабочая арматура марша");
                CreateStairStepMainBars(
                    host,
                    anglePlane,
                    rebarDiameterMain,
                    rebarCoverMainAngle,
                    rebarCoverMainHoriz,
                    barsStepMainHorizont,
                    barsStepMainAngle);
                transMain.Commit();
            }
        }

        /// <summary>
        /// Создает каркас ступени
        /// </summary>
        /// <param name="host">Элемент - основа арматуры</param>
        /// <param name="curve">Линия ребра ступени</param>
        /// <param name="anglePlane">Наклонная плоскость грани марша</param>
        /// <param name="rebarDiameter">Диаметр стержней каркаса ступеней</param>
        /// <param name="rebarCoverSteps">Защитный слой стержней каркаса ступеней</param>
        /// <param name="barsStepHorizont">Шаг горизонтальных прямых стержней каркаса</param>
        /// <param name="barsStepVert">Шаг вертикальных Г- образных стержней каркаса</param>
        /// <returns></returns>
        private static Element CreateStairStepBarsFrame(
            in Element host,
            in Curve curve,
            in PlanarFace anglePlane,
            int rebarDiameter,
            int rebarCoverSteps,
            int barsStepHorizont,
            int barsStepVert)
        {
            Document doc = host.Document;
            XYZ toBottomDir = XYZ.BasisZ.Negate();
            var anglePlaneNormal = anglePlane.FaceNormal;
            XYZ toStepDir = new XYZ(anglePlaneNormal.X, anglePlaneNormal.Y, 0);

            Curve cCorner = CreateStairStepCornerBarCurve(
                curve,
                rebarDiameter,
                rebarCoverSteps,
                toStepDir);

            var barsVertSideOffset = GetVerticalStepBarsSideOffset(cCorner, barsStepVert);
            var cAngle = GetStairStepAngleBarCurves(
                cCorner,
                anglePlane,
                rebarDiameter,
                barsVertSideOffset);

            Rebar barX = null;
            Rebar barY = null;
            Rebar barL = null;

            // Стержни проступи
            #region Прямые горизонтальные стержни каркаса
            barX = Rebar.CreateFromCurves(
                doc,
                RebarStyle.Standard,
                GetRebarBarType(doc, _rbtD6A240),
                null,
                null,
                host,
                toStepDir,
                new Curve[1] { cCorner },
                RebarHookOrientation.Left,
                RebarHookOrientation.Left,
                true,
                false);
            barX.GetShapeDrivenAccessor()
                .SetLayoutAsNumberWithSpacing
                (3,
                barsStepHorizont / SharedValues.FootToMillimeters,
                true,
                true,
                true);

            // Стержни подступенка
            barY = Rebar.CreateFromCurves(
                doc,
                RebarStyle.Standard,
                GetRebarBarType(doc, _rbtD6A240),
                null,
                null,
                host,
                toBottomDir,
                new Curve[1] { cCorner },
                RebarHookOrientation.Left,
                RebarHookOrientation.Left,
                true,
                false);
            barY.GetShapeDrivenAccessor()
                .SetLayoutAsNumberWithSpacing
                (2,
                barsStepHorizont / SharedValues.FootToMillimeters,
                true,
                false,
                true);
            #endregion

            #region Г-стержни каркаса

            // Крайний левый стержень
            barL = Rebar.CreateFromCurves(
                doc,
                RebarStyle.Standard,
                GetRebarBarType(doc, _rbtD6A240),
                null,
                null,
                host,
                (cCorner as Line).Direction,
                cAngle,
                RebarHookOrientation.Left,
                RebarHookOrientation.Left,
                true,
                false);

            int barsLCount = (int)Math.Round((cCorner.Length - 2 * barsVertSideOffset / SharedValues.FootToMillimeters)
                / (barsStepVert / SharedValues.FootToMillimeters)) + 1;

            barL.GetShapeDrivenAccessor()
                .SetLayoutAsNumberWithSpacing
                (barsLCount,
                barsStepVert / SharedValues.FootToMillimeters,
                true,
                true,
                true);

            #endregion

            return barX;
        }

        /// <summary>
        /// Отступ осей вертикальных Г - стержней каркаса ступеней
        /// от торца горизонтального прямого стержня каркаса ступени
        /// </summary>
        /// <param name="cornerCurve">Линия оси прямого углового горизонтального стержня ступени</param>
        /// <param name="barsStepVert">Шаг вертикальных стержней</param>
        /// <returns>Отступ в мм</returns>
        private static double GetVerticalStepBarsSideOffset(
            in Curve cornerCurve,
            int barsStepVert)
        {
            return cornerCurve.Length * SharedValues.FootToMillimeters % barsStepVert / 2;
        }


        /// <summary>
        /// Создает эскиз углового горизонтального стержня каркаса ступени
        /// </summary>
        /// <param name="curve">Ребро ступени</param>
        /// <param name="rebarDiameter">Диаметр стержня каркаса ступени</param>
        /// <param name="rebarCoverSteps">Защитный слой стержней каркаса ступени</param>
        /// <param name="toStepDir">Вектор направления движения вверх по лестнице</param>
        /// <returns>Эскиз стержня с учетом всех отступов</returns>
        private static Curve CreateStairStepCornerBarCurve(
            in Curve curve,
            int rebarDiameter,
            int rebarCoverSteps,
            XYZ toStepDir
            )
        {
            double cCornerCenterOffset = (rebarCoverSteps + rebarDiameter * 1.5) / SharedValues.FootToMillimeters;
            XYZ cCornerToBottomV = XYZ.BasisZ
                .Negate()
                .Multiply(cCornerCenterOffset);
            XYZ cCornerToStepV = toStepDir
                .Normalize()
                .Multiply(cCornerCenterOffset);

            XYZ cCornerStartOffsetV = (curve as Line)
                .Direction
                .Normalize()
                .Multiply(_rebarCoverStepsEnd / SharedValues.FootToMillimeters);

            XYZ cCornerEndOffsetV = (curve as Line)
                .Direction
                .Negate()
                .Normalize()
                .Multiply(_rebarCoverStepsEnd / SharedValues.FootToMillimeters);

            XYZ cCornerStart = curve
                .GetEndPoint(0)
                .Add(cCornerStartOffsetV)
                .Add(cCornerToBottomV)
                .Add(cCornerToStepV);

            XYZ cCornerEnd = curve
                .GetEndPoint(1)
                .Add(cCornerEndOffsetV)
                .Add(cCornerToBottomV)
                .Add(cCornerToStepV);

            return Line.CreateBound(cCornerStart, cCornerEnd);
        }


        /// <summary>
        /// Возвращает тип арматурного стержня по имени типа
        /// </summary>
        /// <param name="doc">Документ для поиска</param>
        /// <param name="rbtName">Наименование типа арматурного стержня</param>
        /// <returns>Тип арматурного стержня</returns>
        private static RebarBarType GetRebarBarType(in Document doc, string rbtName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .WhereElementIsElementType()
                .First(rbt => rbt.Name.Equals(rbtName)) as RebarBarType;
        }

        /// <summary>
        /// Создает эскиз Г-стержня для каркаса ступени
        /// </summary>
        /// <param name="cornerBarCurve">Угловой горизонтальный стержень</param>
        /// <param name="anglePlane">Наклонная плоскость лестницы</param>
        /// <param name="rebarDiameter">Диаметр Г-стержня</param>
        /// <param name="barsVertSideOffset">Отступ оси крайнего Г-стержня от торца горизонтального углового стержня</param>
        /// <returns>Список линий, составляющих эскиз Г-стержня</returns>
        private static IList<Curve> GetStairStepAngleBarCurves(
            in Curve cornerBarCurve,
            in PlanarFace anglePlane,
            int rebarDiameter,
            double barsVertSideOffset
            )
        {
            double angleStair = anglePlane.FaceNormal.AngleTo(XYZ.BasisZ);
            var anglePlaneNormal = anglePlane.FaceNormal;
            XYZ toStepDir = new XYZ(anglePlaneNormal.X, anglePlaneNormal.Y, 0);
            // Расстояние в футах от центра угла Г-стержня
            // до плоскости нижнего защитного слоя наклонной плоскости марша
            double distanceToCoverPlane =
                Math.Abs(anglePlane.DistanceTo(cornerBarCurve.GetEndPoint(0), out _)) +
                (rebarDiameter - _rebarCoverStepsEnd) / SharedValues.FootToMillimeters;

            // Длина вертикального сегмента в футах
            double lengthVert = distanceToCoverPlane / Math.Abs(Math.Cos(angleStair));
            // Длина горизонтального сегмента в футах
            double lengthHoriz = distanceToCoverPlane / Math.Abs(Math.Sin(angleStair));

            XYZ sideOffsetDirection = (cornerBarCurve as Line)
                .Direction
                .Normalize()
                .Multiply(barsVertSideOffset / SharedValues.FootToMillimeters);
            XYZ anglePoint = cornerBarCurve
                .GetEndPoint(0)
                .Add(sideOffsetDirection)
                .Add(XYZ.BasisZ.Multiply(rebarDiameter / SharedValues.FootToMillimeters))
                .Add(toStepDir.Negate().Multiply(rebarDiameter / SharedValues.FootToMillimeters));
            XYZ bottomPoint = new XYZ(anglePoint.X, anglePoint.Y, anglePoint.Z - lengthVert);
            XYZ horizontEndPoint = anglePoint.Add(toStepDir.Normalize().Multiply(lengthHoriz));

            return new Curve[2]
            {
                Line.CreateBound(horizontEndPoint, anglePoint),
                Line.CreateBound(anglePoint, bottomPoint)
            };
        }


        /// <summary>
        /// Создает рабочую арматуру марша
        /// </summary>
        /// <param name="host">Элемент - основа арматуры</param>
        /// <param name="anglePlane">Нижняя наклонная плоскость марша</param>
        /// <param name="rebarDiameter">Диаметр рабочих стержней</param>
        /// <param name="rebarCoverMainAngle">Защитный слой у наклонной грани</param>
        /// <param name="rebarCoverMainHoriz">Защитный слой у горизонтальных граней</param>
        /// <param name="barsStepHorizont">Шаг горизонтальных прямых стержней</param>
        /// <param name="barsStepVert">Шаг наклонных Z - стержней</param>
        private static void CreateStairStepMainBars(
            in Element host,
            in PlanarFace anglePlane,
            int rebarDiameter,
            int rebarCoverMainAngle,
            int rebarCoverMainHoriz,
            int barsStepMainHorizont,
            int barsStepMainVert)
        {
            (IList<Curve> curves, int barsCount) = GetStairMainBarCurves(anglePlane, rebarDiameter, rebarCoverMainAngle, rebarCoverMainHoriz, barsStepMainVert);
            var normal = (curves[1] as Line).Direction.CrossProduct(anglePlane.FaceNormal).Negate();

            Rebar barZ = Rebar.CreateFromCurves(
                host.Document,
                RebarStyle.Standard,
                GetRebarBarType(host.Document, _rbtD12),
                null,
                null,
                host,
                normal,
                curves,
                RebarHookOrientation.Left,
                RebarHookOrientation.Left,
                true,
                false);

            barZ.GetShapeDrivenAccessor()
                .SetLayoutAsNumberWithSpacing
                (barsCount,
                barsStepMainVert / SharedValues.FootToMillimeters,
                true,
                true,
                true);
        }

        /// <summary>
        /// Возвращает линию эскиза Z - стержня мерша
        /// </summary>
        /// <param name="anglePlane">Наклонная плоскость марша</param>
        /// <param name="rebarDiameter">Диаметр рабочей арматуры марша</param>
        /// <param name="rebarCoverMainAngle">Защитный слой рабочей арматуры марша у наклонной грани</param>
        /// <param name="rebarCoverMainHoriz">Защитный слой рабочей арматуры марша на горизонтальных участках</param>
        /// <returns>Кортеж списка линий эскиза Z стержня и их количества</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static (IList<Curve> curves, int barsCount) GetStairMainBarCurves(
        in PlanarFace anglePlane,
        int rebarDiameter,
        int rebarCoverMainAngle,
        int rebarCoverMainHoriz,
        int barsStepMainAngle)
        {
            var curve = GetMainAngleCurve(anglePlane);
            if (curve is null)
            {
                throw new InvalidOperationException(
                    $"Нельзя определить наклонную линию грани с Id {anglePlane.Id} " +
                    $"и площадью {anglePlane.Area * SharedValues.SqFeetToMeters} м.кв.");
            }
            // расстояния от грани бетона до центра стержня с учетом защитного слоя и диаметра стержня
            // на наклонном участке
            double curveOffsetAngle = (rebarCoverMainAngle + rebarDiameter * 0.5) / SharedValues.FootToMillimeters;
            // на прямых участках
            double curveOffsetHoriz = (rebarCoverMainHoriz + rebarDiameter * 0.5) / SharedValues.FootToMillimeters;

            // Нормаль к наклонной линии, направленная внутрь Solid лестницы,
            // с длиной с учетом защитного слоя и диаметра арматуры
            XYZ normalAngleReverse = anglePlane.FaceNormal.Negate().Normalize().Multiply(curveOffsetAngle);

            // нижняя и верхняя точки изгибов Z - линии низа марша по бетону
            (XYZ bottomPoint, XYZ topPoint) =
                curve.GetEndPoint(0).Z < curve.GetEndPoint(1).Z
                ? (curve.GetEndPoint(0), curve.GetEndPoint(1))
                : (curve.GetEndPoint(1), curve.GetEndPoint(0));

            var anglePlaneNormal = anglePlane.FaceNormal;

            // Направление по ходу движения вверх по лестнице в горизонтальной плоскости
            XYZ toStepDir = new XYZ(anglePlaneNormal.X, anglePlaneNormal.Y, 0).Normalize();
            // Направление по ходу движения вниз по лестнице в горизонтальной плоскости
            XYZ fromStepDir = toStepDir.Negate();

            // вектор для смещения оси стержня вверх на горизонтальных участках Z формы,
            // с утетом защитного слоя и диаметра арматуры
            XYZ upDir = new XYZ(0, 0, curveOffsetHoriz);

            XYZ topLineOrigin = topPoint.Add(upDir);
            XYZ bottomLineOrigin = bottomPoint.Add(upDir);
            XYZ angleLineOrigin = bottomPoint.Add(normalAngleReverse);

            Line bottomLine = Line.CreateUnbound(bottomLineOrigin, fromStepDir);
            Line topLine = Line.CreateUnbound(topLineOrigin, fromStepDir);
            Line angleLine = Line.CreateUnbound(angleLineOrigin, topPoint.Subtract(bottomPoint));

            (int mainAngleBarsSideOffset, int barsCount) = GetMainAngleBarsSideOffset(
                GetMainHorizontalCurve(anglePlane), barsStepMainAngle);
            XYZ sideOffsetMainAngle = topLineOrigin.Subtract(bottomLineOrigin).CrossProduct(anglePlane.FaceNormal).Normalize().Negate().Multiply(mainAngleBarsSideOffset / SharedValues.FootToMillimeters);

            XYZ bottomCorner = WorkWithGeometry.GetIntersectPoint(bottomLine, angleLine).Add(sideOffsetMainAngle);
            XYZ topCorner = WorkWithGeometry.GetIntersectPoint(angleLine, topLine).Add(sideOffsetMainAngle);

            // 1 - длина анкеровки сверху и снизу - для упрощения по 1 футу.
            XYZ startBottomZ = bottomCorner.Add(fromStepDir.Multiply(1));
            XYZ endTopZ = topCorner.Add(toStepDir.Multiply(1));


            return (new Curve[3]
            {
                Line.CreateBound(startBottomZ, bottomCorner),
                Line.CreateBound(bottomCorner, topCorner),
                Line.CreateBound(topCorner, endTopZ)
            },
            barsCount);
        }


        /// <summary>
        /// Возвращает отступ от боковых граней для оси Z - стержня марша
        /// </summary>
        /// <param name="curve">Горизонтальная линия границы наклонной плоскости низа марша (ширина марша)</param>
        /// <param name="barsStepMainAngle">Шаг Z - стержней марша</param>
        /// <returns>Кортеж отступа в мм и количества стержней</returns>
        private static (int offset, int count) GetMainAngleBarsSideOffset(
            in Curve curve,
            int barsStepMainAngle)
        {
            return ((int)(curve.Length * SharedValues.FootToMillimeters % barsStepMainAngle / 2),
                (int)(curve.Length * SharedValues.FootToMillimeters / barsStepMainAngle + 1));
        }

        /// <summary>
        /// Возвращает первую горизонтальную прямую линию наклонной плоскости
        /// </summary>
        /// <param name="anglePlane">Наклонная плоскость</param>
        /// <returns>Горизонтальная прямая линия наклонной плоскости (ширина марша)</returns>
        private static Curve GetMainHorizontalCurve(in PlanarFace anglePlane)
        {
            var curves = anglePlane.GetEdgesAsCurveLoops().OrderBy(l => l.GetExactLength()).Last().Simplify();
            foreach (Curve curve in curves)
            {
                if (curve is Line line)
                {
                    var z = Math.Round(line.Direction.Z, 6);
                    if (z == 0)
                    {
                        return line;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Определяет первую наклонную линию грани из самой длинной Loop+
        /// </summary>
        /// <param name="anglePlane">Наклонная грань</param>
        /// <returns>Наклонная линия, или null</returns>
        private static Curve GetMainAngleCurve(in PlanarFace anglePlane)
        {
            var curves = anglePlane.GetEdgesAsCurveLoops().OrderBy(l => l.GetExactLength()).Last().Simplify();
            foreach (Curve curve in curves)
            {
                if (curve is Line line)
                {
                    var z = Math.Round(line.Direction.Z, 6);
                    if (z > 0)
                    {
                        return line;
                    }
                }
            }
            return null;
        }
    }
}
