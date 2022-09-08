using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Xml2CSharp;

namespace MS.Commands.BIM
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ClashReportImport : IExternalCommand
    {
        private Result ErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка!");
            return Result.Failed;
        }

        private readonly string _assemblyDir = WorkWithPath.AssemblyDirectory;

        private readonly string _famPath = @"\EmbeddedFamilies\ABG_GEN_ClashSphere_R22.rfa";

        private const string _familyName = "ABG_GEN_ClashSphere_R22";

        private const string _name = "DoNotEditManually";

        private const string _resultStatuses = "СоздатьАктивн.";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var clashFamSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == _familyName && ft.Name == _name);
            if (clashFamSymb == null)
            {
                bool isSuccess;
                var @familyPath = _assemblyDir + _famPath;
                using (Transaction loadFamily = new Transaction(doc))
                {
                    loadFamily.Start("Loaded clash family");
                    isSuccess = doc.LoadFamilySymbol(familyPath, _name);
                    loadFamily.Commit();
                }
                if (!isSuccess)
                {
                    return ErrorMessage($"Ошибка загрузки семейства из {familyPath}!");
                }
                clashFamSymb = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .WhereElementIsElementType()
                    .Cast<FamilySymbol>()
                    .FirstOrDefault(ft => ft.FamilyName == _familyName && ft.Name == _name);
                if (clashFamSymb == null)
                {
                    throw new InvalidOperationException(
                        "Семейство для коллизий загрузилось, но его нельзя найти в проекте!");
                }
            }
            string xmlPath = String.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "XML файлы (*.xml)|*.xml",
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Выберите xml отчет о коллизиях"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                xmlPath = openFileDialog.FileName;
            }
            else
            {
                return Result.Cancelled;
            }
            if (!xmlPath.EndsWith(".xml"))
            {
                return ErrorMessage("Неверный формат файла!");
            }

            List<Clashresult> clashResults = new List<Clashresult>();
            XmlSerializer serializer = new XmlSerializer(typeof(Exchange));
            using (FileStream fs = new FileStream(xmlPath, FileMode.Open))
            {
                try
                {
                    var exchange = (Exchange)serializer.Deserialize(fs);
                    clashResults = exchange.Batchtest.Clashtests.Clashtest.Clashresults.Clashresult
                        .Where(result => _resultStatuses.Contains(result.Resultstatus))
                        .ToList();
                }
                catch (InvalidOperationException)
                {
                    return ErrorMessage("Xml файл поврежден, нельзя определить clashresult!");
                }
                catch (NullReferenceException)
                {
                    return ErrorMessage("Xml файл поврежден, нельзя определить clashresult!");
                }
            }
            var count = 0;
            using (Transaction placeFams = new Transaction(doc))
            {
                placeFams.Start("Placed clash families");
                foreach (var clash in clashResults)
                {
                    XYZ center = new XYZ(
                        Double.Parse(clash.Clashpoint.Pos3f.X, CultureInfo.InvariantCulture) / SharedValues.FootToMillimeters * 1000,
                        Double.Parse(clash.Clashpoint.Pos3f.Y, CultureInfo.InvariantCulture) / SharedValues.FootToMillimeters * 1000,
                        Double.Parse(clash.Clashpoint.Pos3f.Z, CultureInfo.InvariantCulture) / SharedValues.FootToMillimeters * 1000
                    );
                    Element clashEl = WorkWithFamilies.CreateAdaptiveComponentInstance(doc, clashFamSymb, center);
                    count++;
                    clashEl.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(clash.Assignedto);
                    try
                    {
                        clashEl.get_Parameter(SharedParams.ADSK_ItemCode).Set(clash.Clashobjects.Clashobject[0].Objectattribute.Value);
                        clashEl.get_Parameter(SharedParams.ADSK_Name).Set(clash.Clashobjects.Clashobject[0].Smarttags.Smarttag[0].Value);
                        clashEl.get_Parameter(SharedParams.ADSK_Position).Set(clash.Clashobjects.Clashobject[1].Objectattribute.Value);
                        clashEl.get_Parameter(SharedParams.ADSK_Designation).Set(clash.Clashobjects.Clashobject[1].Smarttags.Smarttag[0].Value);
                        clashEl.LookupParameter("ClashX").Set(center.X);
                        clashEl.LookupParameter("ClashY").Set(center.Y);
                        clashEl.LookupParameter("ClashZ").Set(center.Z);
                    }
                    catch (NullReferenceException)
                    {
                        throw new ArgumentException(
                            "Семейство некорректно! Нельзя назначить значения параметров!");
                    }
                    catch (IndexOutOfRangeException)
                    {

                    }
                }
                placeFams.Commit();
            }
            MessageBox.Show($"Размещено {count} экземпляров семейств коллизий. " +
                $"Семейства размещаются только для коллизий статусов 'Создать' и 'Активн.'. " +
                $"\nОтветственный записан в 'Комментарии'" +
                $"\nid1 записан в 'ADSK_Код изделия'" +
                $"\nid2 записан в 'ADSK_Позиция'" +
                $"\nname1 записано в 'ADSK_Наименование'" +
                $"\nname2 записано в 'ADSK_Обозначение'", "Clashes import");
            return Result.Succeeded;
        }
    }
}
