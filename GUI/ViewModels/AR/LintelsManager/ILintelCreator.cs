using MS.RevitCommands.AR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Набор базовых методов для класса - конструктора перемычки
    /// </summary>
    public interface ILintelCreator
    {
        /// <summary>
        /// Получить перемычку с заданным Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        /// <returns>Перемычка с заданным Guid</returns>
        Lintel GetLintel(Guid guid);
    }
}
