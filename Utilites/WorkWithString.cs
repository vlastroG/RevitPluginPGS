using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites
{
    public static class WorkWithString
    {
        /// <summary>
        /// Определяет раздел проекта из названия файла АР/КР/ОВ/ВК/ЭМ/СС
        /// </summary>
        /// <param name="doc">Файл Revit</param>
        /// <returns>Шифр раздела или пустая строка</returns>
        public static string GetProjectSectionName(Document doc)
        {
            var doc_proj_section = String.Empty;
            var doc_title = doc.Title;
            if (doc_title.Contains("АР")) doc_proj_section = "АР";
            else if (doc_title.Contains("КР")) doc_proj_section = "КР";
            else if (doc_title.Contains("ОВ")) doc_proj_section = "ОВ";
            else if (doc_title.Contains("ВК")) doc_proj_section = "ВК";
            else if (doc_title.Contains("ЭМ")) doc_proj_section = "ЭМ";
            else if (doc_title.Contains("СС")) doc_proj_section = "СС";

            return doc_proj_section;
        }


        /// <summary>
        /// Преобразовывает double в string в формате +/-0.0000
        /// </summary>
        /// <param name="d_number">double значение</param>
        /// <returns>Возвращаемая строка в формате +/-0.0000</returns>
        public static string GetStringFromDouble(double d_number)
        {
            var str = d_number.ToString().Replace(',', '.');
            if (d_number >= 0)
            {
                str = '+' + str;
            }

            return str;
        }
    }
}
