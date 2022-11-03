using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.KR.Services
{
    public static class BarsCreation
    {
        private static readonly int _rebarCoverEnd = 20;

        private static readonly string _rbtD6A240 = "ø6 A240";

        private static Curve CreateStairStepCornerBar(
            in Curve curve,
            int rebarDiameter,
            int rebarCover,
            XYZ toBottomDir,
            XYZ toStepDir
            )
        {
            double cCornerCenterOffset = (rebarCover + rebarDiameter * 1.5) / SharedValues.FootToMillimeters;
            XYZ cCornerToBottomV = toBottomDir
                .Normalize()
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

        public static Element CreateStairStepBarsFrame(
            in Element host,
            in Curve curve,
            int rebarDiameter,
            int rebarCover,
            int barsStepHorizont,
            int barsStepVert,
            int barsVertSideOffset,
            XYZ toBottomDir,
            XYZ toStepDir)
        {
            Document doc = host.Document;

            Curve cCorner = CreateStairStepCornerBar(
                curve,
                rebarDiameter,
                rebarCover,
                toBottomDir,
                toStepDir);

            Rebar barX = null;
            Rebar barY = null;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Каркас ступени");
                // Стержни проступи
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

                trans.Commit();
            }
            return barX;
        }
    }
}
