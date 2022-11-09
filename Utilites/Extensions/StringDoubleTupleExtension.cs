using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class StringDoubleTupleExtension
    {
        /// <summary>
        /// Расширение для добавления типа отделки в список кортежей или прибавления площади к уже имеющемуся
        /// </summary>
        /// <param name="existTupleList">Список кортежей типов отделки и их площадей</param>
        /// <param name="value">Кортеж, который нужно добавить в список</param>
        /// <returns>Обновленный список кортежей</returns>
        public static List<(string Fintype, double Area)> AddOrUpdate(
            this List<(string Fintype, double Area)> existTupleList,
            (string Fintype, double Area) value)
        {
            var exist = existTupleList.FirstOrDefault(wta => wta.Fintype == value.Fintype);
            if (exist.Fintype is null)
            {
                existTupleList.Add(value);
            }
            else
            {
                var index = existTupleList.IndexOf(exist);
                existTupleList[index] = (exist.Fintype, exist.Area + value.Area);
            }
            return existTupleList;
        }
    }
}
