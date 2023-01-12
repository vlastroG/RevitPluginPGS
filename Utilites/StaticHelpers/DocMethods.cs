using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.WorkWith
{
    /// <summary>
    /// Вспомогательный класс для 
    /// </summary>
    public static class DocMethods
    {
        /// <summary>
        /// Возвращает {3D} вид по умолчанию с отключенной областью подрезки
        /// </summary>
        /// <param name="doc">Документ, в котором расположен 3D вид</param>
        /// <returns>3D умолчанию, или null</returns>
        public static View3D GetView3Default(in Document doc)
        {
            View3D view = null;
            using (Transaction get3D = new Transaction(doc))
            {
                get3D.Start("3D вид по умолчанию");
                view = new FilteredElementCollector(doc)
                    .OfClass(typeof(View3D))
                    .FirstOrDefault(
                      e => e.Name.Equals("{3D}")) as View3D;
                if (!(view is null))
                {
                    view.IsSectionBoxActive = false;
                }
                get3D.Commit();
            }
            return view;
        }
    }
}
