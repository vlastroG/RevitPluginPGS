using MS.GUI.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.GUI.ViewModels.KR
{
    /// <summary>
    /// Настройки для команд из панели КР
    /// </summary>
    public class SettingsViewModelKR : ViewModelBase
    {
        private static string _openingFamName;

        private static string _openingTypeName;

        private static string _openingOffsetString;


        /// <summary>
        /// Название семейства проема, 
        /// которое нужно разместить попересечению воздуховода/трубы и плиты/стены
        /// </summary>
        public string OpeningFamName
        {
            get => _openingFamName;
            set => Set(ref _openingFamName, value.TrimEnd());
        }

        /// <summary>
        /// Название типа из <see cref="OpeningFamName">семейства</see> проема,
        /// которое нужно разместить попересечению воздуховода/трубы и плиты/стены
        /// </summary>
        public string OpeningTypeName
        {
            get => _openingTypeName;
            set => Set(ref _openingTypeName, value.TrimEnd());
        }

        /// <summary>
        /// Отступ внутренних граней проема от наружных граней трубы/воздуховода
        /// </summary>
        [RegexStringValidator(@"[^0-9]+")]
        public string OpeningOffsetString
        {
            get => _openingOffsetString;
            set => Set(ref _openingOffsetString, value.TrimEnd());
        }

        public int OpeningOffset
        {
            get
            {
                if (!int.TryParse(_openingOffsetString, out int offset))
                {
                    throw new ArgumentException($"Нельзя преобразовать строку {_openingOffsetString} в число!");
                }
                return offset;
            }
        }
    }
}
