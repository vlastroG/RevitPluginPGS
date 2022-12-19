using MS.Commands.MEP.Mechanic.Impl;
using MS.Commands.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Services
{
    public class MechanicRepository : RepositoryInMemory<Mechanic.Mechanic>
    {
        protected override bool Update(Mechanic.Mechanic source, Mechanic.Mechanic destination)
        {
            switch (source.EquipmentType)
            {
                case Enums.EquipmentType.Fan:
                    return UpdateFan(source as Fan, destination as Fan);
                case Enums.EquipmentType.AirCooler:
                    return UpdateCooler(source as Cooler, destination as Cooler);
                case Enums.EquipmentType.AirHeater:
                    return UpdateHeater(source as Heater, destination as Heater);
                case Enums.EquipmentType.Filter:
                    return UpdateFilter(source as Filter, destination as Filter);
            }
            return false;
        }

        private bool UpdateFan(Fan fanSource, Fan fanDestination)
        {
            fanSource.Mark = fanDestination.Mark;
            fanSource.AirPressureLoss = fanDestination.AirPressureLoss;
            fanSource.AirFlow = fanDestination.AirFlow;
            fanSource.Type = fanDestination.Type;
            fanSource.Count = fanDestination.Count;
            fanSource.EngineSpeed = fanDestination.EngineSpeed;
            fanSource.FanSpeed = fanDestination.FanSpeed;
            fanSource.RatedPower = fanDestination.RatedPower;
            fanSource.ExplosionProofType = fanDestination.ExplosionProofType;
            return true;
        }

        private bool UpdateCooler(Cooler coolerSource, Cooler coolerDestination)
        {
            coolerSource.Count = coolerDestination.Count;
            coolerSource.AirPressureLoss = coolerDestination.AirPressureLoss;
            coolerSource.Type = coolerDestination.Type;
            coolerSource.Power = coolerDestination.Power;
            coolerSource.PowerCool = coolerDestination.PowerCool;
            coolerSource.TemperatureIn = coolerDestination.TemperatureIn;
            coolerSource.TemperatureOut = coolerDestination.TemperatureOut;
            return true;
        }

        private bool UpdateHeater(Heater heaterSource, Heater heaterDestination)
        {
            heaterSource.Type = heaterDestination.Type;
            heaterSource.Power = heaterDestination.Power;
            heaterSource.PowerHeat = heaterDestination.PowerHeat;
            heaterSource.AirPressureLoss = heaterDestination.AirPressureLoss;
            heaterSource.TemperatureIn = heaterDestination.TemperatureIn;
            heaterSource.TemperatureOut = heaterDestination.TemperatureOut;
            heaterSource.Count = heaterDestination.Count;
            return true;
        }

        private bool UpdateFilter(Filter filterSource, Filter filterDestination)
        {
            filterSource.Type = filterDestination.Type;
            filterSource.Note = filterDestination.Note;
            filterSource.Count = filterDestination.Count;
            filterSource.Windage = filterDestination.Windage;
            return true;
        }
    }
}
