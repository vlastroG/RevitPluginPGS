﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.ViewModels.AR.LintelsManager;
using MS.GUI.Windows.AR.LintelsManager;
using MS.Logging;
using MS.RevitCommands.AR.DTO;
using MS.RevitCommands.AR.DTO.LintelsManager;
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
        /// Менеджер перемычек
        /// </summary>
        public readonly string _commandName = "Менеджер перемычек";

        /// <summary>
        /// Название самейства перемычки из уголков  =  "PGS_Обобщенная модель_Перемычка металлическая"
        /// </summary>
        private readonly string _familyLintelAngleName = "PGS_Обобщенная модель_Перемычка металлическая";

        /// <summary>
        /// Название семейства перемычки из брусков =  "PGS_Обобщенная модель_Перемычка составная"
        /// </summary>
        private readonly string _familyLintelBlockName = "PGS_Обобщенная модель_Перемычка составная";

        /// <summary>
        /// Название семейства перемычки из стержней =  "PGS_Перемычка_Стержни_v0.1"
        /// </summary>
        private readonly string _familyLintelBarName = "PGS_Перемычка_Стержни_v0.1";

        /// <summary>
        /// Типоразмер семейства перемычки из уголков
        /// </summary>
        private FamilySymbol FamilySymbolAngle { get; set; }

        /// <summary>
        /// Типоразмер семейства перемычки из брусков
        /// </summary>
        private FamilySymbol FamilySymbolBlock { get; set; }

        /// <summary>
        /// Типоразмер семейства перемычки из стержней
        /// </summary>
        private FamilySymbol FamilySymbolBar { get; set; }

        /// <summary>
        /// Путь к папке, где расположена сборка
        /// </summary>
        private readonly string _assemblyDir = PathMethods.AssemblyDirectory;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (!ValidateSharedParams(doc))
            {
                return Result.Cancelled;
            }
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            View3D view3d = DocMethods.GetView3Default(doc);
            if (view3d is null)
            {
                MessageBox.Show("Не найден {3D} вид по умолчанию!", "Ошибка");
                return Result.Cancelled;
            }

            if (!LoadFamilies(doc))
            {
                return Result.Cancelled;
            }

            (List<OpeningDto> openings, bool updateLintelsLocations) = ShowLintelsManagerWindow(uidoc, view3d);
            if (openings is null)
            {
                return Result.Cancelled;
            }

            EditLintels(doc, openings, updateLintelsLocations);

            return Result.Succeeded;
        }

        /// <summary>
        /// Загружает семейства перемычек в проект
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private bool LoadFamilies(in Document doc)
        {
            List<string> exceptions = new List<string>();

            try
            {
                FamilySymbolBar = FindOrLoadFamily(doc, _familyLintelBarName);
            }
            catch (Exception e)
            {
                exceptions.Add($"{e.Message}\n{e.StackTrace}");
                exceptions.Add(string.Empty);
            }
            try
            {
                FamilySymbolAngle = FindOrLoadFamily(doc, _familyLintelAngleName);
            }
            catch (Exception e)
            {
                exceptions.Add($"{e.Message}\n{e.StackTrace}");
                exceptions.Add(string.Empty);
            }
            try
            {
                FamilySymbolBlock = FindOrLoadFamily(doc, _familyLintelBlockName);
            }
            catch (Exception e)
            {
                exceptions.Add($"{e.Message}\n{e.StackTrace}");
                exceptions.Add(string.Empty);
            }
            if (exceptions.Count != 0)
            {
                Logger.WriteLog(_commandName, exceptions.ToArray(), true);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Находит семейство перемычки в проекте, или загружает его
        /// </summary>
        /// <param name="doc">Документ, в котором происходит поиск семейства</param>
        /// <param name="familyName">Название семейства</param>
        /// <returns>Типоразмер семейства, найденный, или загруженный в проект</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private FamilySymbol FindOrLoadFamily(in Document doc, string familyName)
        {
            var lintelFamSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == familyName);
            if (lintelFamSymb == null)
            {
                bool isSuccess;
                var @familyPath = _assemblyDir + $@"\EmbeddedFamilies\{familyName}.rfa";
                using (Transaction loadFamily = new Transaction(doc))
                {
                    loadFamily.Start("Загрузка перемычки");
                    isSuccess = doc.LoadFamily(familyPath);
                    loadFamily.Commit();
                }
                if (!isSuccess)
                {
                    throw new InvalidOperationException($"Семейство не загрузилось: {familyPath}");
                }
                lintelFamSymb = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(ft => ft.FamilyName == familyName);
                if (lintelFamSymb == null)
                {
                    throw new InvalidOperationException(
                        "Семейство перемычки загрузилось, но его нельзя найти в проекте!");
                }
            }
            return lintelFamSymb;
        }

        /// <summary>
        /// Логика корректировки параметров перемычек в модели Revit в соответствии с переданными данными
        /// </summary>
        /// <param name="doc">Документ, в котором происходит корректировка перемычек</param>
        /// <param name="openings">Список проемов</param>
        /// <param name="updateLintelsLocations">True, если обновлять расположение перемычек в плане</param>
        private void EditLintels(in Document doc, in List<OpeningDto> openings, bool updateLintelsLocations)
        {
            List<string> exceptions = new List<string>();


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
                    catch (Exception e)
                    {
                        // Логгирование
                        exceptions.Add($"Проем: {opening.ToLongString()}");
                        exceptions.Add($"Информация об ошибкe: {e.Message}\n{e.StackTrace}");
                        exceptions.Add(string.Empty);
                    }
                }
                trans.Commit();
            }
            if (exceptions.Count != 0)
            {
                Logger.WriteLog(_commandName, exceptions.ToArray(), true);
            }

            FamilySymbolAngle = null;
            FamilySymbolBar = null;
            FamilySymbolBlock = null;
        }

        /// <summary>
        /// Показывает диалоговое окно менеджера перемычек и возвращает список проемов с перемычками, назначенными пользователем
        /// </summary>
        /// <param name="uidoc">Документ, в котором запускается менеджер перемычек</param>
        /// <param name="view3d">3D вид по умолчанию</param>
        /// <returns>Список проемов, или null, если команда отменена</returns>
        private (List<OpeningDto> openings, bool updateLintelsLocations) ShowLintelsManagerWindow(in UIDocument uidoc, in View3D view3d)
        {
            List<OpeningDto> openings = GetOpeningDtos(uidoc.Document, view3d);
            List<SimilarOpeningsDto> similarOpeningsDto = GetSimilarOpeningsDtos(openings);
            SimilarOpeningsViewModel similarsVM = new SimilarOpeningsViewModel(similarOpeningsDto);
            var similarsUI = new OpeningsSimilarView()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = similarsVM
            };
            similarsUI.ShowDialog();

            while ((similarsUI.DialogResult == true) && similarsVM.EditSelectedSimilarOpening)
            {
                var instancesVM = new OpeningsInstancesViewModel((similarsVM.SelectedOpening as SimilarOpeningsDto).Openings);
                var instancesUI = new OpeningsInstancesView()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = instancesVM
                };
                instancesUI.ShowDialog();
                while ((instancesUI.DialogResult == true) && instancesVM.GoToSelectedOpeningView3D)
                {
                    GoTo3DView(uidoc, view3d, instancesVM.SelectedOpening);
                    instancesUI = new OpeningsInstancesView()
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        DataContext = instancesVM
                    };
                    instancesUI.ShowDialog();
                }
                similarsUI = new OpeningsSimilarView()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = similarsVM
                };
                similarsUI.ShowDialog();
            }


            if (similarsUI.DialogResult == true)
            {
                return (openings, similarsVM.UpdateLintelsLocation);
            }
            else
            {
                return (null, false);
            }
        }

        /// <summary>
        /// Переход к 3D обрезке заданного проема
        /// </summary>
        /// <param name="uidoc">Документ Revit, в котором расположен проем</param>
        /// <param name="view3d">3D вид, на котором показывается 3D подрезка по заданному проему</param>
        /// <param name="openingDto">Заданный проем</param>
        private void GoTo3DView(in UIDocument uidoc, in View3D view3d, in OpeningDto openingDto)
        {
            var openingId = new ElementId(openingDto.OpeningId);
            uidoc.Selection.SetElementIds(new ElementId[0]);

            using (Transaction goTo3DTrans = new Transaction(uidoc.Document))
            {
                goTo3DTrans.Start($"Переход к элементу {openingId}");
                var selectedOpening = uidoc.Document.GetElement(openingId);
                BoundingBoxXYZ bBox = selectedOpening.get_BoundingBox(view3d);
                XYZ min = bBox.Min;
                XYZ max = bBox.Max;
                XYZ origin = (min + max) / 2;
                bBox.Max = max + (max - origin).Normalize().Multiply(2);
                bBox.Min = min + (min - origin).Normalize().Multiply(2);
                view3d.SetSectionBox(bBox);
                view3d.SetOrientation(new ViewOrientation3D(new XYZ(), new XYZ(1, 1, 2), new XYZ(1, 1, -1)));
                goTo3DTrans.Commit();
            }
            uidoc.ActiveView = view3d;
            var openViews = uidoc.GetOpenUIViews();
            foreach (var openView in openViews)
            {
                if (openView.ViewId == uidoc.ActiveView.Id)
                {
                    openView.ZoomToFit();
                    break;
                }
            }
            uidoc.Selection.SetElementIds(new ElementId[] { openingId });
        }

        /// <summary>
        /// Обновляет значения параметров уже размещенной в проекте перемычки
        /// </summary>
        /// <param name="doc">Документ, в котором размещена перемычка</param>
        /// <param name="openingDto">Проем, к которому привязана перемычка</param>
        /// <param name="updateLintelsLocations">Обновлять расположение перемычек в соответствии с расположением проема, или нет</param>
        private void UpdateLintel(in Document doc, in OpeningDto openingDto, bool updateLintelsLocations)
        {
            if (!(doc.GetElement(new ElementId(openingDto.ExistLintelId)) is FamilyInstance lintel))
            {
                throw new NullReferenceException(nameof(openingDto));
            }
            else
            {
                if (updateLintelsLocations)
                {
                    XYZ openingLocation = openingDto.Location;
                    XYZ lintelLocation = (lintel.Location as LocationPoint).Point;
                    if (!lintelLocation.IsAlmostEqualTo(openingLocation))
                    {
                        var openingHost = openingDto.HostWallId;
                        var lintelHost = lintel.Host.Id.IntegerValue;
                        if (openingHost != lintelHost)
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
                SetLintelParameters(lintel, openingDto);
            }
        }

        /// <summary>
        /// Назначает значения параметрам семейства перемычки по Dto проема
        /// </summary>
        /// <param name="lintel">Экземпляр семейства перемычки</param>
        /// <param name="openingDto">Dto проема</param>
        private void SetLintelParameters(in Element lintel, in OpeningDto openingDto)
        {
            lintel.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(0);
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
        }

        /// <summary>
        /// Создает заданную перемычку по проему
        /// </summary>
        /// <param name="doc">Документ, в котором создается перемычка</param>
        /// <param name="openingDto">Проем, для которого создается перемычка</param>
        private void CreateLintel(in Document doc, in OpeningDto openingDto)
        {
            FamilySymbol fSymb;
            switch (openingDto.Lintel.LintelType)
            {
                case Enums.LintelType.Bar:
                    fSymb = FamilySymbolBar;
                    break;
                case Enums.LintelType.Block:
                    fSymb = FamilySymbolBlock;
                    break;
                case Enums.LintelType.Angle:
                    fSymb = FamilySymbolAngle;
                    break;
                default:
                    return;
            }
            if (!fSymb.IsActive)
            {
                fSymb.Activate();
            }
            var lintel = doc.Create.NewFamilyInstance(
                openingDto.Location,
                fSymb,
                openingDto.Direction,
                doc.GetElement(new ElementId(openingDto.HostWallId)),
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            SetLintelParameters(lintel, openingDto);

            lintel.get_Parameter(SharedParams.PGS_Guid).Set(openingDto.Guid.ToString());
        }

        /// <summary>
        /// Возвращает список Dto проемов с одинаковыми габаритами
        /// </summary>
        /// <param name="openingDtos">Список Dto проемов</param>
        /// <returns></returns>
        private List<SimilarOpeningsDto> GetSimilarOpeningsDtos(in List<OpeningDto> openingDtos)
        {
            var similarOpenings = openingDtos.GroupBy(
                opening => new
                {
                    opening.Width,
                    opening.WallThick,
                    opening.WallHeightOverOpening,
                    opening.DistanceConditionToLeftEnd,
                    opening.DistanceConditionToRightEnd,
                    opening.WallMaterial,
                    opening.Level
                })
                .Select(groupOpenings => new SimilarOpeningsDto(
                    Guid.NewGuid(),
                    groupOpenings.Key.Width,
                    groupOpenings.Key.WallThick,
                    groupOpenings.Key.WallHeightOverOpening,
                    groupOpenings.Key.WallMaterial,
                    groupOpenings.Key.Level,
                    groupOpenings.Key.DistanceConditionToLeftEnd,
                    groupOpenings.Key.DistanceConditionToRightEnd,
                    groupOpenings.ToList()
                 ))
                .ToList();

            foreach (var similarOpening in similarOpenings)
            {
                var openings = similarOpening.Openings;
                var openingFirst = openings?.FirstOrDefault();
                var lintel = openingFirst?.Lintel;
                var mark = openingFirst?.Mark;
                bool lintelsEqual = true;
                bool marksEqual = true;
                foreach (var opening in openings)
                {
                    if (lintel is null)
                    {
                        marksEqual = false;
                        lintelsEqual = false;
                        break;
                    }
                    if (lintel.Equals(opening?.Lintel))
                    {
                        marksEqual &= opening.Mark == mark;
                        continue;
                    }
                    else
                    {
                        lintelsEqual = false;
                        marksEqual = false;
                        break;
                    }
                }
                if (lintelsEqual)
                {
                    similarOpening.Lintel = lintel;
                }
                if (marksEqual)
                {
                    similarOpening.Mark = mark;
                }
            }
            return similarOpenings;
        }

        /// <summary>
        /// Валидация проекта Revit на наличие необходимых общих параметров
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <returns>True, если все общие параметры присутствуют, иначе false</returns>
        private bool ValidateSharedParams(in Document doc)
        {
            Guid[] _sharedParamsForElements = new Guid[] {
            SharedParams.PGS_Guid,
            };
            BuiltInCategory[] categories = new BuiltInCategory[]
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                categories,
                _sharedParamsForElements))
            {
                MessageBox.Show(
                    "В текущем проекте не у всех категорий из:" +
                    "\n\"Стены\", \"Двери\", \"Окна\", \"Обобщенные модели\"" +
                    "\nприсутствует параметр \"PGS_Guid\"!",
                    "Ошибка");
                return false;
            }
            return true;
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
                    (double distanceLeft, double distanceRight, XYZ direction) = GetOpeningSideDistances(opening, hostWall as Wall);
                    string wallMaterial = GetWallMaterial(hostWall as Wall);
                    string levelName = GetElementLevel(opening);
                    XYZ openingLocation = GetLocationPoint(opening);

                    OpeningDto openingDto = new OpeningDto(
                        guid,
                        width,
                        wallThick,
                        wallHeightOverOpening,
                        distanceRight,
                        distanceLeft,
                        wallMaterial,
                        levelName,
                        height,
                        hostWall.Id.IntegerValue,
                        opening.Id.IntegerValue,
                        openingLocation,
                        lintel,
                        direction)
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
            string familyName = lintel.Symbol.FamilyName;

            if (familyName.Equals(_familyLintelBlockName))
            {
                return new BlockLintel(guid, lintel);
            }
            if (familyName.Equals(_familyLintelBarName))
            {
                return new BarLintel(guid, lintel);
            }
            if (familyName.Equals(_familyLintelAngleName))
            {
                return new AngleLintel(guid, lintel);
            }
            return null;
        }


        /// <summary>
        /// Возвращает точку размещения элемента, являющегося стеной или семейством, размещаемым по 1 точке.
        /// Если элемент - стена, возвращается центр осевой линии стены.
        /// Если элемент - экземпляр семейства, то возвращается точка расположения семейства.
        /// </summary>
        /// <param name="elem">Элемент, расположение которого нужно получить. Wall или FamilyInstance по 1 точке.</param>
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
        /// <param name="opening">Проем, сделанный семейством окна или двери, или витражная стена</param>
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
        /// <param name="hostWall">Хост стена проема</param>
        /// <returns>Толщина стены в мм</returns>
        private double GetWallThick(in Wall hostWall)
        {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(hostWall.WallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble(), UnitTypeId.Millimeters));
        }

        /// <summary>
        /// Возвращает название типоразмера стены
        /// </summary>
        /// <param name="hostWall">Хост стена проема</param>
        /// <returns>Название типоразмера стены</returns>
        private string GetWallMaterial(in Wall hostWall)
        {
            return hostWall.Name;
        }

        /// <summary>
        /// Возвращает высоту стены над проемом в мм
        /// </summary>
        /// <param name="opening">Проем, сделанный окном, дверью или витражом</param>
        /// <param name="hostWall">Хост стена проема</param>
        /// <param name="view3d">{3D} вид по умолчанию</param>
        /// <returns>Высота участка стены над проемом в мм</returns>
        private double GetWallHeightOverOpening(in Element opening, in Element hostWall, in View3D view3d)
        {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(hostWall.get_BoundingBox(view3d).Max.Z - opening.get_BoundingBox(view3d).Max.Z, UnitTypeId.Millimeters));
        }

        /// <summary>
        /// Возвращает расстояния от граней проема до торцов станы
        /// </summary>
        /// <param name="opening">Проем</param>
        /// <param name="hostWall">Хост стена проема</param>
        /// <returns>Кортеж расстояний слева и справа от граней проема до торцов стены</returns>
        private (double leftDistance, double rightDistance, XYZ direction) GetOpeningSideDistances(in Element opening, in Wall hostWall)
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
                        Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineEnd) - (width / 2), UnitTypeId.Millimeters)),
                        openingDirection);
                }
                else
                {
                    return (Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineEnd) - (width / 2), UnitTypeId.Millimeters)),
                        Math.Round(UnitUtils.ConvertFromInternalUnits(openingLocationOnLevel.DistanceTo(lineStart) - (width / 2), UnitTypeId.Millimeters)),
                        openingDirection);
                }
            }
            else
            {
                return (0, 0, openingDirection);
            }
        }

        /// <summary>
        /// Возвращает нормаль к передней плоскости проема
        /// </summary>
        /// <param name="opening"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
        /// <param name="doc">Документ, в котором происходит поиск</param>
        /// <param name="guid">Значение параметра "PGS_GUID" перемычки</param>
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
                    bool hasValue = !(parameter is null) && parameter.HasValue;
                    return hasValue && parameter.AsValueString().Equals(guid);
                }) as FamilyInstance;
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
        /// Получает стену, в которой расположен витраж
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
            ReferenceIntersector referenceIntersector =
                new ReferenceIntersector(
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
