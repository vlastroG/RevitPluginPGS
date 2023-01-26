using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    /// <summary>
    /// Методы расширения для перечислений
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Возвращает значение атрибута Description элемента перечисления
        /// </summary>
        /// <param name="enumObj">Объект перечисления с атрибутом Description</param>
        /// <returns>Значение атрибута Description, или пустая строка</returns>
        public static string GetEnumDescription(this Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj?.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }
    }
}
