using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Метод расширения для словаря Id элемента Revit и double числа.
        /// Либо создается новая пара ключ-значение либо в значение прибавляется заданное число.
        /// </summary>
        /// <param name="map">Подаваемый словарь</param>
        /// <param name="key">Подаваемый ключ</param>
        /// <param name="value">подаваемое число</param>
        public static void MapIncrease(
    this IDictionary<ElementId, double> map, ElementId key, double value)
        {
            map[key] += value;
        }
    }
}
