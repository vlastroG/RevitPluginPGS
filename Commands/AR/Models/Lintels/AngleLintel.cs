using MS.Commands.AR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models.Lintels
{
    /// <summary>
    /// Перемычка из уголков
    /// </summary>
    public class AngleLintel : Lintel
    {
        /// <summary>
        /// Конструктор перемычки из уголков по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public AngleLintel(Guid guid) : base(guid, LintelType.Angle) { }
    }
}
