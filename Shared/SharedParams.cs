using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Shared
{
    public static class SharedParams
    {
        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки
        /// </summary>
        public static readonly Guid PGS_MarkLintel = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");


        /// <summary>
        /// Guid параметра ADSK_МассаЭлемента
        /// </summary>
        public static readonly Guid ADSK_MassElement = Guid.Parse("5913a1f9-0b38-4364-96fe-a6f3cb7fcc68");


        /// <summary>
        /// Guid параметра PGS_МассаПеремычки
        /// </summary>
        public static readonly Guid PGS_MassLintel = Guid.Parse("77a36313-f239-426c-a6f9-29bf64efee76");

        /// <summary>
        /// Значение параметра "Описание" в типе семейств перемычек
        /// </summary>
        public static readonly string LintelDescription = "Перемычка";
    }
}
