using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using MS.Commands.MEP.Models;
using MS.Commands.MEP.Models.Installation;
using MS.GUI.ViewModels.MEP.DuctInstallation;
using MS.GUI.Windows.MEP;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DuctEquipmentConstructorCmd : IExternalCommand
    {
        /// <summary>
        /// Название семейства и типоразмера наполнения = "Наполнение"
        /// </summary>
        private readonly string _familyBlankFillingName = "Наполнение";

        /// <summary>
        /// Название размещаемого семейства - "Элементы установки"
        /// </summary>
        private readonly string _familyName = "Элементы установки";

        /// <summary>
        /// Название параметра родительского семейства "Вложенное_оборудование_группирование"
        /// </summary>
        private readonly string _groupingMechanic = "Вложенное_оборудование_группирование";

        /// <summary>
        /// Название параметра родительского семейства "Вложенное_наполнение_группирование"
        /// </summary>
        private readonly string _groupingFilling = "Вложенное_наполнение_группирование";

        /// <summary>
        /// Название размещаемого тестового типоразмера семейства - "Фильтр"
        /// </summary>
        private readonly string _typeName = "Фильтр";

        /// <summary>
        /// Название опорного уровня в семействе установки - "Опорный уровень"
        /// </summary>
        private readonly string _viewName = "Опорный уровень";

        /// <summary>
        /// Название центральной фронтальной опорной плоскости - "По центру (Вперед/Назад)"
        /// </summary>
        private readonly string _centerPlane = "По центру (Вперед/Назад)";

        /// <summary>
        /// Название крайней левой плоскости для начала размещения оборудования в установке - "Начало оборудования"
        /// </summary>
        private readonly string _startPlane = "Начало оборудования";

        /// <summary>
        /// Название левой опорной плоскости вемейства оборудования для установки - "Слева"
        /// </summary>
        private readonly string _leftPlane = "Слева";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var ui = new DuctInstallationView();
            ui.ShowDialog();
            DuctEquipmentConstructorViewModel viewModel = ui.DataContext as DuctEquipmentConstructorViewModel;
            var testInt = viewModel.TestIntNull;
            var testDouble = viewModel.TestDoubleNull;
            var testNameShort = viewModel.NameShort;

            //return Result.Succeeded;
            UIApplication uiapp = commandData.Application;

            string path = CopyFamily("П4_Тест");
            UIDocument uidoc = uiapp.OpenAndActivateDocument(path);
            Document doc = uidoc.Document;

            // Получить начальные значения оборудования установки
            FamilySymbol famInstSymb = GetFamilySymbol(doc, _familyName, _typeName);
            Element level = new FilteredElementCollector(doc).OfClass(typeof(Level)).FirstOrDefault();
            ReferencePlane startPlane = new FilteredElementCollector(doc)
                .OfClass(typeof(ReferencePlane))
                .FirstOrDefault(r => r.Name.Equals(_startPlane)) as ReferencePlane;
            Reference startPlaneRef = new Reference(startPlane);
            XYZ startPoint = new XYZ(startPlane.GetPlane().Origin.X, 0, 0);

            Installation installation = CreateTestInstallation();
            var mechanics = installation.GetMechanics();
            var fillings = installation.GetFillings();

            FillInstallationParentGeneralParams(doc, installation);

            XYZ startBlankPoint = new XYZ();



            double length = 1;
            double length1 = 3;
            double length2 = 1.5;
            double length3 = 2;
            double length4 = 1274 / 304.8;

            // Создать тестовые элементы установки
            XYZ rightPoint1 = CreateSymbolicFamilyInstance(doc, level, famInstSymb, startPoint, length);
            XYZ rightPoint2 = CreateSymbolicFamilyInstance(doc, level, famInstSymb, rightPoint1, length1);
            XYZ rightPoint3 = CreateSymbolicFamilyInstance(doc, level, famInstSymb, rightPoint2, length2);
            XYZ rightPoint4 = CreateSymbolicFamilyInstance(doc, level, famInstSymb, rightPoint3, length3);
            _ = CreateSymbolicFamilyInstance(doc, level, famInstSymb, rightPoint4, length4);


            // Добавление общих параметров
            SharedParams.AddParameterWithValue(uidoc, "PGS_ОВ_ХОВС", "PGS_ТипоразмерВентилятора", true, "ТестТипоразмера_007");
            doc.Save();

            return Result.Succeeded;
        }

        /// <summary>
        /// Заполнение геометрических и общих параметров родительского семейства установки
        /// (без добавления параметров для оборудования)
        /// </summary>
        /// <param name="doc">Документ родительского семейства установки</param>
        /// <param name="installation">Вентиляционная установка</param>
        private void FillInstallationParentGeneralParams(in Document doc, in Installation installation)
        {
            using (Transaction fillParentParams = new Transaction(doc))
            {
                fillParentParams.Start("Заполнение родительских парметров");
                FamilyManager familyManager = doc.FamilyManager;
                FamilyParameter famWidthPar = familyManager
                    .GetParameters()
                    .Where(p => p.IsShared)
                    .FirstOrDefault(p => p.GUID.Equals(SharedParams.ADSK_DimensionWidth));
                FamilyParameter famHeightPar = familyManager
                    .GetParameters()
                    .Where(p => p.IsShared)
                    .FirstOrDefault(p => p.GUID.Equals(SharedParams.ADSK_DimensionHeight));
                FamilyParameter famLengthPar = familyManager
                    .GetParameters()
                    .Where(p => p.IsShared)
                    .FirstOrDefault(p => p.GUID.Equals(SharedParams.ADSK_DimensionLength));
                FamilyParameter famGroupParentPar = familyManager
                    .GetParameters()
                    .Where(p => p.IsShared)
                    .FirstOrDefault(p => p.GUID.Equals(SharedParams.ADSK_Grouping));
                FamilyParameter famGroupMechPar = familyManager
                    .GetParameters()
                    .Where(p => !p.IsShared)
                    .FirstOrDefault(p => p.Definition.Name.Equals(_groupingMechanic));
                FamilyParameter famGroupFillPar = familyManager
                    .GetParameters()
                    .Where(p => !p.IsShared)
                    .FirstOrDefault(p => p.Definition.Name.Equals(_groupingFilling));
                familyManager.Set(famLengthPar, installation.Length / SharedValues.FootToMillimeters);
                familyManager.Set(famWidthPar, installation.Width / SharedValues.FootToMillimeters);
                familyManager.Set(famHeightPar, installation.Height / SharedValues.FootToMillimeters);
                familyManager.Set(famGroupParentPar, installation.System + installation.GroupingParent);
                familyManager.Set(famGroupMechPar, installation.System + installation.GroupingMechanic);
                familyManager.Set(famGroupFillPar, installation.System + installation.GroupingFilling);
                fillParentParams.Commit();
            }
        }

        private void FillInstallationMechanicParams(in Document doc, in Installation installation)
        {
            var mechanics = installation.GetMechanics().First();
            foreach (var mechanic in mechanics)
            {
            }
        }

        /// <summary>
        /// Создает тестовую установку (симуляция ввода пользователя)
        /// </summary>
        /// <returns>Заданная вентиляционная установка</returns>
        private Installation CreateTestInstallation()
        {
            Installation installationTest = new Installation(1100, 1100, 7400);

            List<Mechanic.Mechanic> mechanics = new List<Mechanic.Mechanic>()
            {
                new Mechanic.Impl.Fan(300)
                {
                    Mark = "V1.0.P63.R-5,5x15",
                    AirFlow = 8225,
                    AirPressureLoss = 500,
                    FanSpeed = 1514,
                    ExplosionProofType = "АИР112M4",
                    RatedPower = 5500,
                    EngineSpeed = 1432
                },
                new Mechanic.Impl.Heater(300)
                {
                    Type = "N1.2",
                    Count = 1,
                    TemperatureIn = -24,
                    TemperatureOut = 21,
                    PowerHeat = 124131,
                    AirPressureLoss = 73.1
                },
                new Mechanic.Impl.Cooler(300)
                {
                    Type = "C2.4",
                    Count = 1,
                    TemperatureIn = 29,
                    TemperatureOut = 15,
                    PowerCool = 64500,
                    AirPressureLoss = 254.3
                },
                new Mechanic.Impl.Heater(300)
                {
                    Type = "E1.45",
                    Power = 45000,
                    Count = 1,
                    PowerHeat = 44260,
                    TemperatureIn = 12.5,
                    TemperatureOut = 29,
                    AirPressureLoss = 22.1
                },
                new Mechanic.Impl.Heater(300)
                {
                    Type = "Е1.30",
                    Power = 30000,
                    Count = 1,
                    PowerHeat = 16530,
                    TemperatureIn = 15,
                    TemperatureOut = 21,
                    AirPressureLoss = 22.1
                }
            };

            List<Filling> fillings = new List<Filling>()
            {
                new Filling("Блок Управления: Блок управления ACE CR4-E0-3R0-1-1H25-S-S/N", 1),
                new Filling("Датчик влажности/температуры комнатный DPWC111000", 1),
                new Filling("Датчик перепада давления 500 Pa DVL-500", 3),
                new Filling("Датчик температуры воды погружной WTP-3", 1),
                new Filling("Датчик температуры канальный ARK-3",1 ),
                new Filling("Датчик температуры наружного воздуха ARN-3", 1),
                new Filling("Привод ELVA 05/24.M", 1),
                new Filling("Привод PDF 08/230.D", 1),
                new Filling("Термостат 3 м", 2),
                new Filling("Термостат 6 м", 1),
                new Filling("Трехходовой вентиль TBG 25-10", 1),
                new Filling("Циркуляционный насос VL-32PBG-8-N (230В)", 1),
                new Filling("Частотный преобразователь 5,5 кВт 380 В", 1),
                new Filling("Блок Управления: Шкаф автоматики ACW с контроллером FR-11", 1),
                new Filling("Блок Управления: Щит управления силовой ACV-V E45", 1),
                new Filling("Блок Управления: Щит управления силовой ACV-V Е30", 1)
            };

            installationTest.AddMechanic(mechanics);
            installationTest.AddFilling(fillings);

            return installationTest;
        }


        /// <summary>
        /// Возвращает заданный типоразмер семейства
        /// </summary>
        /// <param name="doc">Документ, в котором происоходит поиск</param>
        /// <param name="familyName">Название семейства</param>
        /// <param name="typeName">Название типоразмера семейства</param>
        /// <returns>Типоразмер, которы найден в результате поиска, или null</returns>
        private FamilySymbol GetFamilySymbol(in Document doc, string familyName, string typeName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == familyName && ft.Name == typeName);
        }

        /// <summary>
        /// Копирует семейство из ресурсов плагина в папку 'Документы' пользователя
        /// </summary>
        /// <param name="newName">Название нового семейства</param>
        /// <returns>Полный путь к скопированному файлу, или пустая строка, если не удалось скопировать файл семейства</returns>
        private string CopyFamily(string newName)
        {
            string sourcePath = WorkWithPath.AssemblyDirectory + @"\EmbeddedFamilies\ОВ_установка_default.rfa";
            string destPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{newName}.rfa";
            try
            {
                File.Copy(sourcePath, destPath, true);
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            return destPath;
        }

        /// <summary>
        /// Размещает экземпляр семейства оборудования с условными обозначениями (кубик) внутри семейства установки
        /// </summary>
        /// <param name="doc">Документ семейства установки</param>
        /// <param name="level">Опорный уровень</param>
        /// <param name="familySymbol">Типоразмер семейства оборудования для размещения в установке</param>
        /// <param name="leftPoint">Левая начальная точка для размещения оборудования</param>
        /// <param name="length">Длина оборудования</param>
        /// <returns>Правая крайняя точка размещенного семейства</returns>
        /// <exception cref="ArgumentException"></exception>
        private XYZ CreateSymbolicFamilyInstance(
            in Document doc,
            in Element level,
            in FamilySymbol familySymbol,
            XYZ leftPoint,
            double length)
        {
            using (Transaction placeFams = new Transaction(doc))
            {
                placeFams.Start("Размещение элемента установки");
                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                }

                FamilyInstance famInstEl = doc.FamilyCreate.NewFamilyInstance(
                    leftPoint,
                    familySymbol,
                    level,
                    StructuralType.NonStructural);
                doc.Regenerate();
                try
                {
                    Parameter instWidthPar = famInstEl.get_Parameter(SharedParams.ADSK_DimensionWidth);
                    Parameter instHeightPar = famInstEl.get_Parameter(SharedParams.ADSK_DimensionHeight);
                    famInstEl.get_Parameter(SharedParams.ADSK_DimensionLength).Set(length);

                    FamilyParameter famWidthPar = doc.FamilyManager
                        .GetParameters()
                        .Where(p => p.IsShared)
                        .FirstOrDefault(p => p.GUID.Equals(instWidthPar.GUID));
                    FamilyParameter famHeightPar = doc.FamilyManager
                        .GetParameters()
                        .Where(p => p.IsShared)
                        .FirstOrDefault(p => p.GUID.Equals(instHeightPar.GUID));

                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(instHeightPar, famHeightPar);
                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(instWidthPar, famWidthPar);

                    View view = new FilteredElementCollector(doc)
                        .OfClass(typeof(View))
                        .FirstOrDefault(v => v.Name.Equals(_viewName)) as View;

                    Reference centerPlaneInst = famInstEl.GetReferenceByName(_centerPlane);
                    Reference centerVertPlane = new Reference(new FilteredElementCollector(doc)
                        .OfClass(typeof(ReferencePlane))
                        .FirstOrDefault(r => r.Name.Equals(_centerPlane)));
                    doc.FamilyCreate.NewAlignment(view, centerVertPlane, centerPlaneInst);

                    Reference leftPlaneInst = famInstEl.GetReferenceByName(_leftPlane);
                }
                catch (NullReferenceException)
                {
                    throw new ArgumentException(
                        "Семейство некорректно! Нельзя назначить значения параметров!");
                }
                placeFams.Commit();

                XYZ rightPoint = new XYZ(leftPoint.X + length, leftPoint.Y, leftPoint.Z);

                return rightPoint;
            }
        }

        /// <summary>
        /// Размещает заданный типоразмер семейства болванки (предварительно загрузить скорректированный тип в родительское семейство)
        /// по заданным координатам
        /// </summary>
        /// <param name="doc">Документ родительского семейства установки</param>
        /// <param name="level">Уровень для размещения</param>
        /// <param name="familySymbol">Типоразмер семейства болванки</param>
        /// <param name="leftTopPoint">Левая верхняя точка (на плане) для размещения семейства болванки</param>
        /// <returns>Координата правой верхней точки размещенного семейства болванки  </returns>
        private XYZ CreateBlankFamilyInstance(
            in Document doc,
            in Element level,
            in FamilySymbol familySymbol,
            XYZ leftTopPoint)
        {
            using (Transaction addBoldFamily = new Transaction(doc))
            {
                addBoldFamily.Start("Размещение болванки");

                FamilyInstance famInstEl = doc.FamilyCreate.NewFamilyInstance(
                    leftTopPoint,
                    familySymbol,
                    level,
                    StructuralType.NonStructural);

                doc.Regenerate();

                addBoldFamily.Commit();
            }
            return new XYZ(1 / 304.8, leftTopPoint.Y, leftTopPoint.Z);
        }
    }
}
