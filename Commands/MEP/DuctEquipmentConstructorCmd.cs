using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
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
        /// Название размещаемого семейства - "Элементы установки"
        /// </summary>
        private readonly string _familyName = "Элементы установки";

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
            UIApplication uiapp = commandData.Application;

            string path = CopyFamily("В123");
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

            double length = 1;
            double length1 = 3;
            double length2 = 1.5;
            double length3 = 2;
            double length4 = 1274 / 304.8;

            // Создать тестовые элементы установки
            XYZ rightPoint1 = CreateFamilyInstance(doc, level, famInstSymb, startPoint, length);
            XYZ rightPoint2 = CreateFamilyInstance(doc, level, famInstSymb, rightPoint1, length1);
            XYZ rightPoint3 = CreateFamilyInstance(doc, level, famInstSymb, rightPoint2, length2);
            XYZ rightPoint4 = CreateFamilyInstance(doc, level, famInstSymb, rightPoint3, length3);
            _ = CreateFamilyInstance(doc, level, famInstSymb, rightPoint4, length4);


            doc.Save();
            return Result.Succeeded;
        }

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
                File.Copy(sourcePath, destPath);
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            return destPath;
        }

        /// <summary>
        /// Размещает экземпляр семейства оборудования внутри установки
        /// </summary>
        /// <param name="doc">Документ семейства установки</param>
        /// <param name="level">Опорный уровень</param>
        /// <param name="familySymbol">Типоразмер семейства оборудования для размещения в установке</param>
        /// <param name="leftPoint">Левая начальная точка для размещения оборудования</param>
        /// <param name="length">Длина оборудования</param>
        /// <returns>Правая крайняя точка размещенного семейства</returns>
        /// <exception cref="ArgumentException"></exception>
        private XYZ CreateFamilyInstance(
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
