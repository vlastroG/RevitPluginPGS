using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites
{
    public static class WorkWithParameters
    {
        /// <summary>
        /// Возвращает Id категории элемента в формате int.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id.IntegerValue ?? 0;
        }
    }
}
