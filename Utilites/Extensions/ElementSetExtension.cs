using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ElementSetExtension
    {
        /// <summary>
        /// Конвертирует ElementSet в список Element
        /// </summary>
        /// <param name="elemSet"></param>
        /// <returns></returns>
        public static List<Element> ToList(this ElementSet elemSet)
        {
            List<Element> list = new List<Element>();
            foreach (Element item in elemSet)
            {
                list.Add(item);
            }
            return list;
        }
    }
}
