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

            PushButtonData btnRmArea = new PushButtonData("RoomsArea", "Квартирография", path, "MS.RoomsArea");
            PushButtonData btnRmFinishing = new PushButtonData("RoomsFinishing", "Отделка", path, "MS.RoomsFinishing");

            PushButtonData btnNumerator = new PushButtonData("Numerator", "Маркировка", path, "MS.Numerator");


            panelGeneral.AddItem(btnInfo);

            panelAR.AddItem(btnRmArea);
            panelAR.AddItem(btnRmFinishing);

            panelSS.AddItem(btnNumerator);

            return Result.Succeeded;
        }
    }
}
