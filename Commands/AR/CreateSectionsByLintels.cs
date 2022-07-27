using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MS.Commands.AR
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateSectionsByLintels : IExternalCommand
    {
        /// <summary>
        /// Значение названия типа разрезов для автоматического создания
        /// </summary>
        private string _sectionTypeName = "Разрез_Без номера листа";

        /// <summary>
        /// Смещение области обрезки разреза вниз от центра перемычки
        /// </summary>
        private double _offsetBottom = 0.165;

        /// <summary>
        /// Смещение области обрезки разреза вверх от центра перемычки
        /// </summary>
        private double _offsetTop = 1.65;

        /// <summary>
        /// Смещение области обрезки разреза вправо и влево от центра перемычки
        /// </summary>
        private double _offsetLeftRight = 1.2;


        /// <summary>
        /// Смещение дальнего предела секущего диапазона разреза по перемычке
        /// </summary>
        private double _offsetFarLimit = 1.5;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            int sectionByLintels = 0;

            string sectionTemplateTittle = UserInput.GetStringFromUser(
                "Шаблон вида для разрезов",
                "Напишите название шаблона вида для назначения создаваемым разрезам.",
                "АР_Перемычки");
            if (sectionTemplateTittle.Length == 0)
            {
                return Result.Cancelled;
            }

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
                            .AsValueString() == SharedValues.LintelDescription)
                .Where(e => e.get_Parameter(SharedParams.Org_TypeIncludeInSchedule) != null &&
                            e.get_Parameter(SharedParams.Org_TypeIncludeInSchedule).AsInteger() == 1)
                .Where(e => e.get_Parameter(SharedParams.PGS_MarkLintel) != null &&
                            !String.IsNullOrEmpty(e.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString()))
                .Cast<FamilyInstance>();

            FilteredElementCollector sectionsFilter = new FilteredElementCollector(doc);
            var createdSections = sectionsFilter
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Where(v => (v as View).ViewType == ViewType.Section)
                .ToArray();

            FilteredElementCollector sectionTemplateFilter = new FilteredElementCollector(doc);
            var sectionTemplate = sectionTemplateFilter
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Cast<View>()
                .Where(v => v.ViewType == ViewType.Section)
                .Where(v => v.Title == sectionTemplateTittle)
                .FirstOrDefault();


            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Разрезы по перемычкам");

                foreach (FamilyInstance lintel in lintels)
                {
                    string lintelMark;
                    lintelMark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString();
                    bool isSectionCreated = createdSections.Where(s => s.Name == lintelMark).Count() > 0;
                    if (isSectionCreated)
                    {
                        continue;
                    }
                    else
                    {
                        XYZ center;
                        XYZ direction;
                        double x = (lintel.Location as LocationPoint).Point.X;
                        double y = (lintel.Location as LocationPoint).Point.Y;
                        double z;
                        try
                        {
                            z = (doc.GetElement(lintel.GetSubComponentIds().FirstOrDefault())
                                                .Location as LocationPoint).Point.Z;
                        }
                        catch (ArgumentNullException)
                        {
                            z = (lintel.Location as LocationPoint).Point.Z;
                            //TaskDialog.Show("Ошибка", "Не обнаружены вложенные семейства в семействе перемычки. Нельзя определить отметку низа перемычки." +
                            //    $"\nId = {lintel.Id}");
                            //continue;
                        }
                        center = new XYZ(x, y, z);
                        direction = lintel.HandOrientation.Normalize();
                        XYZ up = XYZ.BasisZ;
                        XYZ viewDirection = up.CrossProduct(direction);

                        Transform sectionTransform = Transform.Identity;
                        sectionTransform.Origin = center;
                        sectionTransform.BasisX = viewDirection;
                        sectionTransform.BasisY = up;
                        sectionTransform.BasisZ = direction;

                        BoundingBoxXYZ boxXYZ = new BoundingBoxXYZ()
                        {
                            Min = new XYZ(-_offsetLeftRight, -_offsetBottom, 0),
                            Max = new XYZ(_offsetLeftRight, _offsetTop, _offsetFarLimit),
                            Transform = sectionTransform,
                            Enabled = true
                        };
                        ViewSection section = ViewSection.CreateSection(doc, sectionTypeId, boxXYZ);
                        string sectionName = lintelMark + '_' + doc.GetElement(lintel.LevelId).Name;
                        try
                        {
                            section.Name = sectionName;
                        }
                        catch (Exception)
                        {
                            section.Name = sectionName + Guid.NewGuid().ToString();
                        }
                        if (sectionTemplate != null)
                        {
                            section.ViewTemplateId = sectionTemplate.Id;
                        }
                        section.get_Parameter(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC).Set(1);
                        sectionByLintels++;
                    }
                }

                trans.Commit();
            }

            MessageBox.Show(
                $"{sectionByLintels} разрезов создано для {lintels.Count()} перемычек." +
                $"\n\nЕсли это количество не соответствует ожиданиям," +
                $"\nто, проверьте, чтобы:" +
                $"\n1. У семейств перемычек в типе в \"Описании\" было \'Перемычка\';" +
                $"\n2. В одном из экземпляров для каждого типа перемычек была галочка напротив Орг.ТипВключатьВСпецификацию;" +
                $"\n3. В этих же экземплярах перемычек был заполнен параметр PGS_МаркаПеремычки." +
                $"\n\nЕсли разрез по перемычке уже существует, новый создаваться не будет.",
                "Разрезы по перемычкам");

            return Result.Succeeded;
        }
    }
}
