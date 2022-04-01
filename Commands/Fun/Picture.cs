using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace MS.Commands.Fun
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class Picture : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;

            Document doc = uiDoc.Document;

            View activeView = doc.ActiveView;

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Draw some picks");

                doc.Create.NewDetailCurveArray(activeView, Dick());

                transaction.Commit();
            }

            return Result.Succeeded;
        }

        private CurveArray Dick()
        {
            CurveArray curveArray = new CurveArray();

            curveArray.Append(Arc.Create(new XYZ(-100, 0, 0), 100, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY));
            curveArray.Append(Arc.Create(new XYZ(100, 0, 0), 100, 0, Math.PI * 2, XYZ.BasisX, XYZ.BasisY));
            curveArray.Append(Arc.Create(new XYZ(0, 600, 0), 100, 0, Math.PI, XYZ.BasisX, XYZ.BasisY));

            curveArray.Append(Line.CreateBound(new XYZ(-100,70,0), new XYZ(-100,600,0)));
            curveArray.Append(Line.CreateBound(new XYZ(100,70,0), new XYZ(100,600,0)));

            return curveArray;
        }
    }
}
