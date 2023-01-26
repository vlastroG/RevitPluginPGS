using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.Enums
{
    /// <summary>
    /// Перечисление для определения способа построения отделочных стен
    /// </summary>
    public enum FinWallsHeight
    {
        /// <summary>
        /// Заданная высота отделочной стены
        /// </summary>
        ByInput,
        /// <summary>
        /// Высота отделочной стены по высоте помещения
        /// </summary>
        ByRoom,
        /// <summary>
        /// Высота отделочной стены по элементу
        /// </summary>
        ByElement
    }
}
