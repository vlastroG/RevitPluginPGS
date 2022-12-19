using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.Models.Interfaces
{
    /// <summary>
    /// Объект с идентификатором
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        int Id { get; set; }
    }
}
