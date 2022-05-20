using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using MS.Utilites;


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
            // Раздел СС
            // RibbonPanel panelSS = application.CreateRibbonPanel("PGS-BIM", "Раздел СС");
            // Раздел ОВиК
            RibbonPanel panelOVVK = application.CreateRibbonPanel("PGS-BIM", "Раздел ОВиК");
            // Раздел FUN
            // RibbonPanel panelFun = application.CreateRibbonPanel("PGS-BIM", "FUN");

            // Info command
            PushButtonData btnInfo = new PushButtonData("Info", "Info", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;
            btnInfo.ToolTip = "Выбор элементов по заданной категории. Категория берется из первого выбранного элемента, затем рамкой выбираются все элементы заданной категории";

            // Selection command
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


            // RoomsFinishingCommand
            PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing", "Отделка", path, "MS.RoomsFinishing");
            Uri btnRmFinishingImagePath = new Uri(assembly_dir + @"\Images\Icons\AR\RoomsFinishing.png");
            BitmapImage btnRmFinishingImage = new BitmapImage(btnRmFinishingImagePath);
            btnRmFinishing.LargeImage = btnRmFinishingImage;


            // SS Numerator command
            PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            Uri btnNumeratorImagePath = new Uri(assembly_dir + @"\Images\Icons\SS\СС.png");
            BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            btnNumerator.LargeImage = btnNumeratorImage;

            // ОВиК Numerator command
            PushButtonData btnPipelineFittings = new PushButtonData("PipelineFittings", "Арматура\nтрубопроводов", path, "MS.Commands.MEP.PipelineFittings");
            Uri btnPipelineFittingsImagePath = new Uri(assembly_dir + @"\Images\Icons\General\Info.png");
            BitmapImage btnPipelineFittingsImage = new BitmapImage(btnPipelineFittingsImagePath);
            btnPipelineFittings.LargeImage = btnPipelineFittingsImage;


            // Fun command
            //PushButtonData btnFun = new PushButtonData("Fun", "Fun", path, "MS.Commands.Fun.Picture");
            //Uri btnFunImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\AR\RoomsFinishing.png");
            //BitmapImage btnFunImage = new BitmapImage(btnFunImagePath);
            //btnFun.LargeImage = btnFunImage;
            //btnFun.ToolTip = "Fun picture";

            // General panel
            panelGeneral.AddItem(btnInfo);
            //panelGeneral.AddItem(btnSelection);

            // AR panel
            panelAR.AddItem(btnRmArea);
            //panelAR.AddItem(btnRmFinishing);
            panelAR.AddItem(btnSelection);
            panelAR.AddItem(btnMasonryMesh);

            // SS panel
            // panelSS.AddItem(btnNumerator);

            // ОВиК panel
            panelOVVK.AddItem(btnPipelineFittings);

            // Fun panel
            //panelFun.AddItem(btnFun);

        }
    }
}
