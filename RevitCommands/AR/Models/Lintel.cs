using MS.RevitCommands.AR.Enums;
using MS.RevitCommands.Models.Interfaces;
using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.Models
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
        /// Id размещенного в проекте экземпляра семейства перемычки
        /// </summary>
        private protected int _existLintelId = -1;


        /// <summary>
        /// Конструктор перемычки по идентификатору и ее типу
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="lintelType"></param>
        public Lintel(Guid guid, LintelType lintelType, int existLintelId = -1)
        {
            _guid = guid;
            _lintelType = lintelType;
            _existLintelId = existLintelId;
        }

        /// <summary>
        /// Идентификатор перемычки
        /// </summary>
        public Guid Guid => _guid;

        /// <summary>
        /// Тип перемычки
        /// </summary>
        public LintelType LintelType => _lintelType;

        /// <summary>
        /// Id размещенного в проекте экземпляра семейства перемычки
        /// </summary>
        public int ExistLintelId => _existLintelId;

        /// <summary>
        /// Возвращает тип перемычки
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return LintelType.GetEnumDescription();
        }
    }
}
