using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LintelsCreationCmd : IExternalCommand
    {
        private const string _parGuidName = "PGS_GUID";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            var opening = doc.GetElement(uidoc.Selection.GetElementIds()?.FirstOrDefault());

            if (opening is null)
            {
                return Result.Succeeded;
            }

            View3D view3d = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault<View3D>(
                  e => e.Name.Equals("{3D}"));
            if (view3d is null)
            {
                Task.Run(() => MessageBox.Show("Не найден {3D} вид по умолчанию!", "Ошибка"));
                return Result.Cancelled;
            }

            var hostWall = GetHostOfCurtainWall(doc, view3d, opening as Wall);
            if (!(hostWall is null))
            {
                var t = new List<ElementId>() { hostWall.Id };
                if (t.Count > 0)
                {
                    uidoc.Selection.SetElementIds(new List<ElementId>() { hostWall?.Id });
                }
            }
            return Result.Succeeded;
            var guid = Guid.NewGuid().ToString();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание перемычек");
                //opening.LookupParameter("PGS_GUID").Set(guid);

                string openingGuid = opening.LookupParameter(_parGuidName).AsValueString();
                var lintel = GetLintelByGuid(doc, openingGuid);
                if (lintel is null)
                {
                    opening.LookupParameter(_parGuidName).ClearValue();
                }
                else
                {
                    XYZ openingLocation = (opening.Location as LocationPoint).Point;
                    XYZ lintelLocation = (lintel.Location as LocationPoint).Point;
                    if (!lintelLocation.IsAlmostEqualTo(openingLocation))
                    {
                        var openingHost = (opening as FamilyInstance).Host.Id;
                        var lintelHost = lintel.Host.Id;
                        if (!openingHost.Equals(lintelHost))
                        {
                            Task.Run(() => MessageBox.Show("Нельзя автоматически поменять основу! " +
                                "Сначала нужно вручную перенести перемычку в новую основу и потом снова запустить команду!" +
                                $"\n\nId проема: {opening.Id}" +
                                $"\nId перемычки: {lintel.Id}"));
                        }
                        else
                        {
                            ElementTransformUtils.MoveElement(doc, lintel.Id, openingLocation - lintelLocation);
                        }
                    }
                }


                //XYZ location = (opening.Location as LocationPoint).Point;

                //string famName = "ADSK_Обобщенная модель_Перемычка из уголков";

                //FamilySymbol fSymb = new FilteredElementCollector(doc)
                //    .OfClass(typeof(FamilySymbol))
                //    .FirstOrDefault(f => f.Name.Equals(famName)) as FamilySymbol;

                //var lintel = doc.Create.NewFamilyInstance(location, fSymb, (opening as FamilyInstance).Host, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                //lintel.LookupParameter(_parGuidName).Set(guid);
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private FamilyInstance GetLintelByGuid(in Document doc, string guid)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Where(e => e.LookupParameter(_parGuidName).HasValue)
                .FirstOrDefault(l => l.LookupParameter(_parGuidName).AsValueString().Equals(guid)) as FamilyInstance;
        }

        private Element GetHostOfCurtainWall(in Document doc, in View3D view3d, in Wall curtainWall)
        {
            ElementFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(
                wallFilter,
                FindReferenceTarget.Element,
                view3d)
            {
                FindReferencesInRevitLinks = false
            };
            XYZ direction = XYZ.BasisZ;
            var wallCurve = (curtainWall.Location as LocationCurve).Curve;
            var wallStart = wallCurve.GetEndPoint(0);
            var wallEnd = wallCurve.GetEndPoint(1);
            XYZ startPoint = (wallStart + wallEnd) / 2;
            var context = referenceIntersector.Find(startPoint, direction);
            var wallsReferences = context.OrderBy(c => c.Proximity).ToList();
            List<Element> elements = new List<Element>();
            for (int i = 0; i < wallsReferences.Count; i++)
            {
                var wall = doc.GetElement(wallsReferences[i].GetReference()) as Wall;
                var isStacked = wall.IsStackedWall;
                var isNotCurtain = wall.CurtainGrid is null;
                if (!isStacked && isNotCurtain)
                {
                    return wall;
                }
                if (wallsReferences[i].Proximity
                    > (curtainWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble() + 1))
                {
                    break;
                }
            }
            return null;
        }
    }
}
