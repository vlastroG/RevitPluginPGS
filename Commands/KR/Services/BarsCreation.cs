using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using MS.Shared;
using MS.Utilites;
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
        private static readonly int _rebarCoverEnd = 20;

        /// <summary>
        /// Название типа арматурного стрежния ø6 A240
        /// </summary>
        private static readonly string _rbtD6A240 = "ø6 A240";

        public static Element CreateStairStepBarsFrame(
            in Element host,
            in Curve curve,
            int rebarDiameter,
            int rebarCover,
            int barsStepHorizont,
            int barsStepVert,
            XYZ toBottomDir,
            XYZ toStepDir)
        {
            Document doc = host.Document;

            Curve cCorner = CreateStairStepCornerBarCurve(
                curve,
                rebarDiameter,
                rebarCover,
                toStepDir);

            var anglePlane = GetAnglePlane(host);
            if (anglePlane is null)
            {
                return null;
            }
            var barsVertSideOffset = GetVerticalAngleBarsSideOffset(cCorner, barsStepVert);
            var cAngle = GetStairStepAngleBarCurves(
                cCorner,
                anglePlane,
                rebarDiameter,
                barsVertSideOffset,
                toStepDir);

            Rebar barX = null;
            Rebar barY = null;
            Rebar barL = null;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Каркас ступени");
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


                trans.Commit();
            }
            return barX;
        }

        private static double GetVerticalAngleBarsSideOffset(
            in Curve cornerCurve,
            int barsStepVert)
        {
            return cornerCurve.Length * SharedValues.FootToMillimeters % barsStepVert / 2;
        }

        /// <summary>
        /// Возвращает наклонную плоскость с ниабольшей площадью
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Поверхность стены, или null, если что-то пошло не так</returns>
        private static PlanarFace GetAnglePlane(in Element element)
        {
            Solid elementSolid = null;
            GeometryElement geomElem = element.get_Geometry(new Options()
            {
                ComputeReferences = true
            });
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        elementSolid = solid;
                        break;
                    }
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                elementSolid = solid;
                                break;
                            }
                        }
                    }
                }
            }

            if (elementSolid == null) return null;
            var faces = elementSolid.Faces;
            PlanarFace elementPlanarFace = null;
            foreach (var face in faces)
            {

                if ((face is PlanarFace face1)
                    && face1.IsAngle()
                    && ((elementPlanarFace is null) || (face1.Area >= elementPlanarFace.Area)))
                {
                    elementPlanarFace = face1;
                }
            }
            return elementPlanarFace;
        }

        /// <summary>
        /// Создает эскиз углового горизонтального стержня каркаса ступени
        /// </summary>
        /// <param name="curve">Ребро ступени</param>
        /// <param name="rebarDiameter">Диаметр стержня каркаса ступени</param>
        /// <param name="rebarCover">Защитный слой стержней каркаса ступени</param>
        /// <param name="toStepDir">Вектор направления движения вверх по лестнице</param>
        /// <returns>Эскиз стержня с учетом всех отступов</returns>
        private static Curve CreateStairStepCornerBarCurve(
            in Curve curve,
            int rebarDiameter,
            int rebarCover,
            XYZ toStepDir
            )
        {
            double cCornerCenterOffset = (rebarCover + rebarDiameter * 1.5) / SharedValues.FootToMillimeters;
            XYZ cCornerToBottomV = XYZ.BasisZ
                .Negate()
                .Multiply(cCornerCenterOffset);
            XYZ cCornerToStepV = toStepDir
                .Normalize()
                .Multiply(cCornerCenterOffset);

            XYZ cCornerStartOffsetV = (curve as Line)
                .Direction
                .Normalize()
                .Multiply(_rebarCoverEnd / SharedValues.FootToMillimeters);

            XYZ cCornerEndOffsetV = (curve as Line)
                .Direction
                .Negate()
                .Normalize()
                .Multiply(_rebarCoverEnd / SharedValues.FootToMillimeters);

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
        /// <param name="toStepDir">Вектор направления движения вверх по лестнице</param>
        /// <returns>Список линий, составляющих эскиз Г-стержня</returns>
        private static IList<Curve> GetStairStepAngleBarCurves(
            in Curve cornerBarCurve,
            in PlanarFace anglePlane,
            int rebarDiameter,
            double barsVertSideOffset,
            XYZ toStepDir
            )
        {
            double angleStair = anglePlane.ComputeNormal(new UV()).AngleTo(XYZ.BasisZ);
            var test = anglePlane.ComputeNormal(new UV());
            XYZ test1 = new XYZ(test.X, test.Y, 0);
            // Расстояние в футах от центра угла Г-стержня
            // до плоскости нижнего защитного слоя наклонной плоскости марша
            double distanceToCoverPlane =
                Math.Abs(anglePlane.DistanceTo(cornerBarCurve.GetEndPoint(0), out _)) +
                (rebarDiameter - _rebarCoverEnd) / SharedValues.FootToMillimeters;

            // Длина вертикального сегмента в футах
            double lengthVert = distanceToCoverPlane / Math.Abs(Math.Cos(angleStair));
            // Жлина горизонтального сегмента в футах
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

    }
}
