using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Utilites.Extensions
{
    public static class ConnectorSetExtension
    {
        /// <summary>
        /// Возвращает первый Connector из перечисления ConnectorSet
        /// </summary>
        /// <param name="connectorSet">Перечисление коннекторов</param>
        /// <returns>Первый коннектор из перечисления</returns>
        public static Connector GetFirst(this ConnectorSet connectorSet)
        {
            Connector firstConnector = null;
            foreach (Connector connector in connectorSet)
            {
                firstConnector = connector;
                break;
            }
            return firstConnector;
        }
    }
}
