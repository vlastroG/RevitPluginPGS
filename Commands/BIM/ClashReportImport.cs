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
        private void ErrorMessage()
        {
            MessageBox.Show("Xml файл поврежден!", "Ошибка!");
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Filter = "XML файлы (*.xml)|*.xml";
            openFileDialog.Multiselect = false;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
            }

            var filePath = @"C:\Users\stroganov.vg\Desktop\КР_Плиты перекрытия-ОВ_Воздуховоды.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(Exchange));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    var exchange = (Exchange)serializer.Deserialize(fs);
                    var clashTests = exchange.Batchtest.Clashtests.Clashtest.Clashresults.Clashresult;
                    var point = clashTests[0].Clashpoint.Pos3f;
                }
                catch (InvalidOperationException)
                {
                    ErrorMessage();
                    return Result.Failed;
                }
                catch (NullReferenceException)
                {
                    ErrorMessage();
                    return Result.Failed;
                }
            }
            var b = 9;
            return Result.Succeeded;
        }
    }
}
