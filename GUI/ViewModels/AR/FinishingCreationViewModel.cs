using Autodesk.Revit.DB;
using MS.Commands.AR.DTO;
using MS.Commands.AR.Enums;
using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR
{
    /// <summary>
    /// ViewModel для настройки команды для генерации отделки
    /// </summary>
    public class FinishingCreationViewModel : ViewModelBase
    {
        /// <summary>
        /// Список Dto для назначения типа отделочных стен отделываемым элементам
        /// </summary>
        public ObservableCollection<WallTypeFinishingDto> DTOs { get; }

        /// <summary>
        /// Список типоразмеров потолков для назначения типа отделки потолка помещений
        /// </summary>
        public ObservableCollection<CeilingType> CeilingTypes { get; }

        /// <summary>
        /// Список типоразмеров отделочных стен для назначения их в Dto
        /// </summary>
        public ObservableCollection<WallType> WallTypes { get; }

        /// <summary>
        /// Выбранный типоразмер потолка
        /// </summary>
        private CeilingType _ceilingType;

        /// <summary>
        /// Выбранный типоразмер потолка
        /// </summary>
        public CeilingType SelectedCeilingType
        {
            get => _ceilingType;
            set => Set(ref _ceilingType, value);
        }

        /// <summary>
        /// Высота отделочных стен, заданная пользователем в мм
        /// </summary>
        private static int _wallsHeight = 2500;

        /// <summary>
        /// Высота отделочных стен, заданная пользователем в мм
        /// </summary>
        public int WallsHeight
        {
            get => _wallsHeight;
            set => Set(ref _wallsHeight, value);
        }

        /// <summary>
        /// Назначать высоту отделочных стен по высоте отделываемых элементов
        /// </summary>
        private static bool _wallsHeightByElements = false;

        /// <summary>
        /// Назначать высоту отделочных стен по высоте отделываемых элементов
        /// </summary>
        public bool WallsHeightByElements
        {
            get => _wallsHeightByElements;
            set => Set(ref _wallsHeightByElements, value);
        }

        /// <summary>
        /// Назначать высоту отделочных стен по высоте помещений
        /// </summary>
        private static bool _wallsHeightByRoom = false;

        /// <summary>
        /// Назначать высоту отделочных стен по высоте помещений
        /// </summary>
        public bool WallsHeightByRoom
        {
            get => _wallsHeightByRoom;
            set => Set(ref _wallsHeightByRoom, value);
        }

        /// <summary>
        /// Назначать высоту отделочных стен по высоте, заданной пользователем
        /// </summary>
        private static bool _wallsHeightByUser = true;

        /// <summary>
        /// Назначать высоту отделочных стен по высоте, заданной пользователем
        /// </summary>
        public bool WallsHeightByUser
        {
            get => _wallsHeightByUser;
            set => Set(ref _wallsHeightByUser, value);
        }

        /// <summary>
        /// Выбранный способ назначения высоты (по элементу/помещению/заданная высота)
        /// </summary>
        public FinWallsHeight WallsHeightType
        {
            get
            {
                if (_wallsHeightByElements)
                    return FinWallsHeight.ByElement;
                if (_wallsHeightByRoom)
                    return FinWallsHeight.ByRoom;
                else return FinWallsHeight.ByInput;
            }
        }

        /// <summary>
        /// Высота потолка, заданная пользователем в мм
        /// </summary>
        private static int _ceilingHeight = 2400;

        /// <summary>
        /// Высота потолка, заданная пользователем в мм
        /// </summary>
        public int CeilingHeight
        {
            get => _ceilingHeight;
            set => Set(ref _ceilingHeight, value);
        }

        /// <summary>
        /// Назначать высоту потолков по помещению
        /// </summary>
        private static bool _ceilingHeightByRoom = false;

        /// <summary>
        /// Назначать высоту потолков по помещению
        /// </summary>
        public bool CeilingHeightByRoom
        {
            get => _ceilingHeightByRoom;
            set => Set(ref _ceilingHeightByRoom, value);
        }

        /// <summary>
        /// Создавать потолки
        /// </summary>
        private static bool _createCeiling = false;

        /// <summary>
        /// Создавать потолки
        /// </summary>
        public bool CreateCeiling
        {
            get => _createCeiling;
            set => Set(ref _createCeiling, value);
        }

        /// <summary>
        /// Создавать отделочные стены
        /// </summary>
        private static bool _createWalls = false;

        /// <summary>
        /// Создавать отделочные стены
        /// </summary>
        public bool CreateWalls
        {
            get => _createWalls;
            set => Set(ref _createWalls, value);
        }


        /// <summary>
        /// Конструктор по умолчанию с пустыми списками
        /// </summary>
        public FinishingCreationViewModel()
        {
            DTOs = new ObservableCollection<WallTypeFinishingDto>();
            WallTypes = new ObservableCollection<WallType>();
            CeilingTypes = new ObservableCollection<CeilingType>();
        }

        /// <summary>
        /// Конструктор ViewModel с заданными списками
        /// </summary>
        /// <param name="wallTypes">Список типоразмеров отделочных стен, 
        /// доступных для назначения в Dto</param>
        /// <param name="ceilingTypes">Список типоразмеров потолков,
        /// доступных для выбора пользователем</param>
        /// <param name="wallDtos">Список Dto для отделочных стен</param>
        public FinishingCreationViewModel(
            IEnumerable<WallType> wallTypes,
            IEnumerable<CeilingType> ceilingTypes,
            IEnumerable<WallTypeFinishingDto> wallDtos)
        {
            DTOs = new ObservableCollection<WallTypeFinishingDto>(wallDtos);
            WallTypes = new ObservableCollection<WallType>(wallTypes);
            CeilingTypes = new ObservableCollection<CeilingType>(ceilingTypes);
        }
    }
}
