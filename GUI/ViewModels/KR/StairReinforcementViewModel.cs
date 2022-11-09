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
        /// Путь к документу, в котором армируется лестница
        /// </summary>
        private static string _docPath = String.Empty;

        /// <summary>
        /// Путь к документу, в котором армируется лестница
        /// </summary>
        public string DocPath
        {
            get => _docPath;
            private set => Set(ref _docPath, value);
        }


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


        /// <summary>
        /// Коллекция типов арматурных стержней для каркасов ступеней
        /// </summary>
        public static ObservableCollection<RebarBarType> RebarTypesSteps { get; private set; } = new ObservableCollection<RebarBarType>();

        /// <summary>
        /// Тип арматурных стержней каркаса лестницы
        /// </summary>
        private static RebarBarType _selectedRebarTypeSteps;

        /// <summary>
        /// Выбранный тип арматурного стержня для каркаса ступени
        /// </summary>
        public RebarBarType SelectedRebarTypeSteps
        {
            get => _selectedRebarTypeSteps;
            set => Set(ref _selectedRebarTypeSteps, value);
        }


        /// <summary>
        /// Коллекция типов арматурных стержней для рабочей арматуры марша
        /// </summary>
        public static ObservableCollection<RebarBarType> RebarTypesMain { get; private set; } = new ObservableCollection<RebarBarType>();

        /// <summary>
        /// Тип рабочих арматурных стержней лестничного марша
        /// </summary>
        private static RebarBarType _selectedRebarTypeMain;

        /// <summary>
        /// Выбранный тип арматурного стержня для каркаса ступени
        /// </summary>
        public RebarBarType SelectedRebarTypeMain
        {
            get => _selectedRebarTypeMain;
            set => Set(ref _selectedRebarTypeMain, value);
        }


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
        /// Шаг горизонтальных стержней каркасов ступеней
        /// </summary>
        private static int _barsStepStepsHorizont = 100;

        /// <summary>
        /// Шаг горизонтальных стержней каркасов ступеней
        /// </summary>
        [RegexStringValidator(@"^[1-9][0-9]{2}$")]
        public int BarsStepStepsHorizont
        {
            get => _barsStepStepsHorizont;
            set => Set(ref _barsStepStepsHorizont, value);
        }


        /// <summary>
        /// Шаг Г- стержней каркасов ступеней
        /// </summary>
        private static int _barsStepStepsVert = 200;

        /// <summary>
        /// Шаг Г- стержней каркасов ступеней
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
        public StairReinforcementViewModel(in IEnumerable<RebarBarType> rebarTypesNames, string docPath)
        {
            RebarTypesSteps = new ObservableCollection<RebarBarType>(rebarTypesNames);
            RebarTypesMain = new ObservableCollection<RebarBarType>(rebarTypesNames);
            DocPath = docPath;
        }

        public StairReinforcementViewModel()
        {
        }
    }
}
