using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.AR.Models.Lintels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR.LintelsManager
{
    /// <summary>
    /// Модель представления окна для редактирования свойств перемычки из арматурных стержней
    /// </summary>
    public class LintelBarViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели представления окна редактирования свойств перемычки из арматурных стержней по умолчанию
        /// </summary>
        public LintelBarViewModel()
        {

        }

        /// <summary>
        /// Конструктор модели представления окна редактирования свойств заданной перемычки из арматурных стержней
        /// </summary>
        /// <param name="barLintel"></param>
        public LintelBarViewModel(BarLintel barLintel)
        {
            _barsDiameter = barLintel.BarsDiameter;
            _barsStep = barLintel.BarsStep;
            _supportLeft = barLintel.SupportLeft;
            _supportRight = barLintel.SupportRight;
        }


        /// <summary>
        /// Диаметр арматурных стержней в мм
        /// </summary>
        private double _barsDiameter;

        /// <summary>
        /// Диаметр арматурных стержней в мм
        /// </summary>
        public double BarsDiameter
        {
            get => _barsDiameter;
            set => Set(ref _barsDiameter, value);
        }

        /// <summary>
        /// Шаг стержней в мм
        /// </summary>
        private double _barsStep;

        /// <summary>
        /// Шаг стержней в мм
        /// </summary>
        public double BarsStep
        {
            get => _barsStep;
            set => Set(ref _barsStep, value);
        }

        /// <summary>
        /// Опирание слева в мм
        /// </summary>
        private double _supportLeft;

        /// <summary>
        /// Опирание слева в мм
        /// </summary>
        public double SupportLeft
        {
            get => _supportLeft;
            set => Set(ref _supportLeft, value);
        }

        /// <summary>
        /// Опирание справа в мм
        /// </summary>
        private double _supportRight;

        /// <summary>
        /// Опирание справа в мм
        /// </summary>
        public double SupportRight
        {
            get => _supportRight;
            set => Set(ref _supportRight, value);
        }


        public Lintel GetLintel(Guid guid)
        {
            return new BarLintel(guid)
            {
                BarsDiameter = _barsDiameter,
                BarsStep = _barsStep,
                SupportLeft = _supportLeft,
                SupportRight = _supportRight
            };
        }
    }
}
