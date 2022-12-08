using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.BIM
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DuctEquipmentConstructorCmd : IExternalCommand
    {
        /// <summary>
        /// Название размещаемого семейства
        /// </summary>
        private readonly string _familyName = "Элементы установки";

        /// <summary>
        /// Название размещаемого типоразмера семейства
        /// </summary>
        private readonly string _typeName = "Фильтр";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var famInstSymb = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .FirstOrDefault(ft => ft.FamilyName == _familyName && ft.Name == _typeName);

            var level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .FirstOrDefault();

            using (Transaction placeFams = new Transaction(doc))
            {
                placeFams.Start("Placed famInst families");

                double xFamInst = 0;
                double yFamInst = 0;
                double zFamInst = 0;
                XYZ point = new XYZ(xFamInst, yFamInst, zFamInst);
                Element famInstEl = Autodesk.Revit.Creation.ItemFactoryBase.NewFamilyInstance(point, famInstSymb, level, StructuralType.NonStructural);

                try
                {

                }
                catch (NullReferenceException)
                {
                    throw new ArgumentException(
                        "Семейство некорректно! Нельзя назначить значения параметров!");
                }
                catch (IndexOutOfRangeException)
                {

                }
                placeFams.Commit();
            }

            return Result.Succeeded;
        }
    }
}
