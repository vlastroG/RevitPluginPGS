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
using System.Windows;

namespace MS.RevitCommands.MEP
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    internal class EquipmentInSystemCmd : IExternalCommand
    {
        /// <summary>
        /// Значение параметра PGS_Идентификация для вентилятора = 1
        /// </summary>
        private readonly string _identFan = "1";

        /// <summary>
        /// Значение параметра PGS_Идентификация для воздухонагревателя = 2
        /// </summary>
        private readonly string _identAirHeater = "2";

        /// <summary>
        /// Значение параметра PGS_Идентификация для воздухоохладителя = 3
        /// </summary>
        private readonly string _identAirCooler = "3";

        /// <summary>
        /// Значение параметра PGS_Идентификация для фильтра = 4
        /// </summary>
        private readonly string _identFilter = "4";

        /// <summary>
        /// Проверяет, присутствует ли общий параметр PGS_Идентификация в экземпляре или типе элемента
        /// и имеет ли он значение.
        /// </summary>
        /// <param name="elem">Экземпляр размещаемого семейства</param>
        /// <returns>True, если параметр присутствует и имеет значение, иначе False</returns>
        private bool IsContainsIdentification(FamilyInstance elem)
        {
            bool identInInst = elem.get_Parameter(SharedParams.PGS_Identification) != null
                && !String.IsNullOrEmpty(elem.get_Parameter(SharedParams.PGS_Identification).AsValueString());
            bool identInSymbol = elem.Symbol.get_Parameter(SharedParams.PGS_Identification) != null
                && !String.IsNullOrEmpty(elem.Symbol.get_Parameter(SharedParams.PGS_Identification).AsValueString());
            return identInInst || identInSymbol;
        }

        /// <summary>
        /// Возвращает значение параметра PGS_Идентификация из экземпляра или типа элемента
        /// </summary>
        /// <param name="elem">Экземпляр семейства, в котором происходит поиск значения параметра</param>
        /// <returns>Значение параметра</returns>
        /// <exception cref="ArgumentNullException">
        /// Исключение, если параметр PGS_Идентификация отсутствует и в типе и в экземпляре элемента
        /// </exception>
        private string GetIdentificationValue(FamilyInstance elem)
        {
            string identification = String.Empty;
            if (elem.get_Parameter(SharedParams.PGS_Identification) != null
                && !String.IsNullOrEmpty(elem.get_Parameter(SharedParams.PGS_Identification).AsValueString()))
                identification = elem.get_Parameter(SharedParams.PGS_Identification).AsValueString();
            else if (elem.Symbol.get_Parameter(SharedParams.PGS_Identification) != null
                && !String.IsNullOrEmpty(elem.Symbol.get_Parameter(SharedParams.PGS_Identification).AsValueString()))
                identification = elem.Symbol.get_Parameter(SharedParams.PGS_Identification).AsValueString();
            else
            {
                throw new ArgumentNullException($"Элемент {elem.Id} не содержит параметр PGS_Идентификация");
            }
            return identification;
        }

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

            List<ElementId> errorsFan = new List<ElementId>();
            List<ElementId> errorsHeater = new List<ElementId>();
            List<ElementId> errorsCooler = new List<ElementId>();
            List<ElementId> errorsFilter = new List<ElementId>();
            int fanIterations = 0;
            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("Оборудование в системах");

                foreach (MechanicalSystem system in systems)
                {

                    List<FamilyInstance> equipment = system.DuctNetwork
                        .ToList()
                        .Where(e => e is FamilyInstance)
                        .Cast<FamilyInstance>()
                        .Where(e => IsContainsIdentification(e))
                        .ToList();

                    var fans = equipment.Where(
                        e => GetIdentificationValue(e) == _identFan)
                        .ToList();

                    var heaters = equipment.Where(
                        e => GetIdentificationValue(e) == _identAirHeater)
                        .ToList();
                    var heater = heaters.FirstOrDefault();

                    var coolers = equipment.Where(
                        e => GetIdentificationValue(e) == _identAirCooler)
                        .ToList();
                    var cooler = coolers.FirstOrDefault();

                    var filters = equipment.Where(
                        e => GetIdentificationValue(e) == _identFilter)
                        .ToList();
                    var filter = filters.FirstOrDefault();

                    double heaterLossAir = 0;
                    double heaterAirTempIn = 0;
                    double heaterAirTempOut = 0;
                    double powerThermal = 0;
                    double heaterPower = 0;
                    string heaterType = String.Empty;
                    double heaterCount = 0;
                    if (heater != null)
                    {
                        try
                        {
                            heaterLossAir = heater.get_Parameter(SharedParams.ADSK_LossPreasureInAirHeater).AsDouble();
                            heaterAirTempIn = heater.get_Parameter(SharedParams.ADSK_AirTempInAirHeater).AsDouble();
                            heaterAirTempOut = heater.get_Parameter(SharedParams.ADSK_AirTempOutAirHeater).AsDouble();
                            powerThermal = heater.get_Parameter(SharedParams.ADSK_PowerThermal).AsDouble();
                            heaterPower = heater.get_Parameter(SharedParams.PGS_AirHeaterPower).AsDouble();
                            heaterType = heater.get_Parameter(SharedParams.PGS_AirHeaterType).AsValueString();
                            heaterCount = heaters.Count;
                        }
                        catch (NullReferenceException)
                        {
                            errorsHeater.Add(heater.Id);
                        }
                    }

                    double coolerLossAir = 0;
                    double coolerAirTempIn = 0;
                    double coolerAirTempOut = 0;
                    double powerCooling = 0;
                    double coolerPower = 0;
                    string coolerType = String.Empty;
                    double coolerCount = 0;
                    if (cooler != null)
                    {
                        try
                        {
                            coolerLossAir = cooler.get_Parameter(SharedParams.ADSK_LossPreasureInAirCooler).AsDouble();
                            coolerAirTempIn = cooler.get_Parameter(SharedParams.ADSK_AirTempInAirCooler).AsDouble();
                            coolerAirTempOut = cooler.get_Parameter(SharedParams.ADSK_AirTempOutAirCooler).AsDouble();
                            powerCooling = cooler.get_Parameter(SharedParams.ADSK_PowerCooling).AsDouble();
                            coolerPower = cooler.get_Parameter(SharedParams.PGS_AirCoolerPower).AsDouble();
                            coolerType = cooler.get_Parameter(SharedParams.PGS_AirCoolerType).AsValueString();
                            coolerCount = coolers.Count;
                        }
                        catch (NullReferenceException)
                        {
                            errorsCooler.Add(cooler.Id);
                        }
                    }

                    double filterResistance = 0;
                    string filterType = String.Empty;
                    double filterCount = 0;
                    if (filter != null)
                    {
                        try
                        {
                            filterResistance = filter.get_Parameter(SharedParams.ADSK_AirFilterResistance).AsDouble();
                            filterType = filter.get_Parameter(SharedParams.PGS_FilterType).AsValueString();
                            filterCount = filters.Count;
                        }
                        catch (NullReferenceException)
                        {
                            errorsFilter.Add(filter.Id);
                        }
                    }

                    foreach (var fan in fans)
                    {
                        try
                        {
                            fan.get_Parameter(SharedParams.ADSK_LossPreasureInAirHeater).Set(heaterLossAir);
                            fan.get_Parameter(SharedParams.ADSK_AirTempInAirHeater).Set(heaterAirTempIn);
                            fan.get_Parameter(SharedParams.ADSK_AirTempOutAirHeater).Set(heaterAirTempOut);
                            fan.get_Parameter(SharedParams.ADSK_PowerThermal).Set(powerThermal);
                            fan.get_Parameter(SharedParams.PGS_AirHeaterPower).Set(heaterPower);
                            fan.get_Parameter(SharedParams.PGS_AirHeaterType).Set(heaterType);
                            fan.get_Parameter(SharedParams.PGS_AirHeaterCount).Set(heaterCount);

                            fan.get_Parameter(SharedParams.ADSK_LossPreasureInAirCooler).Set(coolerLossAir);
                            fan.get_Parameter(SharedParams.ADSK_AirTempInAirCooler).Set(coolerAirTempIn);
                            fan.get_Parameter(SharedParams.ADSK_AirTempOutAirCooler).Set(coolerAirTempOut);
                            fan.get_Parameter(SharedParams.ADSK_PowerCooling).Set(powerCooling);
                            fan.get_Parameter(SharedParams.PGS_AirCoolerPower).Set(coolerPower);
                            fan.get_Parameter(SharedParams.PGS_AirCoolerType).Set(coolerType);
                            fan.get_Parameter(SharedParams.PGS_AirCoolerCount).Set(coolerCount);

                            fan.get_Parameter(SharedParams.ADSK_AirFilterResistance).Set(filterResistance);
                            fan.get_Parameter(SharedParams.PGS_FilterType).Set(filterType);
                            fan.get_Parameter(SharedParams.PGS_FilterCount).Set(filterCount);
                            fanIterations++;
                        }
                        catch (NullReferenceException)
                        {
                            errorsFan.Add(fan.Id);
                        }
                    }
                }
                trans.Commit();
            }
            string errorsFanMessage;
            string errorsHeaterMessage;
            string errorsCoolerMessage;
            string errorsFilterMessage;
            SharedParams.CreateErrorMessage(out errorsFanMessage, errorsFan);
            SharedParams.CreateErrorMessage(out errorsHeaterMessage, errorsHeater);
            SharedParams.CreateErrorMessage(out errorsCoolerMessage, errorsCooler);
            SharedParams.CreateErrorMessage(out errorsFilterMessage, errorsFilter);

            MessageBox.Show(
                $"Обработано {fanIterations} вентиляторов в {systems.Count} системах." +
                $"\nId вентиляторов с отсутствующими общими параметрами: {errorsFanMessage}" +
                $"\nId воздухонагревателей с отсутствующими общими параметрами: {errorsHeaterMessage}" +
                $"\nId воздухоохладителей с отсутствующими общими параметрами: {errorsCoolerMessage}" +
                $"\nId фильтров с отсутствующими общими параметрами: {errorsFilterMessage}",
                "Оборудование в системах");

            return Result.Succeeded;
        }
    }
}
