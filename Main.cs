using Autodesk.Revit.UI;
using MS.Utilites;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;


namespace MS
{
    public class Main : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {

            CreatePanel(application);


            return Result.Succeeded;
        }

        /// <summary>
        /// Создание панели
        /// </summary>
        /// <param name="application"></param>
        public static void CreatePanel(UIControlledApplication application)
        {
            // Create Ribbon tab
            var path = Assembly.GetExecutingAssembly().Location;

            var assembly_dir = WorkWithPath.AssemblyDirectory;

            // Создание вкладки в ленте
            application.CreateRibbonTab("PGS-BIM");
            // Инфо раздел
            RibbonPanel panelGeneral = application.CreateRibbonPanel("PGS-BIM", "Общее");
            // Раздел АР
            RibbonPanel panelAR = application.CreateRibbonPanel("PGS-BIM", "Раздел АР");
            // Раздел КР
            //RibbonPanel panelKR = application.CreateRibbonPanel("PGS-BIM", "Раздел КР");
            // Раздел СС
            // RibbonPanel panelSS = application.CreateRibbonPanel("PGS-BIM", "Раздел СС");
            // Раздел ОВиК
            RibbonPanel panelOVVK = application.CreateRibbonPanel("PGS-BIM", "Раздел ОВиК");


            // Info command
            PushButtonData btnInfo = new PushButtonData("Info", "Info", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;


            // LevelName command
            PushButtonData btnLevelName = new PushButtonData("LevelName", "Отметки\nуровней", path, "MS.Commands.General.LevelName");
            Uri btnLevelNameImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnLevelNameImage = new BitmapImage(btnLevelNameImagePath);
            btnLevelName.LargeImage = btnLevelNameImage;


            // SelectionRooms command
            PushButtonData btnSelection = new PushButtonData("Selection", "Выбор\nпомещений", path, "MS.Selector");
            Uri btnSelectionImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Selection.png");
            BitmapImage btnSelectionImage = new BitmapImage(btnSelectionImagePath);
            btnSelection.LargeImage = btnSelectionImage;

            // MasonryMesh command
            PushButtonData btnMasonryMesh = new PushButtonData("MasonryMesh", "Кладочная\nсетка", path, "MS.Commands.AR.MasonryMesh");
            Uri btnMasonryMeshImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Selection.png");
            BitmapImage btnMasonryMeshImage = new BitmapImage(btnMasonryMeshImagePath);
            btnMasonryMesh.LargeImage = btnMasonryMeshImage;


            // RoomsAreaPGS command
            PushButtonData btnRmArea = new PushButtonData("RoomsAreaNew", "Квартирография\nPGS_temp", path, "MS.Commands.AR.RoomsAreaNew");
            Uri btnRmAreaImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsArea.png");
            BitmapImage btnRmAreaImage = new BitmapImage(btnRmAreaImagePath);
            btnRmArea.LargeImage = btnRmAreaImage;

            // ComplexApartmentNumber command
            PushButtonData btnCmplxAprtNum = new PushButtonData("ComplexAprtmntNmbr", "Составные номера\nквартир", path, "MS.Commands.AR.ComplexAprtmntNmbr");
            Uri btnCmplxAprtNumImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnCmplxAprtNumImage = new BitmapImage(btnCmplxAprtNumImagePath);
            btnCmplxAprtNum.LargeImage = btnCmplxAprtNumImage;

            // OpeningsArea command
            PushButtonData btnOpeningsArea = new PushButtonData("OpeningsArea", "Площадь проемов\nв помещениях", path, "MS.Commands.AR.OpeningsArea");
            Uri btnOpeningsAreaImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnOpeningsAreaImage = new BitmapImage(btnOpeningsAreaImagePath);
            btnOpeningsArea.LargeImage = btnOpeningsAreaImage;

            // MarkLintelsInOpenings command
            PushButtonData btnMarkLintelsInOpenings = new PushButtonData("MarkLintelsInOpenings", "Маркировать перемычки\nокон и дверей", path, "MS.Commands.AR.MarkLintelsInOpenings");
            Uri btnMarkLintelsInOpeningsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnMarkLintelsInOpeningsImage = new BitmapImage(btnMarkLintelsInOpeningsImagePath);
            btnMarkLintelsInOpenings.LargeImage = btnMarkLintelsInOpeningsImage;

            // LintelsSections command
            PushButtonData btnLintelsSections = new PushButtonData("LintelsSections", "Разрезы\nпо перемычкам", path, "MS.Commands.AR.CreateSectionsByLintels");
            Uri btnLintelsSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnLintelsSectionsImage = new BitmapImage(btnLintelsSectionsImagePath);
            btnLintelsSections.LargeImage = btnLintelsSectionsImage;

            // CreateImagesFromSections command
            PushButtonData btnCreateImagesFromSections = new PushButtonData("CreateImagesFromSections", "Ведомость\nперемычек", path, "MS.Commands.AR.CreateImagesFromSections");
            Uri btnCreateImagesFromSectionsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnCreateImagesFromSectionsImage = new BitmapImage(btnCreateImagesFromSectionsImagePath);
            btnCreateImagesFromSections.LargeImage = btnCreateImagesFromSectionsImage;

            // RoomsFinishingMultiMark command
            PushButtonData btnRoomsFinishingMultiMark = new PushButtonData("RoomsFinishingMultiMark", "Помещения\nс одинаковой отделкой", path, "MS.Commands.AR.RoomsFinishingMultiMark");
            Uri btnRoomsFinishingMultiMarkImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnRoomsFinishingMultiMarkImage = new BitmapImage(btnRoomsFinishingMultiMarkImagePath);
            btnRoomsFinishingMultiMark.LargeImage = btnRoomsFinishingMultiMarkImage;


            // RoomsFinishingCommand
            //PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing\nDebug", "Отделка\n(Beta)", path, "MS.Utilites.GetBoundarySegmentElement");
            //Uri btnRmFinishingImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsFinishing.png");
            //BitmapImage btnRmFinishingImage = new BitmapImage(btnRmFinishingImagePath);
            //btnRmFinishing.LargeImage = btnRmFinishingImage;


            // MaterialColorsCommand
            PushButtonData btnMaterialColors = new PushButtonData("MaterialColors", "Обновить\nцвета", path, "MS.Commands.AR.MaterialColors");
            Uri btnMaterialColorsImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\MaterialColors.png");
            BitmapImage btnMaterialColorsImage = new BitmapImage(btnMaterialColorsImagePath);
            btnMaterialColors.LargeImage = btnMaterialColorsImage;


            // StairReinforcement command
            PushButtonData stairRnfrcmtCmd = new PushButtonData("StairReinforcement", "Армирование\nлестниц", path, "MS.Commands.KR.StairReinforcement");
            Uri stairRnfrcmtCmdImagePath = new Uri(assembly_dir + @"\Images\Icons\KR\StairRnfrcmt.png");
            BitmapImage stairRnfrcmtCmdImage = new BitmapImage(stairRnfrcmtCmdImagePath);
            stairRnfrcmtCmd.LargeImage = stairRnfrcmtCmdImage;


            // SS Numerator command
            //PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            //Uri btnNumeratorImagePath = new Uri(assembly_dir + @"\Images\Icons\SS\СС.png");
            //BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            //btnNumerator.LargeImage = btnNumeratorImage;

            // ОВиК Numerator command
            PushButtonData btnPipelineFittings = new PushButtonData("PipelineFittings", "Арматура\nтрубопроводов", path, "MS.Commands.MEP.PipelineFittings");
            Uri btnPipelineFittingsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnPipelineFittingsImage = new BitmapImage(btnPipelineFittingsImagePath);
            btnPipelineFittings.LargeImage = btnPipelineFittingsImage;


            // General panel
            panelGeneral.AddItem(btnInfo);
            panelGeneral.AddItem(btnLevelName);

            // AR panel
            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnCmplxAprtNum);
            panelAR.AddItem(btnOpeningsArea);
            panelAR.AddItem(btnMarkLintelsInOpenings);
            panelAR.AddItem(btnLintelsSections);
            panelAR.AddItem(btnCreateImagesFromSections);
            panelAR.AddItem(btnRoomsFinishingMultiMark);
            //panelAR.AddItem(btnRmFinishing); // в разработке, нужно указать названия параметров
            panelAR.AddItem(btnSelection);
            panelAR.AddItem(btnMasonryMesh);
            panelAR.AddItem(btnMaterialColors);

            // KR panel
            //panelKR.AddItem(stairRnfrcmtCmd); // в разработке (сырая)

            // SS panel
            // panelSS.AddItem(btnNumerator);

            // ОВиК panel
            panelOVVK.AddItem(btnPipelineFittings);
        }
    }
}
