using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.Enums
{
    /// <summary>
    /// Типы перемычек
    /// </summary>
    public enum LintelType
    {
        /// <summary>
        /// Перемычка из арматурных стержней
        /// </summary>
        [Description("Перемычка из арматурных стержней")]
        Bar,
        /// <summary>
        /// Перемычка из брусков
        /// </summary>
        [Description("Перемычка из брусков")]
        Block,
        /// <summary>
        /// Перемычка из уголков
        /// </summary>
        [Description("Перемычка из уголков")]
        Angle
    }
}
