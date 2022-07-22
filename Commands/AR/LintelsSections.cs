using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Utilites;
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
        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки
        /// </summary>
        private static readonly Guid _parLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        /// <summary>
        /// Guid параметра Орг.ТипВключатьВСпецификацию
        /// </summary>
        private static readonly Guid _parIncludeInSchedule = Guid.Parse("45ef1720-9cfe-49a7-b4d7-c67e4f7bd191");

        /// <summary>
        /// Значение встроенного параметра типа "Описание" для семейств перемычек
        /// </summary>  
        private string _lintelDescription = "Перемычка";

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
                            .AsValueString() == _lintelDescription)
                .Where(e => e.get_Parameter(_parIncludeInSchedule) != null &&
                            e.get_Parameter(_parIncludeInSchedule).AsInteger() == 1)
                .Where(e => e.get_Parameter(_parLintelMark) != null &&
                            !String.IsNullOrEmpty(e.get_Parameter(_parLintelMark).AsValueString()))
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

                string lintelMark;
                foreach (FamilyInstance lintel in lintels)
                {
                    lintelMark = lintel.get_Parameter(_parLintelMark).AsValueString();
                    bool isSectionCreated = createdSections.Where(s => s.Name == lintelMark).Count() > 0;
                    if (isSectionCreated)
                    {
                        continue;
                    }
                    else
                    {
                        //Element.Location - точка размещения(X Y координаты)
                        //FamilyInstance.HandOrientation - вектор вдоль длины перемычки
                        //FamilyInstance.GetSubComponentIds().GetFirst().Location.Z - отметка центра разреза по высоте.
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
                            TaskDialog.Show("Ошибка", "Не обнаружены вложенные семейства в семействе перемычки." +
                                $"\nId = {lintel.Id}");
                            continue;
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

                        //Min = new XYZ(-3, -3, 0),
                        //    Max = new XYZ(3, 3, 3),
                        BoundingBoxXYZ boxXYZ = new BoundingBoxXYZ()
                        {
                            Min = new XYZ(-_offsetLeftRight, -_offsetBottom, 0),
                            Max = new XYZ(_offsetLeftRight, _offsetTop, _offsetFarLimit),
                            Transform = sectionTransform,
                            Enabled = true
                        };
                        ViewSection section = ViewSection.CreateSection(doc, sectionTypeId, boxXYZ);
                        section.Name = lintelMark;
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

            TaskDialog.Show("Разрезы по перемычкам",
                $"{sectionByLintels} разрезов создано для {lintels.Count()} перемычек." +
                $"\n\nЕсли это количество не соответствует Вашим ожиданиям," +
                $"\nто, проверьте, чтобы:" +
                $"\n1. У семейств перемычек в типе в \"Описании\" было \'Перемычка\';" +
                $"\n2. В одном из экземпляров для каждого типа перемычек была галочка напротив Орг.ТипВключатьВСпецификацию;" +
                $"\n3. В этих же экземплярах перемычек был заполнен параметр PGS_МаркаПеремычки." +
                $"\n\nЕсли разрез по перемычке уже существует, новый создаваться не будет.");

            return Result.Succeeded;
        }
    }
}
