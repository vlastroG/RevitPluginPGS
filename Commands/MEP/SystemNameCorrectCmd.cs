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
        /// Путь к файлу Excel
        /// </summary>
        private static string _excelPath = String.Empty;

        /// <summary>
        /// Название параметра проекта ИмяСистемы
        /// </summary>
        private readonly string _parSystemName = "ИмяСистемы";

        /// <summary>
        /// Категории элементов для обработки параметра ИмяСистемы:
        /// OST_PipeAccessory       Арматура трубопроводов,
        /// OST_MechanicalEquipment Оборудование,
        /// OST_PlumbingFixtures    Сантехнические приборы,
        /// OST_PipeCurves          Трубы,
        /// OST_PipeFitting         Соединительные детали трубопроводов,
        /// OST_FlexPipeCurves      Гибкие трубы
        /// OST_PipeInsulations     Материалы изоляции труб
        /// </summary>
        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_PlumbingFixtures,
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_PipeFitting,
            BuiltInCategory.OST_FlexPipeCurves,
            BuiltInCategory.OST_PipeInsulations
        };

        /// <summary>
        /// Возвращает коллекцию всех MEP элементов для корректировки параметра 'ИмяСистемы'
        /// </summary>
        /// <param name="doc">Документ, в котором будут находиться элементы MEP</param>
        /// <returns>Коллекция элементов MEP</returns>
        private ICollection<Element> GetMEPelements(in Document doc, in ICollection<BuiltInCategory> categories)
        {
            var MEPcategories = new ElementMulticategoryFilter(categories);
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
        private List<(string Naming, string System)> GetNamingAndSystemTuple(int sheetNumber)
        {
            _excelPath = GetPath(ref _startPath, "Excel Files|*.xlsx", "Выберите файл таблицы Excel", ".xlsx");
            if (String.IsNullOrEmpty(_excelPath)) return null;
            List<(string Naming, string System)> namingAndSystemTuple = new List<(string Naming, string System)>();
            using (Excel excel = new Excel(_excelPath, sheetNumber))
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

            bool trimNumbers = true;

            List<(string Naming, string System)> namingAndSystemTuple = GetNamingAndSystemTuple(1);
            if (namingAndSystemTuple is null) return Result.Cancelled;

            var mepEls = GetMEPelements(doc, _categories);
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
                    string systemNameBefore = el.LookupParameter(_parSystemName).AsValueString();
                    // Значение параметра элемента ИмяСистемы в нижнем регистре
                    string systemFull = systemNameBefore.ToLower();
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
                                var shortNames = systems.Select(sysLong => sysLong.Split(' ').First());
                                if (AreEquals(shortNames))
                                {
                                    // у элемента несколько одинаковых систем
                                    systemCheck = shortNames.First();
                                }
                                else
                                {
                                    // Пропустить экземпляры семейств, у которых несколько разных систем
                                    // и ни одна из них не подходит заданной в Excel букве
                                    errors.Add(el.Id);
                                    continue;
                                }
                            }
                            else
                            {
                                var system = systems.Where(s => s.Contains(namingAndSystemNeed.System)).ToList();
                                if (system.Count == 1)
                                {
                                    systemCheck = trimNumbers
                                        ? system.First().Split(' ').FirstOrDefault().ToUpper()
                                        : system.First().ToUpper();
                                }
                                else
                                {
                                    var shortNames = systems.Select(sysLong => sysLong.Split(' ').First());
                                    if (AreEquals(shortNames))
                                    {
                                        // у элемента несколько одинаковых систем
                                        systemCheck = shortNames.First().ToUpper();
                                    }
                                    else
                                    {
                                        // У экземпляра семейства присутствует несколько разных систем
                                        // и ни одна система не содержит букву из Excel
                                        errors.Add(el.Id);
                                        continue;
                                    }
                                }
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

                    if (!systemNameBefore.Equals(systemCheck))
                    {
                        el.LookupParameter(_parSystemName).Set(systemCheck);
                        count++;
                    }
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
            InvokeFullSystemNamesCmd(
                commandData,
                ref message,
                elements,
                _excelPath,
                2);


            CorrectGrouping(doc);

            return Result.Succeeded;
        }

        /// <summary>
        /// Возвращает, одинаковы ли строки в перечислении
        /// </summary>
        /// <param name="strings">Перечисление строк</param>
        /// <returns>True, если все строки одинаковые, иначе false</returns>
        private bool AreEquals(in IEnumerable<string> strings)
        {
            bool areEquals = false;
            string first = strings.First();
            foreach (string str in strings)
            {
                areEquals = first.Equals(str);
            }
            return areEquals;
        }


        /// <summary>
        /// Вызов команды "Полные имена систем"
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="message"></param>
        /// <param name="elements"></param>
        /// <param name="excelPath">Путь к файлу Excel</param>
        /// <param name="sheetNumber">Номер листа, скоторого читаются данные (начинается с 1)</param>
        private void InvokeFullSystemNamesCmd(
                ExternalCommandData commandData,
                ref string message,
                ElementSet elements,
                string excelPath,
                int sheetNumber)
        {
            var fullSystemNamesCmd = new FullSystemNamesCmd();
            fullSystemNamesCmd.Execute(
                commandData,
                ref message,
                elements,
                excelPath,
                sheetNumber);
        }

        /// <summary>
        /// Назначить элементам значение параметра ADSK_Группирование по значению категории
        /// </summary>
        /// <param name="doc">Документ, в котором корректируется группирование</param>
        private void CorrectGrouping(in Document doc)
        {
            var elems = GetMEPelements(doc, _categories);
            using (Transaction transGrouping = new Transaction(doc))
            {
                transGrouping.Start("Скорректировать ADSK_Группирование");
                foreach (Element elem in elems)
                {
                    var elId = elem.Id.IntegerValue;
                    BuiltInCategory category = (BuiltInCategory)elem.Category.Id.IntegerValue;
                    // 1 - экземпляр, 2 - тип, else - нет параметра
                    int groupParamPlace = 3;
                    if (!(elem is FamilyInstance inst1) ||
                        (elem is FamilyInstance
                        && !(inst1.get_Parameter(SharedParams.ADSK_Grouping) is null)))
                    {
                        groupParamPlace = 1;
                    }
                    else if (elem is FamilyInstance inst
                        && !(inst.Symbol.get_Parameter(SharedParams.ADSK_Grouping) is null))
                    {
                        groupParamPlace = 2;
                    }
                    else
                    {
                        continue;
                    }
                    if (groupParamPlace == 1)
                    {
                        bool isReadOnly = elem.get_Parameter(SharedParams.ADSK_Grouping).IsReadOnly;
                        if (!isReadOnly)
                        {
                            string grouping = elem.get_Parameter(SharedParams.ADSK_Grouping).AsValueString() ?? String.Empty;
                            switch (category)
                            {
                                case BuiltInCategory.OST_MechanicalEquipment:
                                    if (!grouping.Equals("1"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("1");
                                    break;
                                case BuiltInCategory.OST_PlumbingFixtures:
                                    if (!grouping.Equals("2"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("2");
                                    break;
                                case BuiltInCategory.OST_PipeAccessory:
                                    if (!grouping.Equals("3"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("3");
                                    break;
                                case BuiltInCategory.OST_PipeCurves:
                                    if (!grouping.Equals("4"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_PipeFitting:
                                    if (!grouping.Equals("4"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_FlexPipeCurves:
                                    if (!grouping.Equals("4"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_PipeInsulations:
                                    if (!grouping.Equals("5"))
                                        elem.get_Parameter(SharedParams.ADSK_Grouping).Set("5");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else if (groupParamPlace == 2)
                    {
                        var symb = (elem as FamilyInstance).Symbol;
                        bool isReadOnly = symb.get_Parameter(SharedParams.ADSK_Grouping).IsReadOnly;
                        if (!isReadOnly)
                        {
                            string grouping = symb.get_Parameter(SharedParams.ADSK_Grouping).AsValueString() ?? String.Empty;
                            switch (category)
                            {
                                case BuiltInCategory.OST_MechanicalEquipment:
                                    if (!grouping.Equals("1"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("1");
                                    break;
                                case BuiltInCategory.OST_PlumbingFixtures:
                                    if (!grouping.Equals("2"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("2");
                                    break;
                                case BuiltInCategory.OST_PipeAccessory:
                                    if (!grouping.Equals("3"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("3");
                                    break;
                                case BuiltInCategory.OST_PipeCurves:
                                    if (!grouping.Equals("4"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_PipeFitting:
                                    if (!grouping.Equals("4"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_FlexPipeCurves:
                                    if (!grouping.Equals("4"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("4");
                                    break;
                                case BuiltInCategory.OST_PipeInsulations:
                                    if (!grouping.Equals("5"))
                                        symb.get_Parameter(SharedParams.ADSK_Grouping).Set("5");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                transGrouping.Commit();
            }
        }
    }
}
