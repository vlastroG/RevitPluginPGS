using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MS.Commands.General
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LevelName : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            var levels_filter = new FilteredElementCollector(doc);
            var levels = levels_filter
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach (var level in levels)
            {
                var level_name = level.get_Parameter(BuiltInParameter.DATUM_TEXT).AsValueString();
                Regex regex = new Regex(@"\d+.\d+)$");

                StringBuilder sb = new StringBuilder();

            }

            return Result.Succeeded;
        }
    }
}
