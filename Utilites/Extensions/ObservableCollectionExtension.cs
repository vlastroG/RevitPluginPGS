using MS.Commands.MEP.Mechanic;
using MS.Commands.MEP.Mechanic.Impl;
using MS.Commands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    /// <summary>
    /// Методы расширения для работы с ObservableCollection
    /// </summary>
    public static class ObservableCollectionExtension
    {
        /// <summary>
        /// Обновляет элемент в коллекции, если он в ней присутствует
        /// </summary>
        /// <typeparam name="T">Тип объектов коллекции</typeparam>
        /// <param name="collection">Коллекция для обработки</param>
        /// <param name="entity">Элемент с таким же Guid, но новыми параметрами</param>
        /// <returns></returns>
        public static bool UpdateEntity<T>(this ObservableCollection<T> collection, T entity) where T : Mechanic, IIdentifiable
        {
            var entityInCollection = collection.FirstOrDefault(e => e.Guid.Equals(entity.Guid));
            if (entityInCollection != null)
            {
                var index = collection.IndexOf(entityInCollection);
                collection[index] = entity;
                return true;
                entityInCollection = entity;
                switch (entityInCollection.EquipmentType)
                {
                    case Commands.MEP.Enums.EquipmentType.Fan:
                        var fanDestination = entityInCollection as Fan;
                        var fanSource = entity as Fan;
                        fanDestination.FanSpeed = fanSource.FanSpeed;
                        fanDestination.Mark = fanSource.Mark;
                        fanDestination.ExplosionProofType = fanSource.ExplosionProofType;
                        fanDestination.RatedPower = fanSource.RatedPower;
                        fanDestination.AirFlow = fanSource.AirFlow;
                        fanDestination.AirPressureLoss = fanSource.AirPressureLoss;
                        fanDestination.Count = fanSource.Count;
                        fanDestination.EngineSpeed = fanSource.EngineSpeed;
                        fanDestination.Type = fanSource.Type;
                        fanDestination.Length = fanSource.Length;
                        return true;
                    case Commands.MEP.Enums.EquipmentType.AirCooler:
                        var coolerDestination = entityInCollection as Cooler;
                        var coolerSource = entity as Cooler;
                        coolerDestination.Type = coolerSource.Type;
                        coolerDestination.AirPressureLoss = coolerSource.AirPressureLoss;
                        coolerDestination.PowerCool = coolerSource.PowerCool;
                        coolerDestination.Power = coolerSource.Power;
                        coolerDestination.Count = coolerSource.Count;
                        coolerDestination.Length = coolerSource.Length;
                        coolerDestination.TemperatureIn = coolerSource.TemperatureIn;
                        coolerDestination.TemperatureOut = coolerSource.TemperatureOut;
                        return true;
                    case Commands.MEP.Enums.EquipmentType.AirHeater:
                        var heaterDestination = entityInCollection as Heater;
                        var heaterSource = entity as Heater;
                        heaterDestination.Type = heaterSource.Type;
                        heaterDestination.Length = heaterSource.Length;
                        heaterDestination.TemperatureIn = heaterSource.TemperatureIn;
                        heaterDestination.TemperatureOut = heaterSource.TemperatureOut;
                        heaterDestination.AirPressureLoss = heaterSource.AirPressureLoss;
                        heaterDestination.Power = heaterSource.Power;
                        heaterDestination.PowerHeat = heaterSource.PowerHeat;
                        heaterDestination.Count = heaterSource.Count;
                        return true;
                    case Commands.MEP.Enums.EquipmentType.Filter:
                        var filterDestination = entityInCollection as Filter;
                        var filterSource = entity as Filter;
                        filterDestination.Note = filterSource.Note;
                        filterDestination.Windage = filterSource.Windage;
                        filterDestination.Count = filterSource.Count;
                        filterDestination.Length = filterSource.Length;
                        filterDestination.Type = filterSource.Type;
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        /// <summary>
        /// Удаляет элемент с заданным Guid из коллекции, если он присутствует в ней
        /// </summary>
        /// <typeparam name="T">Тип объектов, хранимых в коллкции</typeparam>
        /// <param name="collection">Коллекция объектов</param>
        /// <param name="guid">Guid объекта для удаления</param>
        /// <returns>True, если объект успешно удален</returns>
        public static bool DeleteEntity<T>(this ObservableCollection<T> collection, Guid guid) where T : IIdentifiable
        {
            var entityInCollection = collection.FirstOrDefault(e => e.Guid.Equals(guid));
            if (entityInCollection != null)
            {
                var index = collection.IndexOf(entityInCollection);
                collection.RemoveAt(index);
                return true;
            }
            return false;
        }
    }
}
