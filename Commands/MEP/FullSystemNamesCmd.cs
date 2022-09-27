using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites.DataParsers;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static MS.Utilites.WorkWithPath;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class FullSystemNamesCmd : IExternalCommand
    {
        /// <summary>
        /// Стартовый путь по умолчанию к Excel файлу - Документы пользователя
        /// </summary>
        private static string _startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Название параметра проекта ИмяСистемы
        /// </summary>
        private readonly string _parSystemName = "ИмяСистемы";

        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_FlexPipeCurves
        };

        /// <summary>
        /// Возвращает коллекцию всех MEP элементов для корректировки параметра 'ИмяСистемы'
        /// </summary>
        /// <param name="doc">Документ, в котором будут находиться элементы MEP</param>
        /// <returns>Коллекция элементов MEP</returns>
        private IReadOnlyCollection<Element> GetMEPelements(in Document doc)
        {
            var MEPcategories = new ElementMulticategoryFilter(_categories);
            var mepElements = new FilteredElementCollector(doc)
                .WherePasses(MEPcategories)
                .WhereElementIsNotElementType()
                .Where(e => e.LookupParameter(_parSystemName) != null
                 && !String.IsNullOrEmpty(e.LookupParameter(_parSystemName).AsValueString()))
                .ToList();
            return mepElements;
        }

        private bool ValidateSharedParams(in Document doc)
        {
            Guid[] _sharedParamsForWalls = new Guid[] {
            SharedParams.PGS_SystemName
            };
            foreach (var category in _categories)
            {
                if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                    doc,
                    category,
                    _sharedParamsForWalls))
                {
                    MessageBox.Show("В текущем проекте параметр 'PGS_Наименование системы' " +
                        "присутствует не у всех экземпляров категорий:" +
                        "\nАрматура трубопроводов" +
                        "\nГибкие трубы" +
                        "\nСантехнические приборы" +
                        "\nСоединительные детали трубопроводов" +
                        "\nТрубы" +
                        "\nОборудование",
                        "Ошибка");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Получить кортеж строк сокращения системы и ее полного названия
        /// </summary>
        /// <returns>Кортеж строк. Если произошла отмена или ошибка, то null</returns>
        private List<(string ShortName, string FullName)> GetShortAndFullTuple()
        {
            string excelPath = GetPath(ref _startPath, "Excel Files|*.xlsx", "Выберите файл таблицы Excel", ".xlsx");
            if (String.IsNullOrEmpty(excelPath)) return null;
            List<(string ShortName, string FullName)> shortNameAndFullNameTuple = new List<(string ShortName, string FullName)>();
            using (Excel excel = new Excel(excelPath))
            {
                var row = 1;
                var col = 1;
                string shortName = excel.ReadCell(row, col);
                string fullName = excel.ReadCell(row, col + 1);
                while (!String.IsNullOrEmpty(shortName) && !String.IsNullOrEmpty(fullName))
                {
                    shortNameAndFullNameTuple.Add((shortName, fullName));
                    row++;
                    shortName = excel.ReadCell(row, col);
                    fullName = excel.ReadCell(row, col + 1);
                }
            }
            shortNameAndFullNameTuple.DistinctBy(t => t.ShortName).ToList();
            return shortNameAndFullNameTuple;
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (!ValidateSharedParams(doc)) return Result.Cancelled;
            var elems = GetMEPelements(doc);
            var shortFullNamesTuple = GetShortAndFullTuple();
            if (shortFullNamesTuple is null) return Result.Cancelled;
            int count = 0;
            using (Transaction setFullNamesTrans = new Transaction(doc))
            {
                setFullNamesTrans.Start("Назначить PGS_Наименование системы");
                foreach (var elem in elems)
                {
                    string systemShortName = elem.LookupParameter(_parSystemName).AsValueString();
                    string fullName = shortFullNamesTuple.FirstOrDefault(t => t.ShortName == systemShortName).FullName ?? String.Empty;
                    if (!String.IsNullOrEmpty(fullName))
                    {
                        elem.get_Parameter(SharedParams.PGS_SystemName).Set(fullName);
                        count++;
                    }
                }
                setFullNamesTrans.Commit();
            }
            MessageBox.Show($"'PGS_Наименование системы' скорректировано у {count} элементов.",
                "Корректировка параметра 'PGS_Наименование системы'");
            return Result.Succeeded;
        }
    }
}
