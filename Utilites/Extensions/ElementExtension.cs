using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    /// <summary>
    /// Методы расширения для класса Autodesk.Revit.DB.Element 
    /// </summary>
    public static class ElementExtension
    {
        /// <summary>
        /// Назначает значение параметру элемента по названию
        /// </summary>
        /// <param name="element">Элемент Revit, которому назначается значение параметра</param>
        /// <param name="parName">Название параметра, у которого нужно скорректировать значение</param>
        /// <param name="parValue">Значение параметра, которое нужно назначить</param>
        /// <returns>True, если значение успешно назначено, иначе False</returns>
        /// <exception cref="NullReferenceException">Исключение, если попытка установить null в качестве значения</exception>
        public static bool SetParameterValueByName(this Element element, string parName, dynamic parValue)
        {
            if (parValue is null)
            {
                throw new NullReferenceException(nameof(parValue));
            }
            var elParameter = element.LookupParameter(parName);

            var value = parValue;
            var storageType = elParameter.StorageType;
            try
            {
                if ((storageType == StorageType.Integer) || (storageType == StorageType.Double))
                {
                    var parType = elParameter.GetUnitTypeId();
                    value = UnitUtils.ConvertToInternalUnits(value, parType);
                }
                else if (storageType == StorageType.ElementId)
                {
                    value = new ElementId((int)value);
                }
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                value = parValue;
            }
            try
            {
                switch (storageType)
                {
                    case StorageType.None:
                        return false;
                    case StorageType.Integer:
                        var existValueInt = elParameter.AsInteger();
                        if (existValueInt != value)
                        {
                            elParameter.Set(value);
                        }
                        return true;
                    case StorageType.Double:
                        var existValueDouble = elParameter.AsDouble();
                        if (existValueDouble != value)
                        {
                            elParameter.Set(value);
                        }
                        return true;
                    case StorageType.String:
                        var existValueString = elParameter.AsValueString();
                        if (existValueString != value)
                        {
                            elParameter.Set(value);
                        }
                        return true;
                    case StorageType.ElementId:
                        var existValueId = elParameter.AsElementId();
                        if (existValueId.IntegerValue != value.IntegerValue)
                        {
                            elParameter.Set(value);
                        }
                        return true;
                    default:
                        return false;
                }
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                return false;
            }
        }
    }
}
