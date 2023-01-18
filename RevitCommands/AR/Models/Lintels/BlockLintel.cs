using Autodesk.Revit.DB;
using MS.RevitCommands.AR.Enums;
using MS.Shared;
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
        private const string _blockType1 = "Тип 1-го элемента";
        private const string _blockType2 = "Тип 2-го элемента";
        private const string _blockType3 = "Тип 3-го элемента";
        private const string _blockType4 = "Тип 4-го элемента";
        private const string _blockType5 = "Тип 5-го элемента";
        private const string _blockType6 = "Тип 6-го элемента";
        private const string _windowQuarter = "Размер четверти";
        private const string _insulationThickness = "Толщина утеплителя";
        private const string _firstBlockWithQuarter = "1-ый элемент перемычки с четвертью";
        private const string _angleSupport = "Уголок_Опирание";


        /// <summary>
        /// Конструктор перемычки по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public BlockLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Block, existLintelId) { }

        public BlockLintel(Guid guid, in FamilyInstance lintel) : this(guid, lintel.Id.IntegerValue)
        {
            BlockType_1 = lintel.LookupParameter(_blockType1).AsValueString();
            BlockType_2 = lintel.LookupParameter(_blockType2).AsValueString();
            BlockType_3 = lintel.LookupParameter(_blockType3).AsValueString();
            BlockType_4 = lintel.LookupParameter(_blockType4).AsValueString();
            BlockType_5 = lintel.LookupParameter(_blockType5).AsValueString();
            BlockType_6 = lintel.LookupParameter(_blockType6).AsValueString();
            Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString();
            WindowQuarter = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_windowQuarter).AsDouble(), UnitTypeId.Millimeters);
            InsulationThickness = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_insulationThickness).AsDouble(), UnitTypeId.Millimeters);
            FirstBlockWithQuarter = lintel.LookupParameter(_firstBlockWithQuarter).AsInteger() == 1;
            AngleSupport = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_angleSupport).AsDouble(), UnitTypeId.Millimeters);
        }


        /// <summary>
        /// Тип 1-го блока
        /// </summary>
        [Description(_blockType1)]
        public string BlockType_1 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 2-го блока
        /// </summary>
        [Description(_blockType2)]
        public string BlockType_2 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 3-го блока
        /// </summary>
        [Description(_blockType3)]
        public string BlockType_3 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 4-го блока
        /// </summary>
        [Description(_blockType4)]
        public string BlockType_4 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 5-го блока
        /// </summary>
        [Description(_blockType5)]
        public string BlockType_5 { get; set; } = "2ПБ 26-4";

        /// <summary>
        /// Тип 6-го блока
        /// </summary>
        [Description(_blockType6)]
        public string BlockType_6 { get; set; } = "5ПБ 18-27";


        /// <summary>
        /// Размер четверти в мм
        /// </summary>
        [Description(_windowQuarter)]
        public double WindowQuarter { get; set; } = 0;

        /// <summary>
        /// Толщина утеплителя в мм
        /// </summary>
        [Description(_insulationThickness)]
        public double InsulationThickness { get; set; } = 0;

        /// <summary>
        /// 1-й элемент перемычки с четвертью
        /// </summary>
        [Description(_firstBlockWithQuarter)]
        public bool FirstBlockWithQuarter { get; set; } = false;

        /// <summary>
        /// Уголок_Опирание
        /// </summary>
        [Description(_angleSupport)]
        public double AngleSupport { get; set; } = 250;


        public override bool Equals(object obj)
        {
            if (!(obj is null))
            {
                return (obj is BlockLintel lintelOther)
                    && (BlockType_1 == lintelOther.BlockType_1)
                    && (BlockType_2 == lintelOther.BlockType_2)
                    && (BlockType_3 == lintelOther.BlockType_3)
                    && (BlockType_4 == lintelOther.BlockType_4)
                    && (BlockType_5 == lintelOther.BlockType_5)
                    && (BlockType_6 == lintelOther.BlockType_6)
                    && (WindowQuarter == lintelOther.WindowQuarter)
                    && (InsulationThickness == lintelOther.InsulationThickness)
                    && (FirstBlockWithQuarter == lintelOther.FirstBlockWithQuarter)
                    && (AngleSupport == lintelOther.AngleSupport);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return
                BlockType_1.GetHashCode() +
                BlockType_2.GetHashCode() +
                BlockType_3.GetHashCode() +
                BlockType_4.GetHashCode() +
                BlockType_5.GetHashCode() +
                BlockType_6.GetHashCode() +
                WindowQuarter.GetHashCode() +
                InsulationThickness.GetHashCode() +
                FirstBlockWithQuarter.GetHashCode() +
                AngleSupport.GetHashCode() +
                Mark.GetHashCode();
        }
    }
}
