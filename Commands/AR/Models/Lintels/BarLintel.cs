using MS.Commands.AR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models.Lintels
{
    /// <summary>
    /// Перемычка из арматурных стержней
    /// </summary>
    public class BarLintel : Lintel
    {
        /// <summary>
        /// Конструктор перемычки по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public BarLintel(Guid guid) : base(guid, LintelType.Bar) { }
    }
}
