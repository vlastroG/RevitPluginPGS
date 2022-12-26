using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Utilites.WorkWith;
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

            Element opening = doc.GetElement(uidoc.Selection.GetElementIds()?.FirstOrDefault());

            if (opening is null)
            {
                return Result.Succeeded;
            }

            View3D view3d = WorkWithDoc.GetView3Default(doc);
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

        /// <summary>
        /// Возвращает точку размещения элемента, являющегося стеной или семейством, размещаемым по 1 точке
        /// </summary>
        /// <param name="elem">Элемент, расположение которого нужно получить. Wall или FamilyInstance по 1 точке.</param>
        /// <returns>Точка размещения или null, если условия для типа элемента не соблюдены</returns>
        private XYZ GetLocationPoint(in Element elem)
        {
            if (elem is Wall wall)
            {
                return ((wall.Location as LocationCurve).Curve as Line).Origin;
            }
            else if (elem is FamilyInstance inst)
            {
                return (inst.Location as LocationPoint).Point;
            }
            return null;
        }

        /// <summary>
        /// Получает перемычку по Guid
        /// </summary>
        /// <param name="doc">Документ, в котором происходит поиск</param>
        /// <param name="guid">Значение параметра "PGS_GUID" перемычки</param>
        /// <returns>Найденная перемычка или null</returns>
        private FamilyInstance GetLintelByGuid(in Document doc, string guid)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Where(e => e.LookupParameter(_parGuidName).HasValue)
                .FirstOrDefault(l => l.LookupParameter(_parGuidName).AsValueString().Equals(guid)) as FamilyInstance;
        }

        /// <summary>
        /// Возвращает Хост элемента
        /// </summary>
        /// <param name="doc">Документ, в котором расположен вложенный в стену элемент</param>
        /// <param name="view3d">3D вид по умолчанию</param>
        /// <param name="embeddedElem">Вложенный в стену элемент. Экземпляр семейства или витраж</param>
        /// <returns>Хост стена вложенного элемента</returns>
        private Element GetHostElement(in Document doc, in View3D view3d, in Element embeddedElem)
        {
            if (embeddedElem is FamilyInstance inst)
            {
                return inst.Host;
            }
            else if (embeddedElem is Wall wall)
            {
                if (!(wall.CurtainGrid is null))
                {
                    return GetHostOfCurtainWall(doc, view3d, wall);
                }
            }
            return null;
        }

        /// <summary>
        /// Получает стену, в которой расположеy витраж
        /// </summary>
        /// <param name="doc">Документ, в котором находится стена</param>
        /// <param name="view3d">3D вид для обработки геометрии</param>
        /// <param name="curtainWall">Витражная стена</param>
        /// <returns>Стена, в которой расположен витраж</returns>
        private Element GetHostOfCurtainWall(in Document doc, in View3D view3d, in Wall curtainWall)
        {
            // Отметка верха витража относительно зависимости снизу (базового уровня стены) + 1 фут
            var maxProximity = curtainWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble() +
                curtainWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble()
                + 1;

            ElementFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ReferenceIntersector referenceIntersector = new ReferenceIntersector(
                wallFilter,
                FindReferenceTarget.Element,
                view3d)
            {
                FindReferencesInRevitLinks = false
            };
            XYZ direction = XYZ.BasisZ;
            XYZ startPoint = GetLocationPoint(curtainWall);
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
                    > maxProximity)
                {
                    break;
                }
            }
            return null;
        }
    }
}
