using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LintelsSections : IExternalCommand
    {
        private static readonly Guid _parLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        private static readonly Guid _parIncludeInSchedule = Guid.Parse("45ef1720-9cfe-49a7-b4d7-c67e4f7bd191");

        private string _lintelDescription = "Перемычка";

        private string _sectionTypeName = "Разрез_Без номера листа";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {


            //Element.Location - точка размещения(X Y координаты)
            //FamilyInstance.HandOrientation - вектор вдоль длины перемычки
            //FamilyInstance.GetSubComponentIds().GetFirst().Location.Z - отметка центра разреза по высоте.

            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Transform rotation = Transform.CreateRotation(XYZ.BasisY, 90 * Math.PI / 180);

            BoundingBoxXYZ boxXYZ = new BoundingBoxXYZ()
            {
                Min = new XYZ(-3, -3, 0),
                Max = new XYZ(3, 3, 3),
                Transform = rotation,
                Enabled = true
            };

            FilteredElementCollector sectionTypeFilter = new FilteredElementCollector(doc);
            IList<Element> sectionTypesAll = sectionTypeFilter
                .OfClass(typeof(ViewFamilyType))
                .Where(vft => (vft as ViewFamilyType).ViewFamily == ViewFamily.Section)
                .ToList();
            Element sectionType = sectionTypesAll
                .Where(type => type.Name == _sectionTypeName)
                .FirstOrDefault()
                ?? sectionTypesAll.FirstOrDefault();
            ElementId sectionTypeId = sectionType.Id;

            FilteredElementCollector lintelsFilter = new FilteredElementCollector(doc);
            var lintels = lintelsFilter
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .Where(e => (e is FamilyInstance) &&
                            (e as FamilyInstance).Symbol
                            .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                            .AsValueString() == _lintelDescription)
                .Where(e => e.get_Parameter(_parIncludeInSchedule) != null &&
                            e.get_Parameter(_parIncludeInSchedule).AsInteger() == 1)
                .Where(e => e.get_Parameter(_parLintelMark) != null &&
                            !String.IsNullOrEmpty(e.get_Parameter(_parLintelMark).AsValueString()));


            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Create section");

                foreach (Element lintel in lintels)
                {

                }
                ViewSection section = ViewSection.CreateSection(doc, sectionTypeId, boxXYZ);

                trans.Commit();
            }

            return Result.Succeeded;
        }
    }
}
