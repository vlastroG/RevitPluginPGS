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
        /// <summary>
        /// Название временной папки
        /// </summary>
        private string _dirName = @"\ИзображенияПеремычек_deleteThis\";

        /// <summary>
        /// Путь к временной папке
        /// </summary>
        private string dirPath = String.Empty;

        /// <summary>
        /// Создать временную папку и изображения. Названия изображений совпадают с названиями полученных разрезов.
        /// </summary>
        /// <param name="commandData"></param>
        /// <param name="sections">Разрезы, из которых нужно сделать изображения</param>
        /// <returns>Временная папка, в которой расположены разрезы.</returns>
        private DirectoryInfo CreateDirAndImgs(
            ExternalCommandData commandData,
            ElementId[] sections)
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


            DirectoryInfo temporaryFolder = WorkWithPath.CreateTempDir(@dirPath);
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
                try
                {
                    uiapp.ActiveUIDocument.ActiveView = activeViewWhenCommandStarted;
                }
                catch (ArgumentException)
                {
                    // Cannot make an internal view active
                }
            }

            // Возвращение весов линий в изначальное состояние
            if (!thinLines)
                ThinLinesOptions.AreThinLinesEnabled = false;
            return temporaryFolder;
        }

        /// <summary>
        /// Загружает изображения в проект из папки, если таких изображений (названий) в проекте Revit еще нет,
        /// или обновляет созданные изображения из этой папки, если они присутствуют в проекте.
        /// </summary>
        /// <param name="doc">Проект Revit</param>
        /// <param name="temporaryFolder">Временная папка с изображениями</param>
        /// <param name="createdImages">Список изображений в проекте Revit</param>
        /// <param name="loadedImgs">Счетчик загруженных изображений</param>
        /// <param name="updatedImgs">Счетчик обновленных изображений</param>
        private void LoadImgsFromDirToDocument(
            Document doc,
            DirectoryInfo temporaryFolder,
            IEnumerable<Element> createdImages,
            ref int loadedImgs,
            ref int updatedImgs)
        {
            var files = temporaryFolder.GetFiles("*.png");
            var createdImagesNames = createdImages.Select(v => v.Name).ToArray();

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

            Guid[] _sharedParamsForCommand = new Guid[] { SharedParams.PGS_ImageTypeMaterial };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_GenericModel,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Обобщенная модель\" " +
                    "отсутствует общий параметр PGS_ИзображениеТипоразмераМатериала" +
                    "\nс Guid = 924e3bb2-a048-449f-916f-31093a3aa7a3",
                    "Ошибка");
                return Result.Cancelled;
            }

            int updateImgs = 0;
            int newLoadedImgs = 0;
            int updatedLintels = 0;
            int setLintels = 0;

            var addLevelResult = UserInput.YesNoCancelInput("Изображения по перемычкам", "Если считать перемычки поэтажно - \"Да\", иначе - \"Нет\"");
            if (addLevelResult != System.Windows.Forms.DialogResult.Yes && addLevelResult != System.Windows.Forms.DialogResult.No)
            {
                return Result.Cancelled;
            }
            bool addLevel;
            if (addLevelResult == System.Windows.Forms.DialogResult.Yes)
            {
                addLevel = true;
            }
            else
            {
                addLevel = false;
            }

            var lintels = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .Where(e => (e is FamilyInstance) &&
                            (e as FamilyInstance).Symbol
                            .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                            .AsValueString() == SharedValues.LintelDescription)
                .Cast<FamilyInstance>()
                .ToArray();

            var lintelsUniqueNames = lintels
                .Select(l => WorkWithFamilies.GetLintelUniqueName(l, addLevel))
                .ToArray();

            var lintelsUniqueNamesPngSuff = lintelsUniqueNames.Select(name => name + ".png");

            // Созданные разрезы по перемычкам, имена совпадают с уникальными именами перемычек.
            var createdSections = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Where(v => (v as View).ViewType == ViewType.Section)
                .Where(v => lintelsUniqueNames.Contains(v.Name))
                .ToArray();

            // Созданные изображения перемычек, имена совпадают с уникальными именами перемычек.
            var createdImages = new FilteredElementCollector(doc)
                .OfClass(typeof(ImageType))
                .WhereElementIsElementType()
                .Where(img => lintelsUniqueNamesPngSuff.Contains(img.Name))
                .ToArray();
            var createdImagesNames = createdImages.Select(img => img.Name)
                .ToArray();


            ElementId[] sections;

            var userInput = UserInput.YesNoCancelInput("Обновление изображений перемычек",
                "Обновлять уже загруженные изображения перемычек?");

            switch (userInput)
            {
                // Если обновлять все изображения, то изображения создаются из всех разрезов по перемычкам.
                case System.Windows.Forms.DialogResult.Yes:
                    // Все разрезы по перемычкам
                    sections = createdSections
                        .Select(s => s.Id)
                        .ToArray();
                    break;

                // Если не обновлять изображения, а только добавлять новые,
                // то изображения создаются только из тех разрезов,
                // из которых еще не были созданы изображения.
                case System.Windows.Forms.DialogResult.No:
                    // Разрезы по перемычкам, из которых еще не созданы изображения
                    sections = createdSections
                        .Where(s => !createdImagesNames.Contains(s.Name + ".png"))
                        .Select(s => s.Id)
                        .ToArray();
                    break;

                default:
                    return Result.Cancelled;
            }

            if (sections.Length == 0)
            {
                MessageBox.Show(
                    "Не будет загружено или обновлено ни одно изображение перемычки." +
                    "\n\nЛибо из всех разрезов по перемычкам уже созданы изображения," +
                    "\nлибо разрезы сделаны для другого типа подсчета (поэтажного/сквозного)." +
                    "\nИспользуйте команду \"Разрезы по перемычкам\", " +
                    "не забыв про поэтажный/сквозной подсчет перемычек.",
                    "Предупреждение");
            }

            DirectoryInfo temporaryFolder = CreateDirAndImgs(commandData, sections);

            using (Transaction transImgsLoading = new Transaction(doc))
            {
                transImgsLoading.Start("PGS_LoadLintelsImgs");

                LoadImgsFromDirToDocument(
                    doc,
                    temporaryFolder,
                    createdImages,
                    ref newLoadedImgs,
                    ref updateImgs);

                transImgsLoading.Commit();
            }

            using (Transaction transSetImgs = new Transaction(doc))
            {
                transSetImgs.Start("PGS_SetLintelsImgs");

                var loadedUpdatedImages = new FilteredElementCollector(doc)
                .OfClass(typeof(ImageType))
                .WhereElementIsElementType()
                .Where(img => lintelsUniqueNamesPngSuff.Contains(img.Name))
                .ToArray();
                foreach (FamilyInstance lintel in lintels)
                {
                    string lintelUniqueNamePngSuff = WorkWithFamilies.GetLintelUniqueName(lintel, addLevel) + ".png";
                    var lintelImg = lintel.get_Parameter(SharedParams.PGS_ImageTypeMaterial);
                    if (lintelImg.AsElementId().IntegerValue == -1)
                    {
                        Element img = loadedUpdatedImages.Where(i => i.Name == lintelUniqueNamePngSuff).FirstOrDefault();
                        if (img != null)
                        {
                            lintel.get_Parameter(SharedParams.PGS_ImageTypeMaterial).Set(img.Id);
                            setLintels++;
                            continue;
                        }
                    }
                    if (lintelImg.AsValueString() != lintelUniqueNamePngSuff)
                    {
                        Element img = loadedUpdatedImages.Where(i => i.Name == lintelUniqueNamePngSuff).FirstOrDefault();
                        if (img != null)
                        {
                            lintel.get_Parameter(SharedParams.PGS_ImageTypeMaterial).Set(img.Id);
                            updatedLintels++;
                            continue;
                        }
                    }
                }

                transSetImgs.Commit();
            }

            temporaryFolder.Delete(true);

            MessageBox.Show(
                $"Загружено {newLoadedImgs} изображений перемычек;" +
                $"\nОбновлено {updateImgs} изображений перемычек;" +
                $"\nПереназначено {updatedLintels} изображений перемычкам;" +
                $"\nНазначено {setLintels} изображений перемычкам." +
                $"\n\nПримечание:" +
                $"\n\nИзображения назначаются только для обобщенных моделей, " +
                $"в типоразмере которых в описании написано \"Перемычка\"." +
                $"\nИзображения назначаются для перемычек с уникальным набором значений параметров:" +
                $"\n1) ADSK_Толщина стены, находящегося в экземпляре перемычки " +
                $"или в экземпляре родительского семейства;" +
                $"\n2) ADSK_Наименование всех вложенных элементов перемычки, которые параллельны стене;" +
                $"\n3) Ширине перемычки, вычисленной как расстояние между центрами крайних элементов из п.2)," +
                $"спроецированное на поперечную ось перемычки." +
                $"\n4)* Если перемычки считаются поэтажно, то к этому перечню параметров добавляется Уровень.",
                "Изображения перемычек");


            return Result.Succeeded;
        }
    }
}
