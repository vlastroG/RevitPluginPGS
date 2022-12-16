using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    /// <summary>
    /// Расширения для строк
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Возвращает строку, в которой символы :;"/\|?*[]{}`~&lt;&gt; заменены на _
        /// </summary>
        /// <param name="fileName">Строка, в которой надо заменить символы</param>
        /// <returns>Новая строка с замененными символами</returns>
        public static string ReplaceForbiddenSymbols(this string fileName)
        {
            return Regex.Replace(fileName, @"[\\<>:;/|?*""\[\]{}`~]", "_");
        }
    }
}
