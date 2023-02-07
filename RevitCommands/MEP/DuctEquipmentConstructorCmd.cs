﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using MS.RevitCommands.MEP.Enums;
using MS.RevitCommands.MEP.Models;
using MS.RevitCommands.MEP.Models.Installation;
using MS.RevitCommands.MEP.Models.Symbolic;
using MS.GUI.ViewModels.MEP.DuctInstallation;
using MS.GUI.Windows.MEP;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.MEP
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
            Installation installation = GetDataFromUser();

            if (installation is null)
            {
                return Result.Cancelled;
            }
            UIApplication uiapp = commandData.Application;
            string path = CopyDefaultFamily(installation.System);
            UIDocument uidoc = uiapp.OpenAndActivateDocument(path);
            Document doc = uidoc.Document;

            FillInstallationParentFamilyParams(uidoc, installation);

            PlaceNestedFamilies(uidoc, installation);

            doc.Save();

            return Result.Succeeded;
        }

        /// <summary>
        /// Вывести окно для ввода данных и получить вентиляционную установку от пользователя
        /// </summary>
        /// <returns>Установка, заданная пользователем</returns>
        private Installation GetDataFromUser()
        {
            var ui = new DuctInstallationView();
            ui.ShowDialog();
            if (ui.DialogResult != true)
            {
                return null;
            }
            DuctEquipmentConstructorViewModel viewModel = ui.DataContext as DuctEquipmentConstructorViewModel;
            return viewModel.GetInstallation();
        }

        /// <summary>
        /// Размещиет вложенные семейства в документе родительского семейства
        /// </summary>
        /// <param name="uidoc">Документ родительского семейства</param>
        private void PlaceNestedFamilies(
            in UIDocument uidoc,
            in Installation installation)
        {
            Element level = new FilteredElementCollector(uidoc.Document).OfClass(typeof(Level)).FirstOrDefault();
            var mechanics = CreateMechanicFamilies(uidoc, installation);
            var fillings = CreateFillingFamilies(uidoc, installation);

            XYZ startPointMechanic = new XYZ(0, -1 / SharedValues.FootToMillimeters, 0);
            foreach (Family mechanic in mechanics)
            {
                var fSymbol = uidoc.Document.GetElement(mechanic.GetFamilySymbolIds().First()) as FamilySymbol;
                startPointMechanic = PlaceBlankFamilyInstance(uidoc.Document, level, fSymbol, startPointMechanic, true);
            }

            XYZ startPointFilling = new XYZ();
            foreach (ElementType filling in fillings)
            {
                startPointFilling = PlaceBlankFamilyInstance(uidoc.Document, level, filling as FamilySymbol, startPointFilling, false);
            }

            PlaceSymbolicFamilies(uidoc, installation);
        }

        /// <summary>
        /// Размещает семейство болванки оборудования или наполнения
        /// </summary>
        /// <param name="doc">Документ, в котором размещается семейства болванки</param>
        /// <param name="level">Уровень, на котором размещается семейство болванки</param>
        /// <param name="familySymbol">Типоразмер семейства болванки для размещения</param>
        /// <param name="leftPoint">Левая верхнаяя точка для размещения семейства</param>
        /// <param name="isMechanic">
        /// Если True => ADSK_Группирование будет связан с
        /// <see cref="_groupingMechanic">параметром родительского семейства для оборудования</see>
        /// Если False => c <see cref="_groupingFilling">параметром для наполнения</see>
        /// </param>
        /// <returns>Правая верхняя точка размещенного семейства</returns>
        private XYZ PlaceBlankFamilyInstance(
            in Document doc,
            in Element level,
            in FamilySymbol familySymbol,
            XYZ leftPoint,
            bool isMechanic)
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
                    Parameter instGroupPar = famInstEl.get_Parameter(SharedParams.ADSK_Grouping);
                    FamilyParameter famGroupPar = null;
                    if (isMechanic)
                    {
                        famGroupPar = doc.FamilyManager.get_Parameter(_groupingMechanic);
                    }
                    else
                    {
                        famGroupPar = doc.FamilyManager.get_Parameter(_groupingFilling);
                    }
                    doc.FamilyManager.AssociateElementParameterToFamilyParameter(instGroupPar, famGroupPar);
                }
                catch (Autodesk.Revit.Exceptions.ArgumentNullException)
                {
                }
                placeFams.Commit();

                XYZ rightTopPoint = new XYZ(leftPoint.X + 1 / SharedValues.FootToMillimeters, leftPoint.Y, leftPoint.Z);

                return rightTopPoint;
            }
        }


        /// <summary>
        /// Размещает экземпляры семейств УГО в родительском семействе установки
        /// </summary>
        /// <param name="uidoc">Документ родительского семейства установки</param>
        /// <param name="installation">Установка, заданная пользователем</param>
        private void PlaceSymbolicFamilies(
            in UIDocument uidoc,
            in Installation installation)
        {
            Document doc = uidoc.Document;
            ReferencePlane startPlane = new FilteredElementCollector(doc)
                .OfClass(typeof(ReferencePlane))
                .FirstOrDefault(r => r.Name.Equals(_startPlane)) as ReferencePlane;
            Reference startPlaneRef = new Reference(startPlane);
            Element level = new FilteredElementCollector(uidoc.Document).OfClass(typeof(Level)).FirstOrDefault();

            var symbolics = installation.GetSymbolics();

            XYZ startPoint = new XYZ(startPlane.GetPlane().Origin.X, 0, 0);
            foreach (var symbolic in symbolics)
            {
                FamilySymbol famInstSymb = GetFamilySymbol(doc, _familyName, symbolic.Name);
                startPoint = PlaceSymbolicFamilyInstance(doc, level, famInstSymb, startPoint, symbolic.Length / SharedValues.FootToMillimeters);
            }
        }

        /// <summary>
        /// Создает вложенные семейства оборудования в родительском семействе
        /// </summary>
        /// <param name="uidoc">Документ родительского семейства</param>
        /// <param name="installation"></param>
        /// <returns></returns>
        private ICollection<Family> CreateMechanicFamilies(
            in UIDocument uidoc,
            in Installation installation)
        {
            DefinitionFile defFile = SharedParams.GetSharedParameterFileADSK(uidoc);
            string tempDir = Directory
                .CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{SharedValues.TempDirName}").FullName;
            List<Family> loadedMechanics = new List<Family>();
            for (int i = 1; i < installation.GetMechanics().Count; i++)
            {
                var family = CreateAndLoadMechanicFamily(
                    uidoc,
                    defFile,
                    installation.GetMechanics()[1],
                    installation.System + $"-2-вложенное-{i}",
                    tempDir);
                if (family != null)
                {
                    loadedMechanics.Add(family);
                }
            }
            Directory.Delete(tempDir, true);

            return loadedMechanics;
        }

        /// <summary>
        /// Открывает семейство болванки,
        /// заполняет его необходимыми параметрами для оборудования,
        /// закрывает и загружает обратно в родительское смейство с новым названием
        /// </summary>
        /// <param name="uidoc">Документ Revit</param>
        /// <param name="defFile">Объект файла общих параметров</param>
        /// <param name="mechanics">Коллекция оборудования,
        /// параметры которого нужно добавить в документ вложенного семейства</param>
        /// <param name="newName">Новое наименование вложенного семейства оборудования</param>
        /// <param name="tempDir">Путь к временной папке</param>
        /// <returns>Загруженное семейство в родительском семействе</returns>
        private Family CreateAndLoadMechanicFamily(
            in UIDocument uidoc,
            in DefinitionFile defFile,
            in ICollection<Mechanic.Mechanic> mechanics,
            string newName,
            string tempDir)
        {
            Family loadedFamily = null;
            try
            {
                string path = $@"{tempDir}\{newName.ReplaceForbiddenSymbols()}.rfa";
                Family familyBlank = new FilteredElementCollector(uidoc.Document)
                    .OfClass(typeof(Family))
                    .FirstOrDefault(f => f.Name.Equals(_familyBlankFillingName)) as Family;
                Document familyDocument = uidoc.Document.EditFamily(familyBlank);
                familyDocument.SaveAs(path);
                AddAndFillMechanicParamsInDocument(defFile, familyDocument, mechanics, false);
                loadedFamily = familyDocument.LoadFamily(uidoc.Document);
                familyDocument.Close();
                familyDocument.Dispose();
            }
            catch (Exception)
            {
                return null;
            }
            return loadedFamily;
        }

        /// <summary>
        /// Копирует семейство болванки
        /// </summary>
        /// <param name="uidoc">Документ родительского семейства</param>
        /// <returns>Коллекция типоразмеров созданных элементов наполнения установки</returns>
        private ICollection<ElementType> CreateFillingFamilies(
            in UIDocument uidoc,
            in Installation installation)
        {
            List<ElementType> duplicatedFamilies = new List<ElementType>();
            try
            {
                Family familyBlank = new FilteredElementCollector(uidoc.Document)
                    .OfClass(typeof(Family))
                    .FirstOrDefault(f => f.Name.Equals(_familyBlankFillingName)) as Family;
                using (Transaction createFilling = new Transaction(uidoc.Document))
                {
                    createFilling.Start("Типоразмеры наполнения");
                    familyBlank.Name = _familyBlankFillingName + $"-{installation.System}";
                    FamilySymbol familySymbol = uidoc.Document
                        .GetElement(familyBlank.GetFamilySymbolIds().First()) as FamilySymbol;
                    foreach (var filling in installation.GetFillings())
                    {
                        var duplicatedFamily = familySymbol.Duplicate(filling.Name.ReplaceForbiddenSymbols());
                        duplicatedFamilies.Add(duplicatedFamily);
                        duplicatedFamily.get_Parameter(SharedParams.ADSK_Count).Set(filling.Count);
                        duplicatedFamily.get_Parameter(SharedParams.ADSK_Name).Set(filling.Name);
                    }
                    createFilling.Commit();
                }
            }
            catch (Exception)
            {
                return null;
            }
            return duplicatedFamilies;
        }


        /// <summary>
        /// Создает и заполняет все параметры в родительском семействе установки
        /// </summary>
        /// <param name="uidoc">Документ родительского семейства установки</param>
        /// <param name="installation">Установка, заданная пользователем</param>
        private void FillInstallationParentFamilyParams(in UIDocument uidoc, in Installation installation)
        {
            DefinitionFile defFile = SharedParams.GetSharedParameterFileADSK(uidoc);
            FillInstallationParentGeneralParams(uidoc.Document, installation);
            AddAndFillMechanicParamsInDocument(defFile, uidoc.Document, installation.GetMechanics()[0], true); ;
        }

        /// <summary>
        /// Заполнение геометрических и общих параметров самого родительского семейства установки
        /// без добавления параметров для оборудования
        /// </summary>
        /// <param name="doc">Документ родительского семейства установки</param>
        /// <param name="installation">Вентиляционная установка, заданная пользователем</param>
        private void FillInstallationParentGeneralParams(in Document doc, in Installation installation)
        {
            using (Transaction fillParentParams = new Transaction(doc))
            {
                fillParentParams.Start("Заполнение родительских параметров");
                FamilyManager familyManager = doc.FamilyManager;
                var fManager = doc.FamilyManager;

                var parameters = installation.GetParameters();
                foreach (var parameter in parameters)
                {
                    var famParam = fManager.get_Parameter(parameter.Key);
                    var parValue = parameter.Value;
                    try
                    {
                        var paramType = famParam.GetUnitTypeId();
                        parValue = UnitUtils.ConvertToInternalUnits(parValue, paramType);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        parValue = parameter.Value;
                    }
                    if (famParam.Definition.Name.ToLower().Contains("группирование"))
                    {
                        fManager.SetFormula(famParam, $"\"{parValue}\"");
                    }
                    else
                    {
                        fManager.Set(famParam, parValue);
                    }
                }
                fillParentParams.Commit();
            }
        }

        /// <summary>
        /// Добавляет параметры оборудования и их значения в семейство
        /// </summary>
        /// <param name="defFile">Объект файла общих параметров</param>
        /// <param name="doc">Документ семейства, в которое добавляются параметры оборудования</param>
        /// <param name="mechanics">Список оборудования, параметры которого нужно добавить в семейство</param>
        /// <param name="isInstance">True => все параметры добавлять в экземпляр, False => в тип</param>
        private void AddAndFillMechanicParamsInDocument(
                in DefinitionFile defFile,
                in Document doc,
                in ICollection<Mechanic.Mechanic> mechanics,
                bool isInstance)
        {
            var fManager = doc.FamilyManager;

            List<string> addedParametersNames = new List<string>(fManager.GetParameters().Select(p => p.Definition.Name));
            using (Transaction addMechanic = new Transaction(doc))
            {
                addMechanic.Start("Параметры оборудования в родительском");
                foreach (var mechanic in mechanics)
                {
                    var parameters = mechanic.GetNotEmptyParameters();
                    foreach (var parameter in parameters)
                    {
                        SharedParams.AddParameterWithValue(
                            defFile,
                            doc,
                            GroupTypeId.Mechanical,
                            parameter.Key,
                            isInstance,
                            (object)parameter.Value);
                        if (!addedParametersNames.Contains(parameter.Key))
                        {
                            addedParametersNames.Add(parameter.Key);
                        }
                    }
                    string title = $"-----{EquipmentTypeExtension.GetName(mechanic.EquipmentType)}-----";
                    var titlePar = doc.FamilyManager.AddParameter(
                        title,
                        GroupTypeId.Mechanical,
                        SpecTypeId.String.Text,
                        isInstance);
                    doc.FamilyManager.SetFormula(titlePar, $"\"{title}\"");
                    if (!addedParametersNames.Contains(title))
                    {
                        addedParametersNames.Add(title);
                    }
                }
                addMechanic.Commit();
            }
            addedParametersNames.Reverse();
            using (Transaction sortParameters = new Transaction(doc))
            {
                sortParameters.Start("Сортировка параметров");

                fManager.ReorderParameters(addedParametersNames.Select(n => fManager.get_Parameter(n)).ToList());

                sortParameters.Commit();
            }
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
        private string CopyDefaultFamily(string newName)
        {
            string sourcePath = PathMethods.AssemblyDirectory + @"\EmbeddedFamilies\ОВ_установка_default.rfa";
            string destPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{newName}.rfa";
            try
            {
                File.Copy(sourcePath, destPath, true);
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            catch (System.IO.IOException)
            {
                destPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\{newName}{Guid.NewGuid()}.rfa";
                File.Copy(sourcePath, destPath, true);

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
        private XYZ PlaceSymbolicFamilyInstance(
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
    }
}
