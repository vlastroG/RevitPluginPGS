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
    /// ViewModel для окна редактирования свойств перемычки из уголков
    /// </summary>
    public class LintelAngleViewModel : ViewModelBase, ILintelCreator
    {
        /// <summary>
        /// Конструктор модели окна редактирования свойств перемычки по умолчанию
        /// </summary>
        public LintelAngleViewModel()
        {
            _supportLeft = 250;
            _supportRight = 250;
            _stripe = "5x50";
            _stripeStep = 500;
            _angleMain = "75x5";
            _angleExterior = "75x5";
            _angleSupport = "100x10";
        }

        /// <summary>
        /// Конструктор модели окна редактирования свойств перемычки из уголков
        /// </summary>
        /// <param name="angleLintel">Заданная перемычка из уголков</param>
        public LintelAngleViewModel(AngleLintel angleLintel)
        {
            _supportLeft = angleLintel.SupportLeft;
            _supportRight = angleLintel.SupportRight;
            _stripe = angleLintel.Stripe;
            _stripeStep = angleLintel.StripeStep;
            _angleMain = angleLintel.AngleMain;
            _angleExterior = angleLintel.AngleExterior;
            _angleSupport = angleLintel.AngleSupport;
            _supportAngleLeftVisible = 0;
            _supportAngleRightVisible = 0;
            _angleFirstVisible = 0;
            _angleShelvesInside = 0;
        }



        private double _supportLeft;

        /// <summary>
        /// Опирание слева
        /// </summary>
        public double SupportLeft { get => _supportLeft; set => Set(ref _supportLeft, value); }


        private double _supportRight;

        /// <summary>
        /// Опирание справа
        /// </summary>
        public double SupportRight { get => _supportRight; set => Set(ref _supportRight, value); }


        private double _stripeStep;

        /// <summary>
        /// Шаг полосы
        /// </summary>
        public double StripeStep { get => _stripeStep; set => Set(ref _stripeStep, value); }


        private string _angleMain;

        /// <summary>
        /// Основной уголок
        /// </summary>
        public string AngleMain { get => _angleMain; set => Set(ref _angleMain, value); }


        private string _angleExterior;

        /// <summary>
        /// Уголок для облицовки
        /// </summary>
        public string AngleExterior { get => _angleExterior; set => Set(ref _angleExterior, value); }


        private string _angleSupport;

        /// <summary>
        /// Опорный уголок
        /// </summary>
        public string AngleSupport { get => _angleSupport; set => Set(ref _angleSupport, value); }


        private string _stripe;

        /// <summary>
        /// Полоса
        /// </summary>
        public string Stripe { get => _stripe; set => Set(ref _stripe, value); }


        private int _supportAngleLeftVisible;
        /// <summary>
        /// Видимость_опорный_уголок_1
        /// </summary>
        public int SupportAngleLeftVisible
        {
            get => _supportAngleLeftVisible;
            set => Set(ref _supportAngleLeftVisible, value);
        }


        private int _supportAngleRightVisible;
        /// <summary>
        /// Видимость_опорный_уголок_2
        /// </summary>
        public int SupportAngleRightVisible { get => _supportAngleRightVisible; set => Set(ref _supportAngleRightVisible, value); }


        private int _angleFirstVisible;
        /// <summary>
        /// Вкл_Видимость_1_уголок
        /// </summary>
        public int AngleFirstVisible { get => _angleFirstVisible; set => Set(ref _angleFirstVisible, value); }


        private int _angleShelvesInside;
        /// <summary>
        /// Уголок_Полки внутрь
        /// </summary>
        public int AngleShelvesInside { get => _angleShelvesInside; set => Set(ref _angleShelvesInside, value); }


        private double _windowQuarter;
        /// <summary>
        /// Размер четверти
        /// </summary>
        public double WindowQuarter { get => _windowQuarter; set => Set(ref _windowQuarter, value); }


        private double _insulationThickness;
        /// <summary>
        /// Толщина утеплителя
        /// </summary>
        public double InsulationThickness { get => _insulationThickness; set => Set(ref _insulationThickness, value); }


        public Lintel GetLintel(Guid guid)
        {
            return new AngleLintel(guid)
            {
                SupportLeft = _supportLeft,
                SupportRight = _supportRight,
                StripeStep = _stripeStep,
                AngleMain = _angleMain,
                AngleExterior = _angleExterior,
                AngleSupport = _angleSupport,
                Stripe = _stripe
            };
        }
    }
}
