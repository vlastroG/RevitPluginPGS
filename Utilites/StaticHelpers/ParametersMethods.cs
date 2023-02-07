using Autodesk.Revit.DB;

namespace MS.Utilites
{
    public static class ParametersMethods
    {
        /// <summary>
        /// Возвращает Id категории элемента в формате int.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id.IntegerValue ?? 0;
        }
    }
}
