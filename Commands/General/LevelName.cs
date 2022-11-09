using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Utilites;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MS.Commands.General
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LevelName : IExternalCommand
    {
        private readonly double _footToMetersCoeff = 304.8;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            var levels_filter = new FilteredElementCollector(doc);
            var levels = levels_filter
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToElements();

            var doc_proj_section = WorkWithString.GetProjectSectionName(doc);

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Добавить отм. уровням");

                foreach (var level in levels)
                {
                    var level_name = level.get_Parameter(BuiltInParameter.DATUM_TEXT).AsValueString();
                    var level_height_revit = Math
                        .Round(level
                               .get_Parameter(BuiltInParameter.LEVEL_ELEV)
                               .AsDouble()
                               * _footToMetersCoeff
                               / 1000,
                               3,
                               MidpointRounding.AwayFromZero);

                    Regex regex = new Regex(@"^[А-Я]+[А-Я]+_+[\+\-]*\d+[\.\,]+\d+");

                    if (regex.IsMatch(level_name))
                    {
                        string height_pattern = @"[\+\-]*\d+[\.\,]+\d+";//паттерн для отметки уровня
                        string level_height_str = Regex.Match(level_name, height_pattern).Value;
                        double level_height_from_str = Double.MinValue;
                        try
                        {
                            level_height_from_str = Double.Parse(level_height_str, CultureInfo.InvariantCulture);
                            if (level_height_from_str != level_height_revit)
                            {
                                var level_height_revit_str = WorkWithString.GetStringFromDouble(level_height_revit);
                                var level_name_new = level_name
                                    .Replace(level_height_str, level_height_revit_str);

                                level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(level_name_new);
                            }
                        }
                        catch (FormatException)
                        {
                            throw new FormatException(level_name);
                        }
                    }
                    else
                    {
                        var level_height_revit_str = WorkWithString.GetStringFromDouble(level_height_revit);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(doc_proj_section);
                        sb.Append('_');
                        sb.Append(level_height_revit_str);
                        sb.Append('_');
                        sb.Append(level_name);
                        var level_name_new = sb.ToString();

                        level.get_Parameter(BuiltInParameter.DATUM_TEXT).Set(level_name_new);
                    }
                }
                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
