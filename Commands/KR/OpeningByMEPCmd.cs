using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MS.GUI.KR;
using MS.GUI.ViewModels.KR;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.SelectionFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS.Commands.KR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpeningByMEPCmd : IExternalCommand
    {
        /// <summary>
        /// Настройки команды
        /// </summary>
        private readonly SettingsViewModelKR _settings = new SettingsViewModelKR();

        /// <summary>
        /// Категории элементов MEP, которые можно выбирать
        /// </summary>
        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeCurves
        };

        private static Document _doc;

        private static UIDocument _uidoc;

        /// <summary>
        /// Максимальный отступ от MEP элемента
        /// </summary>
        private readonly int _maxOffset = 1000;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            if (String.IsNullOrEmpty(_settings.OpeningFamName)
                || String.IsNullOrEmpty(_settings.OpeningTypeName)
                || String.IsNullOrEmpty(_settings.OpeningOffsetString))
            {
                var settingsKRview = new SettingsKRview();
                settingsKRview.ShowDialog();
                if (settingsKRview.DialogResult != true)
                {
                    return Result.Cancelled;
                }
            }
            if (_settings.OpeningOffset > _maxOffset)
            {
                MessageBox.Show($"Слишком большой отступ: {_settings.OpeningOffset} мм", "Ошибка");
                var settingsKRview = new SettingsKRview();
                settingsKRview.ShowDialog();
            }

            FamilySymbol openingFamSymb = GetOpeningSymbol();
            if (openingFamSymb is null)
            {
                return Result.Cancelled;
            }

            int familyHostType = openingFamSymb.Family
                .get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR).AsInteger();

            (Element mep, Line mepLine) = GetMEPelement();
            if (mep is null) return Result.Cancelled;

            Element hostElement = null;
            switch (familyHostType)
            {
                // основа семейства - стена
                case 1:
                    hostElement = GetHostElement(new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_Walls
                    });
                    break;
                // основа семейства - перекрытие
                case 2:
                    hostElement = GetHostElement(new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_Floors,
                        BuiltInCategory.OST_StructuralFoundation
                    });
                    break;
                // основа семейства - грань
                case 5:
                    hostElement = GetHostElement(new List<BuiltInCategory>
                    {
                        BuiltInCategory.OST_Walls,
                        BuiltInCategory.OST_Floors,
                        BuiltInCategory.OST_StructuralFoundation
                    });
                    break;
                default:
                    MessageBox.Show("Нельзя определить основу заданного семейства", "Ошибка");
                    hostElement = null;
                    break;
            }
            if (hostElement is null) return Result.Cancelled;

            Face face = GetElementPlane(hostElement);
            var plane = face.GetSurface() as Plane;
            var point = GetPlaneAndLineIntersection(plane, mepLine);
            if (point is null)
            {
                MessageBox.Show("Не найдена точка пересечения оси воздуховода и стены", "Ошибка");
                return Result.Cancelled;
            }
            (double openingH, double openingW) = GetOpeningDimensions(mep);
            if (openingH == openingW && openingW == 0)
            {
                MessageBox.Show("Нельзя определить геометрию выбранного воздуховода/трубы",
                    "Ошибка");
                return Result.Failed;
            }

            bool openingPlaced = false;
            switch (familyHostType)
            {
                // основа семейства - стена
                case 1:
                    openingPlaced = PlaceOpeningByWall(_doc, point, hostElement, openingH, openingW, openingFamSymb);
                    break;
                // основа семейства - перекрытие
                case 2:
                    openingPlaced = PlaceOpeningByFloor(_doc, point, hostElement, openingH, openingW, openingFamSymb);
                    break;
                // основа семейства - грань
                case 5:
                    openingPlaced = PlaceOpeningByFace(_doc, point, face, openingH, openingW, openingFamSymb, hostElement.LevelId);
                    break;
                default:
                    MessageBox.Show("Нельзя определить основу заданного семейства", "Ошибка");
                    hostElement = null;
                    break;
            }
            if (!openingPlaced)
            {
                MessageBox.Show(
                    $"Не удалось разместить '{_settings.OpeningTypeName}' " +
                    $"из '{_settings.OpeningFamName}' " +
                    $"в основе с Id: '{hostElement.Id}'", "Ошибка");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Возвращает габариты проема, 
        /// который должен быть размещен по центру пересечения MEP элемента и стены/плиты
        /// </summary>
        /// <param name="mepEl"></param>
        /// <returns></returns>
        private (double OpeningH, double OpeningW) GetOpeningDimensions(in Element mepEl)
        {
            double mepH = 0;
            double mepW = 0;
            double openingH;
            double openingW;
            if (mepEl is Duct)
            {
                try
                {
                    // Если воздуховод прямоугольный или овальный
                    mepH = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
                    mepW = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
                }
                catch (System.NullReferenceException)
                {
                    // Воздуховод круглого сечения
                    mepH = mepEl.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble();
                    mepW = mepH;
                }
            }
            else if (mepEl is Pipe)
            {
                // Труба
                mepH = mepEl.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                mepW = mepH;
            }
            else
            {
                return (0, 0);
            }
            openingH = mepH + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;
            openingW = mepW + 2 * _settings.OpeningOffset / SharedValues.FootToMillimeters;
            return (openingH, openingW);
        }

        /// <summary>
        /// Вывод сообщения о не найденом семействе и типоразмере
        /// </summary>
        /// <param name="familyName">Название семейства</param>
        /// <param name="typeName">Название типоразмера</param>
        private void ShowError(string familyName, string typeName)
        {
            MessageBox.Show(
                $"В проекте не найдено семейство:" +
                $"\n{familyName}" +
                $"\nс типом" +
                $"\n{typeName}",
                "Ошибка");
        }

        /// <summary>
        /// Возвращает плоскую поверхность с ниабольшей площадью и нормалью,
        /// совпадающей с нормалью выбранной стены
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Поверхность стены, или null, если что-то пошло не так</returns>
        private PlanarFace GetElementPlane(Element element)
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
                    && ((elementPlanarFace is null) || (face1.Area >= elementPlanarFace.Area)))
                {
                    elementPlanarFace = face1;
                }
            }
            return elementPlanarFace;
        }

        /// <summary>
        /// Находит точку пересечения поверхности с линией
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="line"></param>
        /// <returns>Точка пересечеия, или null</returns>
        private XYZ GetPlaneAndLineIntersection(Plane plane, Line line)
        {
            UV uv1, uv2 = new UV();

            plane.Project(line.Origin, out uv1, out double d);
            plane.Project(line.Origin + line.Direction, out uv2, out double b);

            XYZ xyz1 = plane.Origin + (uv1.U * plane.XVec) + (uv1.V * plane.YVec);
            XYZ xyz2 = plane.Origin + (uv2.U * plane.XVec) + (uv2.V * plane.YVec);

            if (xyz1.IsAlmostEqualTo(xyz2))
            {
                return xyz1;
            }

            Line projectedLine = Line.CreateUnbound(xyz1, xyz2 - xyz1);

            IntersectionResultArray iResult = new IntersectionResultArray();
            if (line.Intersect(projectedLine, out iResult) != SetComparisonResult.Disjoint)
            {
                return iResult.get_Item(0).XYZPoint;
            }
            return null;
        }

        /// <summary>
        /// Возвращает типоразмер семейства проема, который нкжно разместить
        /// </summary>
        /// <returns>Заданный в <seealso cref="SettingsViewModelKR">настройках</seealso> типоразмер семейства,
        /// или null, если типоразмер отсутствует в проекте</returns>
        private FamilySymbol GetOpeningSymbol()
        {
            var openingSymb = new FilteredElementCollector(_doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft =>
                ft.FamilyName == _settings.OpeningFamName
                && ft.Name == _settings.OpeningTypeName);
            if (openingSymb is null)
            {
                ShowError(_settings.OpeningFamName, _settings.OpeningTypeName);
                return null;
            }
            return openingSymb;
        }

        /// <summary>
        /// Назначение значения параметру семейства в экземпляр, во время исключения 
        /// Autodesk.Revit.Exceptions.InvalidOperationException, выброшенного при попытке
        /// установить это значение общему параметру, который доступен только для чтения.
        /// </summary>
        /// <param name="opening"></param>
        /// <param name="familyParameterName"></param>
        /// <param name="value"></param>
        private void ExceptionHandlerForSetSharedParameterValue(
            ref FamilyInstance opening,
            string familyParameterName,
            double value)
        {
            try
            {
                // попытка назначения значения для параметра семейства
                opening.LookupParameter(familyParameterName).Set(value);
            }
            catch (NullReferenceException)
            {
                // Параметр семейства отсутствует у экземпляра 
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                // Параметр семейства доступен только на чтение
            }
        }

        /// <summary>
        /// Размещает семейство проема на основе стены
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point">Точка размещения</param>
        /// <param name="hostEl">Стена, в которой будет расположен проем</param>
        /// <param name="openingH">Высота проема</param>
        /// <param name="openingW">Ширина проема</param>
        /// <returns>Экземпляр семейства проема</returns>
        private bool PlaceOpeningByWall(
            in Document doc,
            XYZ point,
            in Element hostEl,
            double openingH,
            double openingW,
            FamilySymbol openingSymb)
        {
            if (!openingSymb.IsActive)
            {
                openingSymb.Activate();
            }
            bool isPlaced = false;
            // familyHostType:
            // 1 - Стена
            // 2 - Перекрытие
            // 5 - Грань
            int familyHostType = openingSymb.Family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR).AsInteger();
            if (familyHostType != 1)
            {
                return isPlaced;
            }

            Level level = doc.GetElement(hostEl.LevelId) as Level;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Отверстие КР по инженерке");

                var opening = doc.Create.NewFamilyInstance(
                    point,
                    openingSymb,
                    hostEl,
                    level,
                    StructuralType.NonStructural);

                // true, если экземпляр содержит параметр Рзм.Диаметр или ADSK_Размер_Диаметр
                bool isFamilyCircle =
                    !(opening.get_Parameter(SharedParams.ADSK_DimensionDiameter) is null);
                isPlaced = true;

                if (isFamilyCircle)
                {
                    // Проем круглый
                    opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM)
                        .Set(point.Z - level.Elevation);
                    try
                    {
                        // попытка назначения общего параметра диаметра
                        opening.get_Parameter(SharedParams.ADSK_DimensionDiameter)
                            .Set(openingH >= openingW ? openingH : openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр диаметра отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр диаметра экземпляра доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Диаметр", openingH);
                    }
                }
                else
                {
                    // Проем прямоугольный
                    opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM)
                        .Set(point.Z - 0.5 * openingH - level.Elevation);
                    try
                    {
                        opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                        opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Высота", openingH);
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Ширина", openingW);
                    }
                }
                trans.Commit();
                return isPlaced;
            }
        }

        /// <summary>
        /// Размещает экземпляр семейства проема с основой по перекрытию
        /// </summary>
        /// <param name="doc">Документ, в котором происходит размещение семейства</param>
        /// <param name="point">Точка размещения</param>
        /// <param name="hostEl">Хост элемент</param>
        /// <param name="openingH">Высота проема</param>
        /// <param name="openingW">Ширина проема</param>
        /// <param name="openingSymb">Типоразмер семейства проема</param>
        /// <returns>True, если семейство размещено, иначе false</returns>
        private bool PlaceOpeningByFloor(
                in Document doc,
                XYZ point,
                in Element hostEl,
                double openingH,
                double openingW,
                FamilySymbol openingSymb)
        {
            if (!openingSymb.IsActive)
            {
                openingSymb.Activate();
            }
            bool isPlaced = false;
            // familyHostType:
            // 1 - Стена
            // 2 - Перекрытие
            // 5 - Грань
            int familyHostType = openingSymb.Family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR).AsInteger();
            if (familyHostType != 2)
            {
                return isPlaced;
            }

            Level level = doc.GetElement(hostEl.LevelId) as Level;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Отверстие КР по инженерке");
                var opening = doc.Create.NewFamilyInstance(
                    point,
                    openingSymb,
                    hostEl,
                    level,
                    StructuralType.NonStructural);

                // true, если экземпляр содержит параметр Рзм.Диаметр или ADSK_Размер_Диаметр
                bool isFamilyCircle =
                    !(opening.get_Parameter(SharedParams.ADSK_DimensionDiameter) is null);
                isPlaced = true;

                if (isFamilyCircle)
                {
                    // Проем круглый
                    try
                    {
                        // попытка назначения общего параметра диаметра
                        opening.get_Parameter(SharedParams.ADSK_DimensionDiameter)
                            .Set(openingH >= openingW ? openingH : openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр диаметра отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр диаметра экземпляра доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Диаметр", openingH);
                    }
                }
                else
                {
                    // Проем прямоугольный
                    try
                    {
                        opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                        opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Высота", openingH);
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Ширина", openingW);
                    }
                }
                trans.Commit();
                return isPlaced;
            }
        }

        /// <summary>
        /// Размещает семейство проема на основе грани
        /// </summary>
        /// <param name="doc">Документ, в котором происходит размещение</param>
        /// <param name="point">Точка, по которой размещается семейство</param>
        /// <param name="face">Грань, на которой размещается семейство</param>
        /// <param name="openingH">Высота проема</param>
        /// <param name="openingW">Ширина проема</param>
        /// <param name="openingSymb">Типоразмер семейства проема</param>
        /// <returns>True, если семейство размещено, иначе false</returns>
        private bool PlaceOpeningByFace(
            in Document doc,
            XYZ point,
            in Face face,
            double openingH,
            double openingW,
            FamilySymbol openingSymb,
            ElementId levelId)
        {
            if (!openingSymb.IsActive)
            {
                openingSymb.Activate();
            }
            bool isPlaced = false;
            // familyHostType:
            // 1 - Стена
            // 2 - Перекрытие
            // 5 - Грань
            int familyHostType = openingSymb.Family.get_Parameter(BuiltInParameter.FAMILY_HOSTING_BEHAVIOR).AsInteger();
            if (familyHostType != 5)
            {
                return isPlaced;
            }
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Отверстие КР по инженерке");

                var opening = doc.Create.NewFamilyInstance(
                    face,
                    point,
                    new XYZ(),
                    openingSymb);
                isPlaced = true;
                opening.get_Parameter(BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM).Set(levelId);
                // true, если экземпляр содержит параметр Рзм.Диаметр или ADSK_Размер_Диаметр
                bool isFamilyCircle =
                    !(opening.get_Parameter(SharedParams.ADSK_DimensionDiameter) is null);

                if (isFamilyCircle)
                {
                    // Проем круглый
                    try
                    {
                        // попытка назначения общего параметра диаметра
                        opening.get_Parameter(SharedParams.ADSK_DimensionDiameter)
                            .Set(openingH >= openingW ? openingH : openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр диаметра отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр диаметра экземпляра доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Диаметр", openingH);
                    }
                }
                else
                {
                    // Проем прямоугольный
                    try
                    {
                        opening.get_Parameter(SharedParams.ADSK_DimensionHeight).Set(openingH);
                        opening.get_Parameter(SharedParams.ADSK_DimensionWidth).Set(openingW);
                    }
                    catch (NullReferenceException)
                    {
                        // Общий параметр отсутствует у экземпляра 
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        // Общий параметр доступен только на чтение
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Высота", openingH);
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Длина", openingH);
                        ExceptionHandlerForSetSharedParameterValue(ref opening, "Ширина", openingW);
                    }

                }
                trans.Commit();
                return isPlaced;
            }
        }



        /// <summary>
        /// Выбор воздуховода из связанного файла пользователем
        /// </summary>
        /// <returns>Воздуховод, или null, если операция отменена или не валидна</returns>
        private (Element duct, Line ductLine) GetMEPelement()
        {
            Selection sel = _uidoc.Selection;
            MulticategoryInLinkSelectionFilter filter
                = new MulticategoryInLinkSelectionFilter(
                  _doc, _categories);
            Element mepEl = null;
            try
            {
                Reference mepElRef = _uidoc.Selection.PickObject(
                    ObjectType.LinkedElement,
                    filter,
                    "Выберите воздуховод или трубу из связи");
                mepEl = filter.LinkedDocument.GetElement(mepElRef.LinkedElementId);
                var link = _doc.GetElement(mepElRef.ElementId) as RevitLinkInstance;
                var mepElCurve = (mepEl.Location as LocationCurve).Curve;
                Transform transform = link.GetTransform();
                if (!transform.AlmostEqual(Transform.Identity))
                {
                    mepElCurve = mepElCurve.CreateTransformed(transform);
                }
                return (mepEl, mepElCurve as Line);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return (null, null);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выбирать воздуховоды и трубы из связанных файлов",
                    "Ошибка");
                return (null, null);
            }
        }

        /// <summary>
        /// Выбор стены пользователем
        /// </summary>
        /// <returns>Стена, или null, если операция отменена или не валидна</returns>
        private Element GetHostElement(List<BuiltInCategory> categories)
        {
            Selection sel = _uidoc.Selection;
            SelectionFilterElementsOfCategory<Element> filter
                = new SelectionFilterElementsOfCategory<Element>(
                    categories,
                    false);
            Element hostElement = null;
            try
            {
                Reference wallRef = _uidoc.Selection.PickObject(
                    ObjectType.Element,
                    filter,
                    "Выберите основу для заданного семейства");
                hostElement = _doc.GetElement(wallRef);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                MessageBox.Show(
                    "Перейдите на вид, где можно выделить основу заданного семейства",
                    "Ошибка");
                return null;
            }
            return hostElement;
        }
    }
}
