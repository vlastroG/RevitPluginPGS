using Autodesk.Revit.DB;
using MS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Comparers
{
    public class LintelsEqualityComparer : IEqualityComparer<FamilyInstance>
    {
        public bool Equals(FamilyInstance lintel1, FamilyInstance lintel2)
        {
            if (lintel1.GetHashCode() == lintel2.GetHashCode())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(FamilyInstance lintel)
        {
            // "Описание" типоразмера семейства
            string symbolDescription = WorkWithFamilies.GetSymbolDescription(lintel);

            // "ADSK_Толщина стены" из семейства перемычки или из внешнего семейства (окно/дверь)
            double wallWidth = WorkWithFamilies.GetWallWidth(lintel);

            StringBuilder sb = new StringBuilder();
            // "ADSK_Наименование" вложенных экземпляров семейств в семействе перемычки
            var adskNames = WorkWithFamilies.GetSubComponentsAdskNames(lintel);
            foreach (var adskName in adskNames)
            {
                sb.Append(adskName);
            }

            // Ширина перемычки
            double widthOfLintel = WorkWithFamilies.GetMaxWidthOfLintel(lintel);

            return (symbolDescription + wallWidth + sb + widthOfLintel).GetHashCode();
        }
    }
}
