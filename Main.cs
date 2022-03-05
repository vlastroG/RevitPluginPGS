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


            PushButtonData btnInfo = new PushButtonData("Info", "Info", path, "MS.Info");
            Uri btnInfoImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\General\Info.png");
            BitmapImage btnInfoImage = new BitmapImage(btnInfoImagePath);
            btnInfo.LargeImage = btnInfoImage;
            btnInfo.ToolTip = "Выбор элементов по заданной категории. Категория берется из первого выбранного элемента, затем рамкой выбираются все элементы заданной категории";

            PushButtonData btnSelection = new PushButtonData("Selection", "Выбор", path, "MS.Selector");
            Uri btnSelectionImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\General\Selection.png");
            BitmapImage btnSelectionImage = new BitmapImage(btnSelectionImagePath);
            btnSelection.LargeImage = btnSelectionImage;


            PushButtonData btnRmArea = new PushButtonData("RoomsArea", "Квартирография", path, "MS.RoomsArea");
            Uri btnRmAreaImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\AR\RoomsArea.png");
            BitmapImage btnRmAreaImage = new BitmapImage(btnRmAreaImagePath);
            btnRmArea.LargeImage = btnRmAreaImage;

            PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing", "Отделка", path, "MS.RoomsFinishing");
            Uri btnRmFinishingImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\AR\RoomsFinishing.png");
            BitmapImage btnRmFinishingImage = new BitmapImage(btnRmFinishingImagePath);
            btnRmFinishing.LargeImage = btnRmFinishingImage;


            PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");
            Uri btnNumeratorImagePath = new Uri(@"D:\Строганов В.Г\REVIT\! C#_Plug-ins\MS\Images\Icons\SS\СС.png");
            BitmapImage btnNumeratorImage = new BitmapImage(btnNumeratorImagePath);
            btnNumerator.LargeImage = btnNumeratorImage;


            panelGeneral.AddItem(btnInfo);
            panelGeneral.AddItem(btnSelection);

            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnRmFinishing);

            panelSS.AddItem(btnNumerator);

            return Result.Succeeded;
        }
    }
}
