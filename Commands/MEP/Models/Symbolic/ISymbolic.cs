using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.MEP.Models.Symbolic
{
    /// <summary>
    /// УГО элементов для вентиляционной установки
    /// </summary>
    public interface ISymbolic
    {
        /// <summary>
        /// Название типа элемента для УГО
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Длина элемента, которое обозначает УГО, в мм
        /// </summary>
        [Range(50, 3000)]
        double Length { get; set; }
    }
}
