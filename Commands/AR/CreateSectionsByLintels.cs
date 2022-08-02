using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using MS.Utilites.Comparers;
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

            var addLevelResult = UserInput.YesNoCancelInput("Разрезы по перемычкам", "Если считать перемычки поэтажно - \"Да\", иначе - \"Нет\"");
            if (addLevelResult != System.Windows.Forms.DialogResult.Yes && addLevelResult != System.Windows.Forms.DialogResult.No)
            {
                return Result.Cancelled;
            }
            bool addLevel;
            if (addLevelResult == System.Windows.Forms.DialogResult.Yes)
            {
                addLevel = true;
            }
            else
            {
                addLevel = false;
            }

            IList<Element> sectionTypesAll = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Where(vft => (vft as ViewFamilyType).ViewFamily == ViewFamily.Section)
                .ToArray();
            Element sectionType = sectionTypesAll
                .Where(type => type.Name == _sectionTypeName)
                .FirstOrDefault()
                ?? sectionTypesAll.FirstOrDefault();
            ElementId sectionTypeId = sectionType.Id;

            var lintelsAll = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsNotElementType()
                .Where(e => (e is FamilyInstance) &&
                            WorkWithFamilies.GetSymbolDescription(e as FamilyInstance)
                            == SharedValues.LintelDescription)
                .Cast<FamilyInstance>()
                .ToArray();
            var lintels = lintelsAll.Distinct(new LintelsEqualityComparer(addLevel));

            var createdSections = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Views)
                .WhereElementIsNotElementType()
                .Where(v => (v as View).ViewType == ViewType.Section)
                .ToArray();

            var sectionTemplate = new FilteredElementCollector(doc)
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
                    string lintelUniqueName;
                    lintelUniqueName = WorkWithFamilies.GetLintelUniqueName(lintel, addLevel);
                    bool isSectionCreated = createdSections.Where(s => s.Name == lintelUniqueName).Count() > 0;
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

                        var isZbySubComp = lintel.GetSubComponentIds().FirstOrDefault() != null;

                        if (isZbySubComp)
                        {
                            z = (doc.GetElement(lintel.GetSubComponentIds().FirstOrDefault())
                                                .Location as LocationPoint).Point.Z;
                        }
                        else
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
                        string sectionName = lintelUniqueName;
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
                $"{sectionByLintels} разрезов создано для {lintelsAll.Count()} перемычек." +
                $"\n\nРазрезы создаются только для обобщенных моделей, " +
                $"в типоразмере которых в описании написано \"Перемычка\"." +
                $"\nРазрезы создаются для уникального набора значений параметров:" +
                $"\n1) ADSK_Толщина стены, находящегося в экземпляре перемычки " +
                $"или в экземпляре родительского семейства;" +
                $"\n2) ADSK_Наименование всех вложенных элементов перемычки, которые параллельны стене;" +
                $"\n3) Ширине перемычки, вычисленной как расстояние между центрами крайних элементов из п.2)," +
                $"спроецированное на поперечную ось перемычки." +
                $"\n4)* Если перемычки считаются поэтажно, то к этому перечню параметров добавляется Уровень.",
                "Разрезы по перемычкам");

            return Result.Succeeded;
        }
    }
}
