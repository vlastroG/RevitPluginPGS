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
            throw new NotImplementedException();
        }

        public int GetHashCode(FamilyInstance lintel)
        {
            // "Описание" типоразмера семейства
            string symbolDescription = WorkWithFamilies.GetSymbolDescription(lintel);

            // "ADSK_Толщина стены" из семейства перемычки или из внешнего семейства (окно/дверь)
            double wallWidth = WorkWithFamilies.GetWallWidth(lintel);

            StringBuilder sb = new StringBuilder();
            var adskNames = WorkWithFamilies.GetSubComponentsAdskNames(lintel);
            foreach (var adskName in adskNames)
            {
                sb.Append(adskName);
            }
            string subCompsAdskNames = sb.ToString();

            double wodthOfLintel = WorkWithFamilies.GetMaxWidthOfLintel(lintel);

            // "ADSK_Наименование" вложенных экземпляров семейств в семействе перемычки

            throw new NotImplementedException();
        }



    }
}
