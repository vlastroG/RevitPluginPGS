using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

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

        /// <summary>
        /// Значение встроенного параметра типа "Описание" для семейств перемычек
        /// </summary>  
        private string _lintelDescription = "Перемычка";

        private DirectoryInfo CreateDirAndImgs(
            ExternalCommandData commandData,
            List<ElementId> sections)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

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

            // Включение весов линий
            bool thinLines = ThinLinesOptions.AreThinLinesEnabled;
            if (!thinLines)
                ThinLinesOptions.AreThinLinesEnabled = true;

            dirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @_dirName;
            DirectoryInfo temporaryFolder = Directory.CreateDirectory(@dirPath);
            foreach (ElementId section in sections)
            {
                View sectionView = doc.GetElement(section) as View;
                uiapp.ActiveUIDocument.ActiveView = sectionView;
                ieo.FilePath = dirPath + sectionView.Name;
                doc.ExportImage(ieo);
                if (sectionView != activeViewWhenCommandStarted)
                {
                    // Закрыть открытый вид
                }
            }

            if (activeViewWhenCommandStarted.ViewType != ViewType.Internal
                && activeViewWhenCommandStarted.ViewType != ViewType.Undefined)
            {
                uiapp.ActiveUIDocument.ActiveView = activeViewWhenCommandStarted;
            }

            // Возвращение весов линий в изначальное состояние
            if (!thinLines)
                ThinLinesOptions.AreThinLinesEnabled = false;
            return temporaryFolder;
        }

        private void LoadImgsFromDirToDocument(
            Document doc,
            DirectoryInfo temporaryFolder,
            IEnumerable<Element> createdImages,
            IEnumerable<string> createdImagesNames,
            ref int loadedImgs,
            ref int updatedImgs)
        {
            var files = temporaryFolder.GetFiles("ПР*.png");

            foreach (FileInfo file in files)
            {
                bool isDocContainsImage = createdImagesNames.Contains(file.Name);
                ImageTypeOptions ito = new ImageTypeOptions(
                    file.FullName,
                    false,
                    ImageTypeSource.Import);

                // Если документ содержит изображение перемычки
                if (isDocContainsImage)
                {
                    createdImages.Where(i => i.Name == file.Name)
                        .Cast<ImageType>()
                        .FirstOrDefault()
                        .ReloadFrom(ito);
                    updatedImgs++;
                }
                // Если документ не содержит изображение перемычки
                else
                {
                    ImageType.Create(doc, ito);
                    loadedImgs++;
                }
            }
        }


        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int updateImgs = 0;
            int newLoadedImgs = 0;
            int updatedLintels = 0;

            // Созданные разрезы по перемычкам, имена начинаются с "ПР-".
            var createdSections = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Where(v => (v as View).ViewType == ViewType.Section)
                .Where(v => v.Name.StartsWith(_prefix));

            // Созданные изображения перемычек, имена начинаются с "ПР-".
            var createdImages = new FilteredElementCollector(doc)
                .OfClass(typeof(ImageType))
                .WhereElementIsElementType()
                .Where(v => v.Name.StartsWith(_prefix));

            var createdImagesNames = createdImages.Select(v => v.Name);

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

            DirectoryInfo temporaryFolder = CreateDirAndImgs(commandData, sections);

            using (Transaction transImgsLoading = new Transaction(doc))
            {
                transImgsLoading.Start("PGS_LoadLintelsImgs");

                LoadImgsFromDirToDocument(
                    doc,
                    temporaryFolder,
                    createdImages,
                    createdImagesNames,
                    ref newLoadedImgs,
                    ref updateImgs);

                transImgsLoading.Commit();
            }

            var lintels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .Where(e => (e is FamilyInstance) &&
                            (e as FamilyInstance).Symbol
                            .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                            .AsValueString() == _lintelDescription)
                .Where(e => e.get_Parameter(SharedParams.PGS_MarkLintel) != null &&
                            !String.IsNullOrEmpty(e.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString()))
                .Cast<FamilyInstance>();

            var loadedImgs = new FilteredElementCollector(doc)
                .OfClass(typeof(ImageType))
                .WhereElementIsElementType()
                .Where(v => v.Name.StartsWith(_prefix));

            using (Transaction transSetImgs = new Transaction(doc))
            {
                transSetImgs.Start("PGS_SetLintelsImgs");

                foreach (FamilyInstance lintel in lintels)
                {
                    string lintelMarkImg = lintel.get_Parameter(SharedParams.PGS_MarkLintel)
                        .AsValueString() + '_' + doc.GetElement(lintel.LevelId).Name + ".png";
                    //var lintelImg = lintel.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE);
                    var lintelImg = lintel.LookupParameter("PGS_Изображение");
                    if (lintelImg.AsElementId() == null || lintelImg.AsValueString() != lintelMarkImg)
                    {
                        Element img = loadedImgs.Where(i => i.Name == lintelMarkImg).FirstOrDefault();
                        //lintel.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).Set(img.Id);
                        if (img != null)
                        {
                            lintel.LookupParameter("PGS_Изображение").Set(img.Id);
                            updatedLintels++;
                        }
                    }
                }

                transSetImgs.Commit();
            }

            temporaryFolder.Delete(true);

            MessageBox.Show(
                $"Загружено {newLoadedImgs} изображений перемычек;" +
                $"\nОбновлено {updateImgs} изображений перемычек;" +
                $"\nНазначено {updatedLintels} изображений перемычкам." +
                $"\n\nПримечание:" +
                $"\nИзображения назначаются только тем перемычкам," +
                $"\nу которых задан параметр PGS_МаркаПеремычки" +
                $"\nи для которых созданы разрезы." +
                $"\nЕсли первое поле названия изображения совпадает с PGS_МаркаПеремычки," +
                $"\nто переназначаться перемычке оно не будет.",
                "Изображения перемычек");


            return Result.Succeeded;
        }
    }
}
