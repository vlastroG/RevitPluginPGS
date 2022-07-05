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
    }
}
