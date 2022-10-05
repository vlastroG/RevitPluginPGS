using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO.FinishingCreateionCmd
{
    /// <summary>
    /// DTO для создания стен по границе помещения (команда "Ведомость отделки")
    /// </summary>
    public class WallDto
    {
        /// <summary>
        /// Конструктор DTO для создания отделочных стен по границам помещений
        /// </summary>
        /// <param name="Segment">Граница помещения</param>
        /// <param name="LevelId">Id уровня, на котором должна располагаться стена</param>
        /// <param name="HRoom">Высота помещения, которому принадлежит граница для создания отделочной стены</param>
        /// <param name="HElem">Высота элемента, который образуетграницу помещения</param>
        /// <param name="RoomBottomOffset">Смещение снизу от уровня</param>
        public WallDto(BoundarySegment Segment, ElementId LevelId, double HRoom, double HElem, double RoomBottomOffset)
        {
            this.Segment = Segment;
            this.LevelId = LevelId;
            this.HRoom = HRoom;
            this.HElem = HElem;
            this.RoomBottomOffset = RoomBottomOffset;
        }

        /// <summary>
        /// Линия границы помещения
        /// </summary>
        public BoundarySegment Segment { get; }

        /// <summary>
        /// Id уровня, на котором должна располагаться отделочная стена
        /// </summary>
        public ElementId LevelId { get; }

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
    }
}