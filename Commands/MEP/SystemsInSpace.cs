using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SystemsInSpace : IExternalCommand
    {
        /// <summary>
        /// Значение, которое содержится в параметре Комментарии у пространств, которые не обрабатываются
        /// </summary>
        private readonly string _comment = "не обрабатывать";

        /// <summary>
        /// Значение, которое содержится в параметре Тип системы у вытяжной системы
        /// </summary>
        private readonly string _systemExhaust = "вытяжка";

        /// <summary>
        /// Значение, которое содержится в параметре Тип системы у приточной системы
        /// </summary>
        private readonly string _systemSypply = "приток";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            Guid[] _sharedParamsForCommand = new Guid[] {
            SharedParams.ADSK_SupplySystemName,
            SharedParams.ADSK_ExhaustSystemName
            };
            if (!SharedParams.IsCategoryOfDocContainsSharedParams(
                doc,
                BuiltInCategory.OST_MEPSpaces,
                _sharedParamsForCommand))
            {
                MessageBox.Show("В текущем проекте у категории \"Пространства\" " +
                    "отсутствуют необходимые общие параметры:" +
                    "\nADSK_Наименование вытяжной системы" +
                    "\nADSK_Наименование приточной системы",
                    "Ошибка");
                return Result.Cancelled;
            }

            System.Windows.Forms.DialogResult userWarning = System.Windows.Forms.MessageBox.Show(
                "У всех пространств, в Комментирии которых НЕ содержится \'не обрабатывать\' " +
                "обновятся значения параметров \'ADSK_Наименование вытяжной системы\' " +
                "и \'ADSK_Наименование приточной системы\' в соответствии с названиями приточных и вытяжных" +
                " систем воздуховодов в этих пространствах.",
                "Предупреждение",
                System.Windows.Forms.MessageBoxButtons.OKCancel);
            if (userWarning != System.Windows.Forms.DialogResult.OK)
            {
                return Result.Cancelled;
            }

            var spaces = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .WhereElementIsNotElementType()
                .Where(e => ReferenceEquals(e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                                .AsValueString(), null) ||
                            !e.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                                .AsValueString().ToLower()
                                .Contains(_comment))
                .Cast<Space>()
                .ToArray();

            var ducts = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .WhereElementIsNotElementType()
                .Where(e => e.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
                                .AsValueString().ToLower()
                                .Contains(_systemExhaust)
                         || e.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
                                .AsValueString().ToLower()
                                .Contains(_systemSypply))
                .ToArray();
            var exhaustCount = 0;
            var supplyCount = 0;

            List<ElementId> errorIds = new List<ElementId>();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Системы в пространствах назначение");

                foreach (var space in spaces)
                {
                    SpatialElementGeometryCalculator calculator = new SpatialElementGeometryCalculator(doc);
                    Solid spaceSolid;
                    try
                    {
                        SpatialElementGeometryResults results = calculator.CalculateSpatialElementGeometry(space);
                        spaceSolid = results.GetGeometry();
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException)
                    {
                        errorIds.Add(space.Id);
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        errorIds.Add(space.Id);
                        continue;
                    }

                    var ductsExhaustInSpace = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_DuctCurves)
                        .WhereElementIsNotElementType()
                        .WherePasses(new ElementIntersectsSolidFilter(spaceSolid))
                        .Where(e => e.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
                                        .AsValueString().ToLower()
                                        .Contains(_systemExhaust))
                        .GroupBy(duct => duct.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString())
                        .Select(grp => grp.First().get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString())
                        .ToArray();

                    var ductsSupplyInSpace = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_DuctCurves)
                        .WhereElementIsNotElementType()
                        .WherePasses(new ElementIntersectsSolidFilter(spaceSolid))
                        .Where(e => e.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM)
                                        .AsValueString().ToLower()
                                        .Contains(_systemSypply))
                        .GroupBy(duct => duct.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString())
                        .Select(grp => grp.First().get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsValueString())
                        .ToArray();

                    string exhaustSystemsInSpace = String.Join(", ", ductsExhaustInSpace);
                    string supplySystemsInSpace = String.Join(", ", ductsSupplyInSpace);

                    if (space.get_Parameter(SharedParams.ADSK_ExhaustSystemName).AsValueString() != exhaustSystemsInSpace)
                    {
                        space.get_Parameter(SharedParams.ADSK_ExhaustSystemName).Set(exhaustSystemsInSpace);
                        exhaustCount++;
                    }
                    if (space.get_Parameter(SharedParams.ADSK_SupplySystemName).AsValueString() != supplySystemsInSpace)
                    {
                        space.get_Parameter(SharedParams.ADSK_SupplySystemName).Set(supplySystemsInSpace);
                        supplyCount++;
                    }
                }
                trans.Commit();
                if (errorIds.Count > 0)
                {
                    string ids = String.Join(", ", errorIds.Select(e => e.ToString()));
                    MessageBox.Show($"Ошибка, пространства не обработаны, нельзя определить их объемы. Id: {ids}." +
                        $"\n\nЗначения наименований вытяжных систем в пространствах обновлены {exhaustCount} раз;" +
                        $"\nЗначения наименований приточных систем в пространствах обновлены {supplyCount} раз",
                        "Системы в пространствах, выполнено с ошибками!");
                }
                else
                {
                    MessageBox.Show($"Значения наименований вытяжных систем в пространствах обновлены {exhaustCount} раз;" +
                        $"\nЗачения наименований приточных систем в пространствах обновлены {supplyCount} раз",
                        "Систмы в пространствах, выполнено без ошибок");
                }
            }
            return Result.Succeeded;
        }
    }
}
