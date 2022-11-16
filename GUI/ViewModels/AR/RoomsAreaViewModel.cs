using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.AR
{
    /// <summary>
    /// Настройки для квартирографии
    /// </summary>
    public class RoomsAreaViewModel : ViewModelBase
    {
        /// <summary>
        /// True - Расчитывать площади для помещений, видимых на виде
        /// False - Расчитывать площади для помещений во всем проекте
        /// </summary>
        private static bool _roomsOnlyInView = true;

        /// <summary>
        /// True - Расчитывать площади помещений до 2 знака
        /// False - Расчитывать площади помещений до 3 знака
        /// </summary>
        private static bool _twoDecimals = true;

        /// <summary>
        /// True - Расчитывать площади для помещений, видимых на виде
        /// False - Расчитывать площади для помещений во всем проекте
        /// </summary>
        public bool RoomsOnlyInView
        {
            get => _roomsOnlyInView;
            set => Set(ref _roomsOnlyInView, value);
        }

        /// <summary>
        /// True - Расчитывать площади помещений до 2 знака
        /// False - Расчитывать площади помещений до 3 знака
        /// </summary>
        public bool TwoDecimals
        {
            get => _twoDecimals;
            set => Set(ref _twoDecimals, value);
        }
    }
}
