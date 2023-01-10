using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.ViewModels.AR.LintelsManager;
using MS.GUI.Windows.AR.LintelsManager;
using MS.RevitCommands.AR.DTO;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.AR.Models.Lintels;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Extensions;
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
        /// <summary>
        /// Название самейства перемычки из уголков  =  "ADSK_Обобщенная модель_Перемычка из уголков"
        /// </summary>
        private readonly string _familyLintelAngleName = "ADSK_Обобщенная модель_Перемычка из уголков";

        /// <summary>
        /// Название семейства перемычки из брусков =  "ADSK_Обобщенная модель_Перемычка составная"
        /// </summary>
        private readonly string _familyLintelBlockName = "ADSK_Обобщенная модель_Перемычка составная";

        /// <summary>
        /// Название семейства перемычки из стержней =  "PGS_Перемычка_Стержни_v0.1"
        /// </summary>
        private readonly string _familyLintelBarName = "PGS_Перемычка_Стержни_v0.1";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            View3D view3d = DocMethods.GetView3Default(doc);
            if (view3d is null)
            {
                MessageBox.Show("Не найден {3D} вид по умолчанию!", "Ошибка");
                return Result.Cancelled;
            }

            (List<OpeningDto> openings, bool updateLintelsLocations) = ShowLintelsManagerWindow(doc, view3d);
            if (openings is null)
            {
                return Result.Cancelled;
            }

            EditLintels(doc, openings, updateLintelsLocations);



            return Result.Succeeded;
        }

        private void EditLintels(in Document doc, in List<OpeningDto> openings, bool updateLintelsLocations)
        {
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start($"Корректировка перемычек");
                foreach (var opening in openings)
                {
                    try
                    {
                        //Описание алгоритма корректировки перемычек
                        //
                        //Перемычка не была назначена до команды (ExistLintelId < 0)		
                        //			Перемычка была назначена    (Lintel is not null)
                        //			                             => create Lintel
                        //		или	Перемычка не была назначена (Lintel is null)
                        //		                                 => continue
                        //
                        //Перемычка была назначена до команды    (ExistLintelId > 0)		
                        //			Перемычка была удалена и не назначена (ExistLintelDeleted && Lintel is null)
                        //			                             => delete Lintel;
                        //		или	Перемычка была удалена и назначена	  (ExistLintelDeleted && Lintel is not null)
                        //		                                 => delete Lintel then create Lintel;
                        //				
                        //				
                        //  Или		Перемычка не была удалена и не была изменена	!ExistLintelDeleted
                        //                                       => check and correct only location if updateLintelsLocations
                        //		или Перемычка не была удалена и была изменена		!ExistLintelDeleted
                        //		                                 => edit Lintel and correct location if updateLintelsLocations;
                        if (opening.ExistLintelId <= 0)
                        {
                            if (opening.Lintel is null)
                            {
                                continue;
                            }
                            else
                            {
                                // создать перемычку с заданными параметрами
                                CreateLintel(doc, opening);
                            }
                        }
                        else
                        {
                            if (opening.ExistLintelDeleted)
                            {
                                // удалить перемычку по Id
                                doc.Delete(new ElementId(opening.ExistLintelId));

                                if (!(opening.Lintel is null))
                                {
                                    // cоздать перемычку с заданными параметрами
                                    CreateLintel(doc, opening);
                                }
                            }
                            else
                            {
                                // скорректировать значения параметров существующей перемычки по Id и,
                                // если updateLintelsLocations, то скорректировать расположение перемычки
                                UpdateLintel(doc, opening, updateLintelsLocations);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // добавить заполнение списка полученных ошибок и их последующий вывод
                    }

                }
                trans.Commit();
            }
        }

        /// <summary>
        /// Показывает диалоговое окно менеджера перемычек и возвращает список проемов с перемычками, назначенными пользователем
        /// </summary>
        /// <param name="doc">Документ, в котором запускается менеджер перемычек</param>
        /// <param name="view3d">3D вид по умолчанию</param>
        /// <returns>Список проемов, или null, если команда отменена</returns>
        private (List<OpeningDto> openings, bool updateLintelsLocations) ShowLintelsManagerWindow(in Document doc, in View3D view3d)
        {
            List<OpeningDto> openings = GetOpeningDtos(doc, view3d);

            LintelsManagerViewModel vm = new LintelsManagerViewModel(openings);
            var ui = new LintelsManagerView()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = vm
            };
            ui.ShowDialog();
            if (ui.DialogResult == true)
            {
                var viewModel = ui.DataContext as LintelsManagerViewModel;
                return (viewModel.Openings.ToList(), viewModel.UpdateLintelsLocation);
            }
            else
            {
                return (null, false);
            }
        }

        /// <summary>
        /// Обновляет значения параметров уже размещенной в проекте перемычки
        /// </summary>
        /// <param name="doc">Документ, в котором размещена перемычка</param>
        /// <param name="openingDto">Проем, к которому привязана перемычка</param>
        /// <param name="updateLintelsLocations">Обновлять расположение перемычек в соответствии с расположением проема, или нет</param>
        private void UpdateLintel(in Document doc, in OpeningDto openingDto, bool updateLintelsLocations)
        {
            var lintel = doc.GetElement(new ElementId(openingDto.ExistLintelId)) as FamilyInstance;
            if (lintel is null)
            {
                throw new NullReferenceException(nameof(openingDto));
            }
            else
            {
                var parLintelDictionary = openingDto.Lintel.GetParametersValues();
                foreach (var keyValuePair in parLintelDictionary)
                {
                    lintel.SetParameterValueByName(keyValuePair.Key, (object)keyValuePair.Value);
                }
                var parOpeningDictionary = openingDto.GetParametersValues();
                foreach (var keyValuePair in parOpeningDictionary)
                {
                    lintel.SetParameterValueByName(keyValuePair.Key, (object)keyValuePair.Value);
                }
                if (updateLintelsLocations)
                {
                    XYZ openingLocation = openingDto.Location;
                    XYZ lintelLocation = (lintel.Location as LocationPoint).Point;
                    if (!lintelLocation.IsAlmostEqualTo(openingLocation))
                    {
                        var openingHost = openingDto.HostWallId;
                        var lintelHost = lintel.Host.Id;
                        if (!openingHost.Equals(lintelHost))
                        {
                            Task.Run(() => MessageBox.Show("Нельзя автоматически поменять основу! " +
                                "Сначала нужно вручную перенести перемычку в новую основу и потом снова запустить команду!" +
                                $"\nId перемычки: {lintel.Id}"));
                        }
                        else
                        {
                            ElementTransformUtils.MoveElement(doc, lintel.Id, openingLocation - lintelLocation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Создает заданную перемычку по проему
        /// </summary>
        /// <param name="doc">Документ, в котором создается перемычка</param>
        /// <param name="openingDto">Проем, для которого создается перемычка</param>
        private void CreateLintel(in Document doc, in OpeningDto openingDto)
        {
            string famName = string.Empty;
            switch (openingDto.Lintel.LintelType)
            {
                case Enums.LintelType.Bar:
                    famName = _familyLintelBarName;
                    break;
                case Enums.LintelType.Block:
                    famName = _familyLintelBlockName;
                    break;
                case Enums.LintelType.Angle:
                    famName = _familyLintelAngleName;
                    break;
                default:
                    return;
            }

            FamilySymbol fSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .FirstOrDefault(f => f.Name.Equals(famName)) as FamilySymbol;

            var lintel = doc.Create.NewFamilyInstance(
                openingDto.Location,
                fSymb,
                doc.GetElement(new ElementId(openingDto.HostWallId)),
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            var parLintelDictionary = openingDto.Lintel.GetParametersValues();
            foreach (var keyValuePair in parLintelDictionary)
            {
                lintel.SetParameterValueByName(keyValuePair.Key, (object)keyValuePair.Value);
            }

            var parOpeningDictionary = openingDto.GetParametersValues();
            foreach (var keyValuePair in parOpeningDictionary)
            {
                lintel.SetParameterValueByName(keyValuePair.Key, (object)keyValuePair.Value);
            }

            lintel.get_Parameter(SharedParams.PGS_Guid).Set(openingDto.Guid.ToString());
        }


        /// <summary>
        /// Возвращает список проемов с перемычками из текущего документа
        /// </summary>
        /// <param name="doc">Текущий документ</param>
        /// <param name="view3d">3D вид по умолчанию</param>
        /// <returns>Список проемов с перемычками, уже созданными в проекте</returns>
        private List<OpeningDto> GetOpeningDtos(in Document doc, in View3D view3d)
        {
            List<OpeningDto> openingDtos = new List<OpeningDto>();

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
                .Where(wall => !(wall.CurtainGrid is null))
                .Where(wall => (wall.Location as LocationCurve).Curve is Line line)
                .Cast<Element>()
                .ToList();
            openings.AddRange(curtainWalls);

            using (Transaction setGuidsTrans = new Transaction(doc))
            {
                setGuidsTrans.Start("Назначение Guid проемам");
                foreach (Element opening in openings)
                {
                    var hostWall = GetHostElement(doc, view3d, opening);
                    if (hostWall is null)
                    {
                        continue;
                    }
                    Guid guid = Guid.NewGuid();
                    var guidParam = opening.get_Parameter(SharedParams.PGS_Guid);
                    var guidString = guidParam.AsValueString() ?? string.Empty;
                    bool searchLintel = true;
                    if ((guidString.Length != Guid.Empty.ToString().Length) || !Guid.TryParse(guidString, out guid))
                    {
                        guid = Guid.NewGuid();
                        guidParam.Set(guid.ToString());
                        searchLintel = false;
                    }
                    var lintel = searchLintel ? GetLintel(doc, guid) : null;
                    (double height, double width) = GetOpeningWidthAndHeight(opening);
                    var wallThick = GetWallThick(hostWall as Wall);
                    var wallHeightOverOpening = GetWallHeightOverOpening(opening, hostWall, view3d);
                    (double distanceLeft, double distanceRight) = GetOpeningSideDistances(opening, hostWall as Wall);
                    string wallMaterial = GetWallMaterial(hostWall as Wall);
                    string levelName = GetElementLevel(opening);
                    XYZ openingLocation = GetLocationPoint(opening);

                    OpeningDto openingDto = new OpeningDto(
                        guid,
                        width,
                        height,
                        wallThick,
                        wallHeightOverOpening,
                        distanceRight,
                        distanceLeft,
                        wallMaterial,
                        levelName,
                        hostWall.Id.IntegerValue,
                        opening.Id.IntegerValue,
                        openingLocation,
                        lintel)
                    {
                        Mark = lintel?.Mark
                    };
                    openingDtos.Add(openingDto);
                }
                setGuidsTrans.Commit();
            }
            return openingDtos;
        }

        /// <summary>
        /// Возвращает название уровня, на котором расположен элемент
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <returns>Название уровня</returns>
        private string GetElementLevel(in Element element)
        {
            var doc = element.Document;
            return doc.GetElement(element.LevelId).Name;
        }

        /// <summary>
        /// Возаращает модель перемычки по заданному экземпляру семейства перемычки
        /// </summary>
        /// <param name="doc">Документ, в котором происходит поиск</param>
        /// <param name="guid">Значение PGS_Guid параметра перемычки</param>
        /// <returns>Модель перемычки</returns>
        private Lintel GetLintel(in Document doc, Guid guid)
        {
            var lintel = GetLintelByGuid(doc, guid.ToString());
            if (lintel is null)
            {
                return null;
            }
            string familyName = lintel.Symbol.Name;
            switch (familyName)
            {
                case "ADSK_Обобщенная модель_Перемычка составная":
                    return new BlockLintel(guid, lintel.Id.IntegerValue)
                    {
                        BlockType_1 = lintel.LookupParameter("Тип 1-го элемента").AsValueString(),
                        BlockType_2 = lintel.LookupParameter("Тип 2-го элемента").AsValueString(),
                        BlockType_3 = lintel.LookupParameter("Тип 3-го элемента").AsValueString(),
                        BlockType_4 = lintel.LookupParameter("Тип 4-го элемента").AsValueString(),
                        BlockType_5 = lintel.LookupParameter("Тип 5-го элемента").AsValueString(),
                        BlockType_6 = lintel.LookupParameter("Тип 6-го элемента").AsValueString(),
                        Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString()
                    };
                case "PGS_Перемычка_Стержни_v0.1":
                    return new BarLintel(guid, lintel.Id.IntegerValue)
                    {
                        BarsDiameter = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("ADSK_Размер_Диаметр").AsDouble(), UnitTypeId.Millimeters),
                        BarsStep = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("Шаг стержней").AsDouble(), UnitTypeId.Millimeters),
                        SupportLeft = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("Опирание слева").AsDouble(), UnitTypeId.Millimeters),
                        SupportRight = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("Опирание справа").AsDouble(), UnitTypeId.Millimeters),
                        Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString()
                    };
                case "ADSK_Обобщенная модель_Перемычка из уголков":
                    return new AngleLintel(guid, lintel.Id.IntegerValue)
                    {
                        AngleExterior = lintel.LookupParameter("Уголок для облицовки").AsValueString(),
                        AngleMain = lintel.LookupParameter("Внутренний уголок").AsValueString(),
                        AngleSupport = lintel.LookupParameter("Опорные уголки").AsValueString(),
                        Stripe = lintel.LookupParameter("Полоса").AsValueString(),
                        StripeStep = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("Полоса_Шаг").AsDouble(), UnitTypeId.Millimeters),
                        SupportLeft = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("уголок_опирание_1").AsDouble(), UnitTypeId.Millimeters),
                        SupportRight = UnitUtils.ConvertFromInternalUnits(lintel.LookupParameter("уголок_опирание_2").AsDouble(), UnitTypeId.Millimeters),
                        Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString()
                    };
            }
            return null;
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
        private (double height, double width) GetOpeningWidthAndHeight(in Element opening)
        {
            (double height, double width) = GeometryMethods.GetWidthAndHeightOfElement(opening);
            if (opening is Wall wall)
            {
                height += wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
            }
            else
            {
                height += opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble();
            }
            var heightMM = Math.Round(UnitUtils.ConvertFromInternalUnits(height, UnitTypeId.Millimeters));
            var widthMM = Math.Round(UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters));
            return (heightMM, widthMM);
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
        private string GetWallMaterial(in Wall hostWall)
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
        private (double leftDistance, double rightDistance) GetOpeningSideDistances(in Element opening, in Wall hostWall)
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
                .FirstOrDefault(e =>
                {
                    var parameter = e.get_Parameter(SharedParams.PGS_Guid);
                    bool hasValue = parameter.HasValue;
                    return hasValue && parameter.AsValueString().Equals(guid);
                }) as FamilyInstance;
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
