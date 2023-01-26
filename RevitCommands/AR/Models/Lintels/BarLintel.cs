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
    /// Перемычка из арматурных стержней
    /// </summary>
    public class BarLintel : Lintel
    {
        private const string _diameter = "ADSK_Размер_Диаметр";

        private const string _barsStep = "Шаг стержней";

        private const string _supportLeft = "Опирание слева";

        private const string _supportRight = "Опирание справа";


        /// <summary>
        /// Конструктор перемычки по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public BarLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Bar, existLintelId) { }

        public BarLintel(Guid guid, in FamilyInstance lintel) : this(guid, lintel.Id.IntegerValue)
        {
            BarsDiameter = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_diameter).AsDouble(), UnitTypeId.Millimeters);
            BarsStep = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_barsStep).AsDouble(), UnitTypeId.Millimeters);
            SupportLeft = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_supportLeft).AsDouble(), UnitTypeId.Millimeters);
            SupportRight = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_supportRight).AsDouble(), UnitTypeId.Millimeters);
            Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString();
        }

        /// <summary>
        /// Диаметр арматурных стержней в мм
        /// </summary>
        [Description(_diameter)]
        public double BarsDiameter { get; set; } = 12;

        /// <summary>
        /// Опирание слева в мм
        /// </summary>
        [Description(_supportLeft)]
        public double SupportLeft { get; set; } = 250;

        /// <summary>
        /// Опирание справа в мм
        /// </summary>
        [Description(_supportRight)]
        public double SupportRight { get; set; } = 250;

        /// <summary>
        /// Шаг стержней в мм
        /// </summary>
        [Description(_barsStep)]
        public double BarsStep { get; set; } = 60;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is null))
            {
                return (obj is BarLintel lintelOther)
                    && (BarsDiameter == lintelOther.BarsDiameter)
                    && (BarsStep == lintelOther.BarsStep)
                    && (SupportLeft == lintelOther.SupportLeft)
                    && (SupportRight == lintelOther.SupportRight);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return
                BarsDiameter.GetHashCode() +
                BarsStep.GetHashCode() +
                SupportLeft.GetHashCode() +
                SupportRight.GetHashCode() +
                Mark.GetHashCode();
        }
    }
}
