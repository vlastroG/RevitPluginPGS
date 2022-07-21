using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace MS.Utilites
{
    public static class ExtensionDictionary
    {
        /// <summary>
        /// Метод расширения для словаря Id элемента Revit и double числа.
        /// Либо создается новая пара ключ-значение либо в значение прибавляется заданное число.
        /// </summary>
        /// <param name="map">Подаваемый словарь.</param>
        /// <param name="key">Подаваемый ключ.</param>
        /// <param name="value">подаваемое число.</param>
        public static void MapIncrease(
    this IDictionary<ElementId, double> map, ElementId key, double value)
        {
            if (map.ContainsKey(key))
            {
                map[key] += value;
            }
            else
            {
                map[key] = value;
            }
        }


        /// <summary>
        /// Метод расширения для обновления пары ключ-значение в словаре.
        /// Если ключ уже существует, то значение перезаписывается,
        /// если ключа нет, то в словарь добавляется новая пара.
        /// </summary>
        /// <typeparam name="TKey">Ключ</typeparam>
        /// <typeparam name="TValue">Значение</typeparam>
        /// <param name="dict">Обновляемый словарь</param>
        /// <param name="key">Ключ</param>
        /// <param name="value">Новое значение</param>
        public static void AddOrUpdate<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}
