using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.Models.Interfaces
{
    /// <summary>
    /// Интерфейс для идентифицируемых обхектов
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        Guid Guid { get; }
    }
}
