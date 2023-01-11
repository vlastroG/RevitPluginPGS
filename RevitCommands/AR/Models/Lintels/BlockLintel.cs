using MS.RevitCommands.AR.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.Models.Lintels
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
        public BlockLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Block, existLintelId) { }

        /// <summary>
        /// Тип 1-го блока
        /// </summary>
        [Description("Тип 1-го элемента")]
        public string BlockType_1 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 2-го блока
        /// </summary>
        [Description("Тип 2-го элемента")]
        public string BlockType_2 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 3-го блока
        /// </summary>
        [Description("Тип 3-го элемента")]
        public string BlockType_3 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 4-го блока
        /// </summary>
        [Description("Тип 4-го элемента")]
        public string BlockType_4 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 5-го блока
        /// </summary>
        [Description("Тип 5-го элемента")]
        public string BlockType_5 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 6-го блока
        /// </summary>
        [Description("Тип 6-го элемента")]
        public string BlockType_6 { get; set; } = "2ПБ 26-4";
    }
}
