using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.SelectionFilters
{
    /// <summary>
    /// Фильтр выбора элементов, образующих проемы
    /// </summary>
    public class SelectionFilterOpenings : ISelectionFilter
    {
        /// <summary>
        /// Разрешен только выбор экземплярв семейств категорий Окна и Двери и витражных стен
        /// </summary>
        /// <param name="elem"></param>
        /// <returns>Если элемент - экземпляр семейства категории Окно или Дверь, или витражная стена => True, иначе => False</returns>
        public bool AllowElement(Element elem)
        {
            if (elem is FamilyInstance inst)
            {
                var category = (BuiltInCategory)ParametersMethods.GetCategoryIdAsInteger(inst);
                if ((category == BuiltInCategory.OST_Doors)
                    || (category == BuiltInCategory.OST_Windows))
                {
                    if ((inst.Host is Wall wall) && (wall.CurtainGrid is null))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (elem is Wall wall)
            {
                if (!(wall.CurtainGrid is null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
