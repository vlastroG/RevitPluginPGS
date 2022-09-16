using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Utilites.DataParsers;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MS.Utilites.WorkWithParameters;
using static MS.Utilites.WorkWithPath;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SystemNameCorrectCmd : IExternalCommand
    {
        private static string _startPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private readonly List<BuiltInCategory> _categories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_PipeAccessory,
            BuiltInCategory.OST_MechanicalEquipment,
            BuiltInCategory.OST_PlumbingFixtures
        };

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
                string naming = excel.ReadCell(row, col);
                string system = excel.ReadCell(row, col + 1);
                while (!String.IsNullOrEmpty(naming) && !String.IsNullOrEmpty(system))
                {
                    namingAndSystemTuple.Add((naming, system));
                }
            }
            namingAndSystemTuple.DistinctBy(t => t.Naming).ToList();
            return namingAndSystemTuple;
        }

        /// <summary>
        /// Название параметра проекта ИмяСистемы
        /// </summary>
        private readonly string _parSystemName = "ИмяСистемы";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            List<(string Naming, string System)> namingAndSystemTuple = GetNamingAndSystemTuple();
            if (ReferenceEquals(namingAndSystemTuple, null)) return Result.Cancelled;


            throw new NotImplementedException();
        }
    }
}
