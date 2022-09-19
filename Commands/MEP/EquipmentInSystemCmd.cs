using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using MS.Shared;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class EquipmentInSystemCmd : IExternalCommand
    {
        /// <summary>
        /// Значение параметра PGS_Идентификация для вентилятора
        /// </summary>
        private readonly string _identFan = "1";

        /// <summary>
        /// Значение параметра PGS_Идентификация для воздухонагревателя
        /// </summary>
        private readonly string _identAirHeater = "2";

        /// <summary>
        /// Значение параметра PGS_Идентификация для воздухоохладителя
        /// </summary>
        private readonly string _identAirCooler = "3";

        /// <summary>
        /// Значение параметра PGS_Идентификация для фильтра
        /// </summary>
        private readonly string _identFilter = "4";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var systems = new FilteredElementCollector(doc)
                .OfClass(typeof(MechanicalSystem))
                .WhereElementIsNotElementType()
                .Cast<MechanicalSystem>()
                .ToList();

            foreach (MechanicalSystem system in systems)
            {
                List<FamilyInstance> equipment = system.DuctNetwork
                    .ToList()
                    .Where(e => e is FamilyInstance)
                    .Where(e => e.get_Parameter(SharedParams.PGS_Identification) != null)
                    .Cast<FamilyInstance>()
                    .ToList();

                var fan = equipment.Where(e => e.get_Parameter(SharedParams.PGS_Identification).AsValueString() == _identFan).ToList();

            }

            return Result.Succeeded;
        }
    }
}
