using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MaterialColors : IExternalCommand
    {
        /// <summary>
        /// Название временной папки
        /// </summary>
        private string _dirName = @"\MaterialColors_deleteThis\";

        /// <summary>
        /// Путь к временной папке
        /// </summary>
        private string dirPath = String.Empty;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.PGS_ImageTypeMaterial
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_Materials,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Материалы\" " +
                    "отсутствует необходимый общий параметр:" +
                    "\nPGS_ИзображениеТипоразмераМатериала",
                    "Ошибка");
                return Result.Cancelled;
            }

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

                        if (elem.get_Parameter(SharedParams.PGS_ImageTypeMaterial) != null
                            && elem.get_Parameter(SharedParams.PGS_ImageTypeMaterial).AsElementId().IntegerValue > 1)
                        {
                            try
                            {
                                ElementId imgId = elem.get_Parameter(SharedParams.PGS_ImageTypeMaterial).AsElementId();
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

                        dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @_dirName;
                        string @filePath = @dirPath + @materialName + ".png";

                        WorkWithGeometry.CreateColoredRectanglePng(
                            width,
                            height,
                            dpi,
                            red,
                            green,
                            blue,
                            dirPath,
                            filePath);

                        ImageTypeOptions opt = new ImageTypeOptions(
                                @filePath,
                                false,
                                ImageTypeSource.Import);

                        var _imageType = ImageType.Create(
                            doc,
                            opt);
                        elem.get_Parameter(SharedParams.PGS_ImageTypeMaterial).Set(_imageType.Id);
                        updateMaterials++;
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                trans.Commit();
            }


            if (updateMaterials == 0)
                MessageBox.Show($"Обновлено {updateMaterials} материалов.");
            else
                MessageBox.Show($"Обновлено {updateMaterials} материалов.\n\nМожете удалить временную папку\n{dirPath}\nи ее содержимое.");

            return Result.Succeeded;
        }
    }
}
