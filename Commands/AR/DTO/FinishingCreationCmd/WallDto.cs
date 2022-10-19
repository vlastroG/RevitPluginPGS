using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO.FinishingCreationCmd
{
    /// <summary>
    /// DTO для создания стен по границе помещения (команда "Ведомость отделки")
    /// </summary>
    public class WallDto
    {
        private List<ElementId> _elementsToJoin = new List<ElementId>();

        /// <summary>
        /// Линия границы помещения
        /// </summary>
        public Curve Curve { get; }

        /// <summary>
        /// Id уровня, на котором должна располагаться отделочная стена
        /// </summary>
        public ElementId LevelId { get; }

        /// <summary>
        /// Значение параметра элемента PGS_ТипОтделкиСтен, по которому выполняется отделка
        /// </summary>
        public string FinTypeName { get; }

        /// <summary>
        /// Высота помещения, в котором будет расположена отделочная стена
        /// </summary>
        public double HRoom { get; }

        /// <summary>
        /// Высота элемента, который образует границу помещения для построения стены
        /// </summary>
        public double HElem { get; }

        /// <summary>
        /// Смещение снизу от уровня для стены
        /// </summary>
        public double RoomBottomOffset { get; }

        /// <summary>
        /// Смещение снизу от уровня для элемента, по которому будет создаваться отделка
        /// </summary>
        public double ElementBottomOffset { get; }

        /// <summary>
        /// Список элементов, с которыми нужно будет соединить созданную отделочную стену
        /// </summary>
        public IReadOnlyList<ElementId> ElementsToJoin { get { return _elementsToJoin; } }

        /// <summary>
        /// Конструктор DTO для создания отделочных стен по границам помещений
        /// </summary>
        /// <param name="Curve">Граница помещения</param>
        /// <param name="FinTypeName">Значение параметра элемента PGS_ТипОтделкиСтен, 
        /// по которому выполняется отделка</param>
        /// <param name="LevelId">Id уровня, на котором должна располагаться стена</param>
        /// <param name="HRoom">Высота помещения, которому принадлежит граница для создания отделочной стены</param>
        /// <param name="HElem">Высота элемента, который образуетграницу помещения</param>
        /// <param name="RoomBottomOffset">Смещение снизу от уровня для помещения, в котором создается отделка</param>
        /// <param name="ElemBottomOffset">Смещение снизу от уровня для элемента, по которому создается отделка</param>
        public WallDto(
            Curve Curve,
            ElementId LevelId,
            string FinTypeName,
            double HRoom,
            double HElem,
            double RoomBottomOffset,
            double ElemBottomOffset,
            IEnumerable<ElementId> elementsToJoin)
        {
            this.Curve = Curve;
            this.LevelId = LevelId;
            this.FinTypeName = FinTypeName;
            this.HRoom = HRoom;
            this.HElem = HElem;
            this.RoomBottomOffset = RoomBottomOffset;
            this.ElementBottomOffset = ElemBottomOffset;
            _elementsToJoin.AddRange(elementsToJoin);
        }
    }
}