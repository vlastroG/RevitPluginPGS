using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using MS.Utilites;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MaterialColors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector materialCollector = new FilteredElementCollector(doc);
            List<Element> materials = materialCollector
                .OfCategory(BuiltInCategory.OST_Materials)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            //foreach (Material elem in materials)
            //{
            Material elem = materials.FirstOrDefault() as Material;
            string materialName = elem.Name;

            int width = 360;
            int height = 86;
            float dpi = 144;

            Color color = elem.Color;
            int red = color.Red;
            int green = color.Green;
            int blue = color.Blue;

            string @path = "%AppData%/pgsRevitPlugin/MaterialColors/" + @materialName + ".png";

            WorkWithGeometry.CreateColoredRectanglePng(width, height, dpi, red, green, blue, path);
            //}


            return Result.Succeeded;
        }
    }
}
