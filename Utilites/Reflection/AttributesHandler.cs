using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Reflection
{
    /// <summary>
    /// Обработчик атрибутов
    /// </summary>
    public static class AttributesHandler
    {
        /// <summary>
        /// Возвращает описание к свойству класса в виде строки
        /// </summary>
        /// <typeparam name="T">Класс, в котором определено целевое свойство</typeparam>
        /// <param name="fieldName">Название целевого свойства</param>
        /// <returns>Значение атрибута описания свойства или null</returns>
        public static string GetDescription<T>(string fieldName)
        {
            string result;
            FieldInfo fi = typeof(T).GetField(fieldName);
            if (fi != null)
            {
                try
                {
                    object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
                    result = (description.Description);
                }
                catch
                {
                    result = null;
                }
            }
            else
            {
                result = null;
            }

            return result;
        }
    }
}
