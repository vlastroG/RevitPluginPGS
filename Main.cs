﻿using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

            application.CreateRibbonTab("PGS-BIM");

            var path = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData("RoomsArea", "Квартирография", path, "MS.RoomsArea");

            RibbonPanel panel = application.CreateRibbonPanel("PGS-BIM", "Раздел АР");

            panel.AddItem(button);

            return Result.Succeeded;
        }
    }
}
