using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

            // Создание вкладки в ленте
            application.CreateRibbonTab("PGS-BIM");
            // Инфо раздел
            RibbonPanel panelGeneral = application.CreateRibbonPanel("PGS-BIM", "Общее");
            // Раздел АР
            RibbonPanel panelAR = application.CreateRibbonPanel("PGS-BIM", "Раздел АР");
            // Раздел СС
            RibbonPanel panelSS = application.CreateRibbonPanel("PGS-BIM", "Раздел СС");

            // Info command
            PushButtonData btnInfo = new PushButtonData("Info", "Info", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\General\Info.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;
            btnInfo.ToolTip = "Выбор элементов по заданной категории. Категория берется из первого выбранного элемента, затем рамкой выбираются все элементы заданной категории";

            // Selection command
            PushButtonData btnSelection = new PushButtonData("Selection", "Выбор", path, "MS.Selector");
            Uri btnSelectionImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\General\Selection.png");
            BitmapImage btnSelectionImage = new BitmapImage(btnSelectionImagePath);
            btnSelection.LargeImage = btnSelectionImage;


            // RoomsAreaPGS command
            PushButtonData btnRmArea = new PushButtonData("RoomsArea", "Квартирография\nPGS_temp", path, "MS.RoomsArea");
            Uri btnRmAreaImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\AR\RoomsArea.png");
            BitmapImage btnRmAreaImage = new BitmapImage(btnRmAreaImagePath);
            btnRmArea.LargeImage = btnRmAreaImage;


            // RoomsFinishingCommand
            PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing", "Отделка", path, "MS.RoomsFinishing");
            Uri btnRmFinishingImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\AR\RoomsFinishing.png");
            BitmapImage btnRmFinishingImage = new BitmapImage(btnRmFinishingImagePath);
            btnRmFinishing.LargeImage = btnRmFinishingImage;


            // SS Numerator command
            PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            Uri btnNumeratorImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\SS\СС.png");
            BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            btnNumerator.LargeImage = btnNumeratorImage;


            // General panel
            panelGeneral.AddItem(btnInfo);
            panelGeneral.AddItem(btnSelection);

            // AR panel
            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnRmFinishing);

            // SS panel
            panelSS.AddItem(btnNumerator);


        }
    }
}
