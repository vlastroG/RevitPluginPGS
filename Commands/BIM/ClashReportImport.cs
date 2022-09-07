using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private Result ErrorMessage()
        {
            MessageBox.Show("Xml файл поврежден!", "Ошибка!");
            return Result.Failed;
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            string path = String.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "XML файлы (*.xml)|*.xml";
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;
            }
            else
            {
                return Result.Cancelled;
            }
            if (!path.EndsWith(".xml"))
            {
                return ErrorMessage();
            }

            List<Clashresult> clashResults = new List<Clashresult>();
            XmlSerializer serializer = new XmlSerializer(typeof(Exchange));
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                try
                {
                    var exchange = (Exchange)serializer.Deserialize(fs);
                    clashResults = exchange.Batchtest.Clashtests.Clashtest.Clashresults.Clashresult;
                }
                catch (InvalidOperationException)
                {
                    return ErrorMessage();
                }
                catch (NullReferenceException)
                {
                    return ErrorMessage();
                }
            }

            using (Transaction placeFams = new Transaction(doc))
            {
                placeFams.Start("Размещение семейств коллизий");
                foreach (var clash in clashResults)
                {

                }
                placeFams.Commit();
            }

            return Result.Succeeded;
        }
    }
}
