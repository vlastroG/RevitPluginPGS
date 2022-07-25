using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateImagesFromSections : IExternalCommand
    {
        private static string _prefix = "ПР-";


        /// <summary>
        /// Название временной папки
        /// </summary>
        private string _dirName = @"\ИзображенияПеремычек_deleteThis\";

        /// <summary>
        /// Путь к временной папке
        /// </summary>
        private string dirPath = String.Empty;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Созданные разрезы по перемычкам, имена начинаются с "ПР-".
            FilteredElementCollector sectionsFilter = new FilteredElementCollector(doc);
            var createdSections = sectionsFilter
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Where(v => (v as View).ViewType == ViewType.Section)
                .Where(v => v.Name.StartsWith(_prefix));

            // Созданные изображения перемычек, имена начинаются с "ПР-".
            FilteredElementCollector imagesFilter = new FilteredElementCollector(doc);
            var createdImagesNames = imagesFilter
                .OfCategory(BuiltInCategory.OST_RasterImages)
                .WhereElementIsElementType()
                .Where(v => v.Name.StartsWith(_prefix))
                .Select(v => v.Name);

            List<ElementId> sections;

            var userInput = UserInput.YesNoCancelInput("Обновление изображений перемычек",
                "Обновлять уже созданные изображения перемычек?");

            switch (userInput)
            {
                // Если обновлять все изображения
                case System.Windows.Forms.DialogResult.Yes:
                    // Все разрезы по перемычкам
                    sections = createdSections
                        .Select(s => s.Id)
                        .ToList();
                    break;

                // Если не обновлять изображения, а только добавлять новые
                case System.Windows.Forms.DialogResult.No:
                    // Разрезы по перемычкам, из которых еще не созданы изображения
                    sections = createdSections
                        .Where(s => !createdImagesNames.Contains(s.Name + ".png"))
                        .Select(s => s.Id)
                        .ToList();
                    break;

                default:
                    return Result.Cancelled;
            }

            // Включение весов линий
            bool thinLines = ThinLinesOptions.AreThinLinesEnabled;
            if (!thinLines)
                ThinLinesOptions.AreThinLinesEnabled = true;
            View activeViewWhenCommandStarted = uiapp.ActiveUIDocument.ActiveView;

            ImageExportOptions ieo = new ImageExportOptions()
            {
                ExportRange = ExportRange.CurrentView,
                FitDirection = FitDirectionType.Horizontal,
                ImageResolution = ImageResolution.DPI_600,
                ZoomType = ZoomFitType.Zoom,
                Zoom = 100,
                HLRandWFViewsFileType = ImageFileType.PNG
            };

            dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @_dirName;
            Directory.CreateDirectory(@dirPath);
            foreach (ElementId section in sections)
            {
                View sectionView = doc.GetElement(section) as View;
                uiapp.ActiveUIDocument.ActiveView = sectionView;

                ieo.FilePath = dirPath + sectionView.Name;
                string test = ieo.FilePath;
                doc.ExportImage(ieo);
                if (sectionView != activeViewWhenCommandStarted)
                {
                    // Закрыть открытый вид
                }    
            }
            uiapp.ActiveUIDocument.ActiveView = activeViewWhenCommandStarted;
            //using (Transaction trans = new Transaction(doc))
            //{
            //    trans.Start("Изображения перемычек");
            //    Directory.CreateDirectory(@dirPath);
            //    ieo.FilePath = dirPath;
            //    doc.ExportImage(ieo);

            //    trans.Commit();
            //}




            // Возвращение весов линий в изначальное состояние
            if (!thinLines)
                ThinLinesOptions.AreThinLinesEnabled = false;

            return Result.Succeeded;
        }
    }
}
