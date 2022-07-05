using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using MS.Utilites;
using System.IO;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MaterialColors : IExternalCommand
    {
        private string _parName = "Изображение типоразмера материала";

        private string _dirName = "MaterialColors_deleteThis";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector materialCollector = new FilteredElementCollector(doc);
            List<Element> materials = materialCollector
                .OfCategory(BuiltInCategory.OST_Materials)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            int updateMaterials = 0;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Изображения материалов");
                try
                {

                    foreach (Material elem in materials)
                    {
                        string materialName = elem.Name.GetHashCode().ToString();


                        int width = 360;
                        int height = 86;
                        float dpi = 144;

                        Color color = elem.Color;
                        int red = color.Red;
                        int green = color.Green;
                        int blue = color.Blue;

                        if (elem.LookupParameter(_parName) != null
                            && elem.LookupParameter(_parName).AsElementId().IntegerValue > 1)
                        {
                            try
                            {
                                ElementId imgId = elem.LookupParameter(_parName).AsElementId();
                                var _color = (doc.GetElement(imgId) as ImageType).GetImage().GetPixel(1, 1);
                                var r = _color.R;
                                var g = _color.G;
                                var b = _color.B;
                                if (red == r && green == g && blue == b)
                                {
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }

                        string @dirPath = @_dirName;
                        string @filePath = @dirPath + @"\" + @materialName + ".png";

                        WorkWithGeometry.CreateColoredRectanglePng(
                            width,
                            height,
                            dpi,
                            red,
                            green,
                            blue,
                            dirPath,
                            filePath);

                        var _imageType = ImageType.Create(
                            doc,
                            new ImageTypeOptions(
                                path: filePath,
                                true,
                                ImageTypeSource.Import));
                        elem.LookupParameter(_parName).Set(_imageType.Id);
                        updateMaterials++;
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                trans.Commit();
            }
            string rvtDirPath = new FileInfo(doc.PathName).DirectoryName;
            string _fullDitName = rvtDirPath + @"\" + _dirName;

            if (updateMaterials == 0)
                MessageBox.Show($"Обновлено {updateMaterials} материалов.");
            else
                MessageBox.Show($"Обновлено {updateMaterials} материалов.\n\nМожете удалить временную папку\n{_fullDitName}\nи ее содержимое.");

            return Result.Succeeded;
        }
    }
}
