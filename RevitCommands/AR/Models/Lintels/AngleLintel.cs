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
    /// Перемычка из уголков
    /// </summary>
    public class AngleLintel : Lintel
    {
        private const string _angleExterior = "Уголок для облицовки";
        private const string _angleFirstVisible = "Вкл_Видимость_1 уголок";
        private const string _angleMain = "Внутренний уголок";
        private const string _angleShelvesInside = "Уголок_Полки внутрь";
        private const string _angleSupport = "Опорные уголки";
        private const string _insulationThickness = "Толщина утеплителя";
        private const string _stripe = "Полоса";
        private const string _stripeOffset = "Полоса_Отступ";
        private const string _stripeStep = "Полоса_Шаг";
        private const string _supportAngleLeftVisible = "Видимость_опорный уголок_1";
        private const string _supportAngleRightVisible = "Видимость_опорный уголок_2";
        private const string _supportLeft = "уголок_опирание_1";
        private const string _supportRight = "уголок_опирание_2";
        private const string _windowQuarter = "Размер четверти";


        /// <summary>
        /// Конструктор перемычки из уголков по Guid
        /// </summary>
        /// <param name="guid">Идентификатор перемычки</param>
        public AngleLintel(Guid guid, int existLintelId = -1) : base(guid, LintelType.Angle, existLintelId) { }

        public AngleLintel(Guid guid, in FamilyInstance lintel) : this(guid, lintel.Id.IntegerValue)
        {
            AngleExterior = lintel.LookupParameter(_angleExterior).AsValueString();
            AngleMain = lintel.LookupParameter(_angleMain).AsValueString();
            AngleSupport = lintel.LookupParameter(_angleSupport).AsValueString();
            Stripe = lintel.LookupParameter(_stripe).AsValueString();
            StripeStep = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_stripeStep).AsDouble(), UnitTypeId.Millimeters);
            SupportLeft = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_supportLeft).AsDouble(), UnitTypeId.Millimeters);
            SupportRight = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_supportRight).AsDouble(), UnitTypeId.Millimeters);
            Mark = lintel.get_Parameter(SharedParams.PGS_MarkLintel).AsValueString();
            SupportAngleLeftVisible = lintel.LookupParameter(_supportAngleLeftVisible).AsInteger() == 1;
            SupportAngleRightVisible = lintel.LookupParameter(_supportAngleRightVisible).AsInteger() == 1;
            AngleFirstVisible = lintel.LookupParameter(_angleFirstVisible).AsInteger() == 1;
            AngleShelvesInside = lintel.LookupParameter(_angleShelvesInside).AsInteger() == 1;
            WindowQuarter = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_windowQuarter).AsDouble(), UnitTypeId.Millimeters);
            InsulationThickness = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_insulationThickness).AsDouble(), UnitTypeId.Millimeters);
            StripeOffset = UnitUtils.ConvertFromInternalUnits(
                lintel.LookupParameter(_stripeOffset).AsDouble(), UnitTypeId.Millimeters);
        }


        /// <summary>
        /// Опирание слева
        /// </summary>
        [Description(_supportLeft)]
        public double SupportLeft { get; set; } = 250;

        /// <summary>
        /// Опирание справа
        /// </summary>
        [Description(_supportRight)]
        public double SupportRight { get; set; } = 250;

        /// <summary>
        /// Шаг полосы
        /// </summary>
        [Description(_stripeStep)]
        public double StripeStep { get; set; } = 300;

        /// <summary>
        /// Основной уголок
        /// </summary>
        [Description(_angleMain)]
        public string AngleMain { get; set; } = "63x5";

        /// <summary>
        /// Уголок для облицовки
        /// </summary>
        [Description(_angleExterior)]
        public string AngleExterior { get; set; } = "63x5";

        /// <summary>
        /// Опорный уголок
        /// </summary>
        [Description(_angleSupport)]
        public string AngleSupport { get; set; } = "75x5";

        /// <summary>
        /// Полоса
        /// </summary>
        [Description(_stripe)]
        public string Stripe { get; set; } = "5x50";

        /// <summary>
        /// Видимость опорного уголка слева
        /// </summary>
        [Description(_supportAngleLeftVisible)]
        public bool SupportAngleLeftVisible { get; set; } = false;

        /// <summary>
        /// Видимость опорного уголка справа
        /// </summary>
        [Description(_supportAngleRightVisible)]
        public bool SupportAngleRightVisible { get; set; } = false;

        /// <summary>
        /// Видимость первого уголка
        /// </summary>
        [Description(_angleFirstVisible)]
        public bool AngleFirstVisible { get; set; } = true;

        /// <summary>
        /// Полки уголков внутрь
        /// </summary>
        [Description(_angleShelvesInside)]
        public bool AngleShelvesInside { get; set; } = false;

        /// <summary>
        /// Размер четверти в мм
        /// </summary>
        [Description(_windowQuarter)]
        public double WindowQuarter { get; set; } = 0;

        /// <summary>
        /// Размер четверти в мм
        /// </summary>
        [Description(_stripeOffset)]
        public double StripeOffset { get; set; } = 20;

        /// <summary>
        /// Толщина утеплителя в мм
        /// </summary>
        [Description(_insulationThickness)]
        public double InsulationThickness { get; set; } = 0;


        public override bool Equals(object obj)
        {
            if (!(obj is null))
            {
                return (obj is AngleLintel lintelOther)
                    && (AngleExterior == lintelOther.AngleExterior)
                    && (AngleMain == lintelOther.AngleMain)
                    && (AngleSupport == lintelOther.AngleSupport)
                    && (Stripe == lintelOther.Stripe)
                    && (StripeStep == lintelOther.StripeStep)
                    && (SupportLeft == lintelOther.SupportLeft)
                    && (SupportRight == lintelOther.SupportRight)
                    && (SupportAngleLeftVisible == lintelOther.SupportAngleLeftVisible)
                    && (SupportAngleRightVisible == lintelOther.SupportAngleRightVisible)
                    && (AngleFirstVisible == lintelOther.AngleFirstVisible)
                    && (AngleShelvesInside == lintelOther.AngleShelvesInside)
                    && (WindowQuarter == lintelOther.WindowQuarter)
                    && (InsulationThickness == lintelOther.InsulationThickness)
                    && (StripeOffset == lintelOther.StripeOffset);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return
                AngleExterior.GetHashCode() +
                AngleMain.GetHashCode() +
                AngleSupport.GetHashCode() +
                Stripe.GetHashCode() +
                StripeStep.GetHashCode() +
                SupportLeft.GetHashCode() +
                SupportRight.GetHashCode() +
                SupportAngleLeftVisible.GetHashCode() +
                SupportAngleRightVisible.GetHashCode() +
                AngleFirstVisible.GetHashCode() +
                AngleShelvesInside.GetHashCode() +
                WindowQuarter.GetHashCode() +
                InsulationThickness.GetHashCode() +
                StripeOffset.GetHashCode() +
                Mark.GetHashCode();
        }
    }
}
