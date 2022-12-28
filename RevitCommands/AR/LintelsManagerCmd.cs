using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.ViewModels.AR.LintelsManager;
using MS.GUI.Windows.AR.LintelsManager;
using MS.RevitCommands.AR.DTO;
using MS.Utilites;
using MS.Utilites.SelectionFilters;
using MS.Utilites.WorkWith;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.RevitCommands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LintelsManagerCmd : IExternalCommand
    {
        private const string _parGuid = "PGS_GUID";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;




            LintelsManagerViewModel vm = new LintelsManagerViewModel();

            var ui = new LintelsManagerView()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm
            };
            ui.ShowDialog();
            return Result.Succeeded;


            Element opening = GetOpeningTest(uidoc);

            if (opening is null)
            {
                return Result.Succeeded;
            }

            View3D view3d = DocMethods.GetView3Default(doc);
            if (view3d is null)
            {
                MessageBox.Show("Не найден {3D} вид по умолчанию!", "Ошибка");
                return Result.Cancelled;
            }

            var hostWall = GetHostElement(doc, view3d, opening);
            if (!(hostWall is null))
            {
                var t = new List<ElementId>() { hostWall.Id };
                if (t.Count > 0)
                {
                    uidoc.Selection.SetElementIds(new List<ElementId>() { hostWall?.Id });
                }
            }

            var t1 = GetOpeningWidth(opening);
            var t2 = GetWallThick(hostWall as Wall);
            (var left, var right) = GetOpeningSidesDistances(opening, hostWall as Wall);
            var t5 = GetWallHeightOverOpening(opening, hostWall, view3d);
            var t6 = GetWallName(hostWall as Wall);

            Task.Run(() => MessageBox.Show($"Проем {opening.Id}\nСлева: {left}\nСправа: {right}"));
            //using (Transaction test = new Transaction(doc))
            //{
            //    test.Start("test");
            //    var curve = Line.CreateBound(GetLocationPoint(opening), GetLocationPoint(opening) + GetElementDirection(opening)) as Curve;
            //    var plane = SketchPlane.Create(doc, opening.LevelId);
            //    var twet = doc.Create.NewModelCurve(curve, plane);
            //    test.Commit();
            //}


            return Result.Succeeded;
            var guid = Guid.NewGuid().ToString();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Создание перемычек");
                //opening.LookupParameter("PGS_GUID").Set(guid);

                string openingGuid = opening.LookupParameter(_parGuid).AsValueString();
                var lintel = GetLintelByGuid(doc, openingGuid);
                if (lintel is null)
                {
                    opening.LookupParameter(_parGuid).ClearValue();
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
                //lintel.LookupParameter(_parGuid).Set(guid);
                trans.Commit();
            }

            return Result.Succeeded;
        }

        private Dictionary<Guid, OpeningDto> GetOpeningDtos(in Document doc, in View3D view3d)
        {
            Dictionary<Guid, OpeningDto> openingDtos = new Dictionary<Guid, OpeningDto>();

            BuiltInCategory[] famInstCategories = new BuiltInCategory[2] { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows };
            var multiFamInstFilter = new ElementMulticategoryFilter(famInstCategories);
            var openings = new FilteredElementCollector(doc)
                .WherePasses(multiFamInstFilter)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(inst => (inst.Host is Wall wall) && (wall.CurtainGrid is null))
                .Cast<Element>()
                .ToList();
            var curtainWalls = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .Where(wall => wall.CurtainGrid is null)
                .Cast<Element>()
                .ToList();
            openings.AddRange(curtainWalls);

            using (Transaction setGuidsTrans = new Transaction(doc))
            {
                setGuidsTrans.Start("Назначение Guid проемам");
                foreach (Element opening in openings)
                {
                    Guid guid = Guid.NewGuid();
                    var guidParam = opening.LookupParameter(_parGuid);
                    var guidString = guidParam.AsValueString();
                    if (!(guidString.Length != Guid.Empty.ToString().Length) && !Guid.TryParse(guidString, out guid))
                    {
                        guid = Guid.NewGuid();
                        guidParam.Set(guid.ToString());
                    }
                    var hostWall = GetHostElement(doc, view3d, opening);
                    var width = GetOpeningWidth(opening);
                    var wallThick = GetWallThick(hostWall as Wall);
                    var wallHeightOverOpening = GetWallHeightOverOpening(opening, hostWall, view3d);


                }
                setGuidsTrans.Commit();
            }
            return openingDtos;
        }


        /// <summary>
        /// Возвращает элемент, выбранный пользователем
        /// </summary>
        /// <guidParam name="uidoc"></guidParam>
        /// <returns></returns>
        private Element GetOpeningTest(in UIDocument uidoc)
        {
            SelectionFilterOpenings filter = new SelectionFilterOpenings();
            Element opening = null;
            try
            {
                Reference openingRef = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    filter,
                    "Выберите проем");
                opening = uidoc.Document.GetElement(openingRef);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбрать элементы",
                    "Ошибка");
                return null;
            }
            return opening;
        }

        /// <summary>
        /// Возвращает точку размещения элемента, являющегося стеной или семейством, размещаемым по 1 точке.
        /// Если элемент - стена, возвращается центр осевой линии стены.
        /// Если элемент - экземпляр семейства, то возвращается точка расположения семейства.
        /// </summary>
        /// <guidParam name="elem">Элемент, расположение которого нужно получить. Wall или FamilyInstance по 1 точке.</guidParam>
        /// <returns>Точка размещения или null, если условия для типа элемента не соблюдены</returns>
        private XYZ GetLocationPoint(in Element elem)
        {
            if (elem is Wall wall)
            {
                var curve = (wall.Location as LocationCurve).Curve;
                return (curve.GetEndPoint(1) + curve.GetEndPoint(0)) / 2;
            }
            else if (elem is FamilyInstance inst)
            {
                return (inst.Location as LocationPoint).Point;
            }
            else
            {
                throw new ArgumentException(nameof(elem));
            }
        }

        /// <summary>
        /// Возвращает ширину проема в мм
        /// </summary>
        /// <guidParam name="opening">Проем, сделанный семейством окна или двери, или витражная стена</guidParam>
        /// <returns>Ширина проема в мм</returns>
        private double GetOpeningWidth(in Element opening)
        {
            (_, double width) = GeometryMethods.GetWidthAndHeightOfElement(opening);
            return Math.Round(UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters));
        }

        /// <summary>
        /// Возвращает толщину стены в мм
        /// </summary>
        /// <guidParam name="hostWall">Хост стена проема</guidParam>
        /// <returns>Толщина стены в мм</returns>
        private double GetWallThick(in Wall hostWall)
        {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(hostWall.WallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble(), UnitTypeId.Millimeters));
        }

        /// <summary>
        /// Возвращает название типоразмера стены
        /// </summary>
        /// <guidParam name="hostWall">Хост стена проема</guidParam>
        /// <returns>Название типоразмера стены</returns>
        private string GetWallName(in Wall hostWall)
        {
            return hostWall.Name;
        }

        /// <summary>
        /// Возвращает высоту стены над проемом в мм
        /// </summary>
        /// <guidParam name="opening">Проем, сделанный окном, дверью или витражом</guidParam>
        /// <guidParam name="hostWall">Хост стена проема</guidParam>
        /// <guidParam name="view3d">{3D} вид по умолчанию</guidParam>
        /// <returns>Высота участка стены над проемом в мм</returns>
        private double GetWallHeightOverOpening(in Element opening, in Element hostWall, in View3D view3d)
        {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(hostWall.get_BoundingBox(view3d).Max.Z - opening.get_BoundingBox(view3d).Max.Z, UnitTypeId.Millimeters));
        }

        /// <summary>
        /// Возвращает расстояния от граней проема до торцов станы
        /// </summary>
        /// <guidParam name="opening">Проем</guidParam>
        /// <guidParam name="hostWall">Хост стена проема</guidParam>
        /// <returns>Кортеж расстояний слева и справа от граней проема до торцов стены</returns>
        private (double leftDistance, double rightDistance) GetOpeningSidesDistances(in Element opening, in Wall hostWall)
        {
            (_, double width) = GeometryMethods.GetWidthAndHeightOfElement(opening);
            XYZ openingLocation = GetLocationPoint(opening);
            XYZ openingDirection = GetElementDirection(opening);
            if ((hostWall.Location as LocationCurve).Curve is Line line)
            {
                XYZ lineDirection = line.Direction;
                XYZ lineStart = line.GetEndPoint(0);
                XYZ lineEnd = line.GetEndPoint(1);
                XYZ openingLocationOnLevel = new XYZ(openingLocation.X, openingLocation.Y, lineStart.Z);
                if (lineDirection.CrossProduct(openingDirection).IsAlmostEqualTo(XYZ.BasisZ))
                {
                    return (Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineStart) - (width / 2), UnitTypeId.Millimeters)),
                        Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineEnd) - (width / 2), UnitTypeId.Millimeters)));
                }
                else
                {
                    return (Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineEnd) - (width / 2), UnitTypeId.Millimeters)),
                        Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineStart) - (width / 2), UnitTypeId.Millimeters)));
                }
            }
            else
            {
                return (0, 0);
            }
        }

        private XYZ GetElementDirection(in Element opening)
        {
            if (opening is FamilyInstance inst)
            {
                return inst.FacingOrientation;
            }
            else if (opening is Wall wall)
            {
                return wall.Orientation;
            }
            else throw new ArgumentException(nameof(opening));
        }

        /// <summary>
        /// Получает перемычку по Guid
        /// </summary>
        /// <guidParam name="doc">Документ, в котором происходит поиск</guidParam>
        /// <guidParam name="guid">Значение параметра "PGS_GUID" перемычки</guidParam>
        /// <returns>Найденная перемычка или null</returns>
        private FamilyInstance GetLintelByGuid(in Document doc, string guid)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType()
                .Where(e => e.LookupParameter(_parGuid).HasValue)
                .FirstOrDefault(l => l.LookupParameter(_parGuid).AsValueString().Equals(guid)) as FamilyInstance;
        }

        /// <summary>
        /// Возвращает Хост элемента
        /// </summary>
        /// <guidParam name="doc">Документ, в котором расположен вложенный в стену элемент</guidParam>
        /// <guidParam name="view3d">3D вид по умолчанию</guidParam>
        /// <guidParam name="embeddedElem">Вложенный в стену элемент. Экземпляр семейства или витраж</guidParam>
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
        /// Получает стену, в которой расположен витраж
        /// </summary>
        /// <guidParam name="doc">Документ, в котором находится стена</guidParam>
        /// <guidParam name="view3d">3D вид для обработки геометрии</guidParam>
        /// <guidParam name="curtainWall">Витражная стена</guidParam>
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
