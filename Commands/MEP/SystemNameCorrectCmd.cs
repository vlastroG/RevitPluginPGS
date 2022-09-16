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
using static MS.Utilites.WorkWithParameters;
using static MS.Utilites.WorkWithPath;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SystemNameCorrectCmd : IExternalCommand
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
            BuiltInCategory.OST_PlumbingFixtures
        };

        /// <summary>
        /// Возвращает коллекцию всех MEP элементов для корректировки параметра 'ИмяСистемы'
        /// </summary>
        /// <param name="doc">Документ, в котором будут находиться элементы MEP</param>
        /// <returns>Коллекция элементов MEP</returns>
        private IReadOnlyCollection<FamilyInstance> GetMEPelements(Document doc)
        {
            var MEPcategories = new ElementMulticategoryFilter(_categories);
            var mepElements = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WherePasses(MEPcategories)
                .WhereElementIsNotElementType()
                .Where(e => e.LookupParameter(_parSystemName) != null
                 && !String.IsNullOrEmpty(e.LookupParameter(_parSystemName).AsValueString()))
                .Cast<FamilyInstance>()
                .ToList();
            return mepElements;
        }

        /// <summary>
        /// Получить кортеж строк, которые должны содержаться в ADSK_Наименование, и типа системы, 
        /// который необходимо оставить в ИмяСистемы.
        /// </summary>
        /// <returns>Кортеж строк. Если произошла отмена или ошибка, то null</returns>
        private List<(string Naming, string System)> GetNamingAndSystemTuple()
        {
            string excelPath = GetPath(ref _startPath, "Excel Files|*.xlsx", "Выберите файл таблицы Excel", ".xlsx");
            if (String.IsNullOrEmpty(excelPath)) return null;
            List<(string Naming, string System)> namingAndSystemTuple = new List<(string Naming, string System)>();
            using (Excel excel = new Excel(excelPath))
            {
                var row = 1;
                var col = 1;
                string naming = excel.ReadCell(row, col).ToLower();
                string system = excel.ReadCell(row, col + 1).ToLower();
                while (!String.IsNullOrEmpty(naming) && !String.IsNullOrEmpty(system))
                {
                    namingAndSystemTuple.Add((naming, system));
                    row++;
                    naming = excel.ReadCell(row, col).ToLower();
                    system = excel.ReadCell(row, col + 1).ToLower();
                }
            }
            namingAndSystemTuple.DistinctBy(t => t.Naming).ToList();
            return namingAndSystemTuple;
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            List<(string Naming, string System)> namingAndSystemTuple = GetNamingAndSystemTuple();
            if (namingAndSystemTuple is null) return Result.Cancelled;

            var mepEls = GetMEPelements(doc);
            List<ElementId> errors = new List<ElementId>();
            int count = 0;
            using (Transaction correctSystemNames = new Transaction(doc))
            {
                correctSystemNames.Start("Скорректировать ИмяСистемы");
                foreach (var el in mepEls)
                {
                    string adskNaming = String.Empty;
                    if (el.get_Parameter(SharedParams.ADSK_Name).AsValueString() != null)
                        adskNaming = el.get_Parameter(SharedParams.ADSK_Name).AsValueString().ToLower();
                    else if (el.Symbol.get_Parameter(SharedParams.ADSK_Name).AsValueString() != null)
                        adskNaming = el.Symbol.get_Parameter(SharedParams.ADSK_Name).AsValueString().ToLower();
                    else
                    {
                        errors.Add(el.Id);
                        continue;
                    }
                    if (String.IsNullOrEmpty(adskNaming))
                    {
                        errors.Add(el.Id);
                        continue;
                    }
                    string systemFull = el.LookupParameter(_parSystemName).AsValueString().ToLower();
                    string systemShort = namingAndSystemTuple.FirstOrDefault(t => systemFull.Contains(t.Naming)).System;
                    var systems = systemFull.Split('\u002C');
                    var system = systems.Where(s => s.Contains(systemShort)).ToList();
                    var systemCheck = String.Empty;
                    if (system.Count > 1)
                    {
                        errors.Add(el.Id);
                        systemCheck = system.First();
                    }
                    else if (system.Count == 1)
                    {
                        systemCheck = system.First();
                    }
                    else
                    {
                        continue;
                    }
                    el.LookupParameter(_parSystemName).Set(systemCheck);
                    count++;
                }
                correctSystemNames.Commit();
            }

            if (errors.Count > 0)
            {
                string ids = String.Join(", ", errors.Select(e => e.ToString()));
                MessageBox.Show($"Ошибка, элементы не обработаны, " +
                    $"нельзя определить значение параметра 'ADSK_Наименование' " +
                    $"или у элемента несколько однотипных систем. Id: {ids}." +
                    $"\n\n'ИмяСистемы' скорректировано у {count} элементов.",
                    "Корректировка параметра 'ИмяСистемы', выполнено с ошибками!");
            }
            else
            {
                MessageBox.Show($"\n\n'ИмяСистемы' скорректировано у {count} элементов.",
                    "Корректировка параметра 'ИмяСистемы'");
            }
            return Result.Succeeded;
        }
    }
}
