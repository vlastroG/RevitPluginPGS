using MS.Commands.AR.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models.Lintels
{
    /// <summary>
    /// Перемычка из брусков
    /// </summary>
    public class BlockLintel : Lintel
    {
        /// <summary>
        /// Конструктор перемычки по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public BlockLintel(Guid guid) : base(guid, LintelType.Block) { }
    }
}
