using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
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

            System.Windows.Forms.DialogResult trimNumbersUI = UserInput.YesNoCancelInput("Корректировка параметра ИмяСистемы", "Удалять цифры из наименования системы?");
            bool trimNumbers = false;
            switch (trimNumbersUI)
            {
                case System.Windows.Forms.DialogResult.Yes:
                    trimNumbers = true;
                    break;
                case System.Windows.Forms.DialogResult.No:
                    trimNumbers = false;
                    break;
                default:
                    return Result.Cancelled;
            }

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
                    if (el is FamilyInstance elFamInst)
                    {
                        if (elFamInst.get_Parameter(SharedParams.ADSK_Name).AsValueString() != null)
                            adskNaming = elFamInst.get_Parameter(SharedParams.ADSK_Name).AsValueString().ToLower();
                        else if (elFamInst.Symbol.get_Parameter(SharedParams.ADSK_Name).AsValueString() != null)
                            adskNaming = elFamInst.Symbol.get_Parameter(SharedParams.ADSK_Name).AsValueString().ToLower();
                        if (String.IsNullOrEmpty(adskNaming))
                        {
                            // Пропустить экземпляры элементов семейств,
                            // у которых отсутствует или пуст параметр ADSK_Наименование
                            errors.Add(el.Id);
                            continue;
                        }
                    }
                    // Значение параметра элемента ИмяСистемы в нижнем регистре
                    string systemFull = el.LookupParameter(_parSystemName).AsValueString().ToLower();
                    // Массив значений имен систем элемента, которые были разделены запятой (в нижнем регистре)
                    var systems = systemFull.Split('\u002C');
                    // Имя системы, которое нужно перезаписать в ИмяСистемы
                    var systemCheck = String.Empty;

                    if (systems.Length == 1)
                    {
                        // Если у элемента 1 тип системы изначально
                        systemCheck = trimNumbers
                            ? systems.First().Split(' ').FirstOrDefault().ToUpper()
                            : systems.First().ToUpper();
                    }
                    else if (systems.Length > 1)
                    {
                        // У элемента несколько типов систем
                        if (!String.IsNullOrEmpty(adskNaming))
                        {
                            // Обработка элемента экземпляра семейства с заполненным параметром ADSK_Наименование

                            // буква, заданная в Excel файле для ADSK_Наименование
                            var namingAndSystemNeed = namingAndSystemTuple.FirstOrDefault(t => adskNaming.Contains(t.Naming));
                            if (namingAndSystemNeed.System is null)
                            {
                                // Пропустить экземпляры семейств, у которых несколько систем
                                // и ни одна из них не подходит заданной в Excel букве
                                errors.Add(el.Id);
                                continue;
                            }
                            var system = systems.Where(s => s.Contains(namingAndSystemNeed.System)).ToList();
                            if (system.Count == 1)
                            {
                                systemCheck = trimNumbers
                                    ? system.First().Split(' ').FirstOrDefault().ToUpper()
                                    : system.First().ToUpper();
                            }
                            else
                            {
                                // У экземпляра семейства присутствует несколько систем с одинаковой буквой из Excel
                                // или ни одна система не содержит букву из Excel
                                errors.Add(el.Id);
                                continue;
                            }
                        }
                        else
                        {
                            errors.Add(el.Id);
                            continue;
                        }
                    }
                    else
                    {
                        // ИмяСистемы пусто
                        errors.Add(el.Id);
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
                    $"или нельзя определить необходимый тип системы. Id: {ids}." +
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
