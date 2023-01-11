using MS.RevitCommands.MEP.Mechanic;
using MS.RevitCommands.MEP.Mechanic.Impl;
using MS.RevitCommands.Models.Interfaces;
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
        public static bool UpdateEntity<T>(this ObservableCollection<T> collection, T entity) where T : IIdentifiable
        {
            var count = collection.Count;
            for (int i = 0; i < count; i++)
            {
                if (collection[i].Guid.Equals(entity.Guid))
                {
                    collection[i] = entity;
                    return true;
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
            var count = collection.Count;
            for (int i = 0; i < count; i++)
            {
                var entityInCollection = collection.FirstOrDefault(e => e.Guid.Equals(guid));
                if (collection[i].Guid.Equals(guid))
                {
                    collection.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}
