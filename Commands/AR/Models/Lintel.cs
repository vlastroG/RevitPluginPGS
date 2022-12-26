using MS.Commands.AR.Enums;
using MS.Commands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models
{
    /// <summary>
    /// Базовый класс перемычки
    /// </summary>
    public abstract class Lintel : IIdentifiable
    {
        /// <summary>
        /// Идентификатор перемычки
        /// </summary>
        private protected readonly Guid _guid;

        /// <summary>
        /// Тип перемычки - из стержней/брусков/уголков
        /// </summary>
        private protected readonly LintelType _lintelType;


        /// <summary>
        /// Конструктор перемычки по идентификатору и ее типу
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="lintelType"></param>
        public Lintel(Guid guid, LintelType lintelType)
        {
            _guid = guid;
            _lintelType = lintelType;
        }


        /// <summary>
        /// Идентификатор перемычки
        /// </summary>
        public Guid Guid => _guid;

        /// <summary>
        /// Тип перемычки
        /// </summary>
        public LintelType LintelType => _lintelType;
    }
}
