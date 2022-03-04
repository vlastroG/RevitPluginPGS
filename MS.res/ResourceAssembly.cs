using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.res
{
    /// <summary>
    /// Вспомогательные методы
    /// </summary>
    public class ResourceAssembly
    {
        /// <summary>
        /// Возвращает текущую сборку
        /// </summary>
        /// <returns>Текущая ResourceAssembly</returns>
        public static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Возвращает namespace исполняемой сборки ResourceAssembly
        /// </summary>
        /// <returns>Namespase</returns>
        public static string GetNamespace()
        {
            return typeof(ResourceAssembly).Namespace + ".";
        }
    }
}
