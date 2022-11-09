using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using MS.GUI.ViewModels.Base;
using MS.Utilites.Extensions;

namespace MS.GUI.ViewModels.KR
{
    public class StairReinforcementViewModel : ViewModelBase
    {
        /// <summary>
        /// Создавать армирование ступеней?
        /// </summary>
        private static bool _createStepFrames = true;

        /// <summary>
        /// Создавать армирование ступеней?
        /// </summary>
        public bool CreateStepFrames
        {
            get => _createStepFrames;
            set => Set(ref _createStepFrames, value);
        }


        /// <summary>
        /// Отступ концов арматурных стержней от торца бетонной грани
        /// </summary>
        private static int _rebarCoverEnd = 20;

        /// <summary>
        /// Отступ концов арматурных стержней от торца бетонной грани
        /// </summary>
        [RegexStringValidator(@"^[1-7][0-9]$")]
        public int RebarCoverEnd
        {
            get => _rebarCoverEnd;
            set => Set(ref _rebarCoverEnd, value);
        }


        ///// <summary>
        ///// Коллекция типов арматурных стержней для каркасов ступеней
        ///// </summary>
        //public ObservableCollection<RebarBarType> RebarTypesSteps { get; }

        ///// <summary>
        ///// Тип арматурных стержней каркаса лестницы
        ///// </summary>
        //private static RebarBarType _selectedRebarTypeSteps;

        ///// <summary>
        ///// Выбранный тип арматурного стержня для каркаса ступени
        ///// </summary>
        //public RebarBarType SelectedRebarTypeSteps
        //{
        //    get => _selectedRebarTypeSteps;
        //    set => Set(ref _selectedRebarTypeSteps, value);
        //}


        ///// <summary>
        ///// Коллекция типов арматурных стержней для рабочей арматуры марша
        ///// </summary>
        //public ObservableCollection<RebarBarType> RebarTypesMain { get; }

        ///// <summary>
        ///// Тип рабочих арматурных стержней лестничного марша
        ///// </summary>
        //private static RebarBarType _selectedRebarTypeMain;

        ///// <summary>
        ///// Выбранный тип арматурного стержня для каркаса ступени
        ///// </summary>
        //public RebarBarType SelectedRebarTypeMain
        //{
        //    get => _selectedRebarTypeMain;
        //    set => Set(ref _selectedRebarTypeMain, value);
        //}


        /// <summary>
        /// Защитный слой арматурных стержней каркасов ступеней
        /// </summary>
        private static int _rebarCoverSteps = 20;

        /// <summary>
        /// Защитный слой арматурных стержней каркасов ступеней
        /// </summary>
        [RegexStringValidator(@"^[1-7][0-9]$")]
        public int RebarCoverSteps
        {
            get => _rebarCoverSteps;
            set => Set(ref _rebarCoverSteps, value);
        }


        /// <summary>
        /// Защитный слой рабочих арматурных стержней марша по наклонной грани
        /// </summary>
        private static int _rebarCoverMainAngle = 25;

        /// <summary>
        /// Защитный слой рабочих арматурных стержней марша по наклонной грани
        /// </summary>
        [RegexStringValidator(@"^[1-7][0-9]$")]
        public int RebarCoverMainAngle
        {
            get => _rebarCoverMainAngle;
            set => Set(ref _rebarCoverMainAngle, value);
        }


        /// <summary>
        /// Защитный слой рабочих арматурных стержней марша по горизонтальным граням
        /// </summary>
        private static int _rebarCoverMainHoriz = 30;

        /// <summary>
        /// Защитный слой рабочих арматурных стержней марша по горизонтальным граням
        /// </summary>
        [RegexStringValidator(@"^[1-7][0-9]$")]
        public int RebarCoverMainHoriz
        {
            get => _rebarCoverMainHoriz;
            set => Set(ref _rebarCoverMainHoriz, value);
        }


        /// <summary>
        /// Шаг Г- стержней каркасов ступеней
        /// </summary>
        private static int _barsStepStepsHorizont = 200;

        /// <summary>
        /// Шаг Г- стержней каркасов ступеней
        /// </summary>
        [RegexStringValidator(@"^[1-9][0-9]{2}$")]
        public int BarsStepStepsHorizont
        {
            get => _barsStepStepsHorizont;
            set => Set(ref _barsStepStepsHorizont, value);
        }


        /// <summary>
        /// Шаг горизонтальных стержней каркасов ступеней
        /// </summary>
        private static int _barsStepStepsVert = 100;

        /// <summary>
        /// Шаг горизонтальных стержней каркасов ступеней
        /// </summary>
        [RegexStringValidator(@"^[1-9][0-9]{2}$")]
        public int BarsStepStepsVert
        {
            get => _barsStepStepsVert;
            set => Set(ref _barsStepStepsVert, value);
        }


        /// <summary>
        /// Шаг поперечных горизонтальных стержней марша
        /// </summary>
        private static int _barsStepMainHorizont = 200;

        /// <summary>
        /// Шаг поперечных горизонтальных стержней марша
        /// </summary>
        [RegexStringValidator(@"^[1-9][0-9]{2}$")]
        public int BarsStepMainHorizont
        {
            get => _barsStepMainHorizont;
            set => Set(ref _barsStepMainHorizont, value);
        }


        /// <summary>
        /// Шаг рабочих продольных Z - стержней марша
        /// </summary>
        private static int _barsStepMainAngle = 200;

        /// <summary>
        /// Шаг рабочих продольных Z - стержней марша
        /// </summary>
        [RegexStringValidator(@"^[1-9][0-9]{2}$")]
        public int BarsStepMainAngle
        {
            get => _barsStepMainAngle;
            set => Set(ref _barsStepMainAngle, value);
        }

        /// <summary>
        /// Конструктор формы для настроек армирования лестницы
        /// </summary>
        /// <param name="doc">Документ, в котором будет армироваться лестница</param>
        //public StairReinforcementViewModel(in Document doc)
        //{
        //    var rebarTypes = new FilteredElementCollector(doc)
        //        .OfClass(typeof(RebarBarType))
        //        .WhereElementIsElementType()
        //        .Cast<RebarBarType>();

        //    RebarTypesSteps = new ObservableCollection<RebarBarType>(rebarTypes);
        //    RebarTypesMain = new ObservableCollection<RebarBarType>(rebarTypes);
        //}

        public StairReinforcementViewModel()
        {
            //RebarTypesSteps = new ObservableCollection<RebarBarType>();
            //RebarTypesMain = new ObservableCollection<RebarBarType>();
        }
    }
}
