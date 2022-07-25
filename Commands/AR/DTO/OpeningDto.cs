using Autodesk.Revit.DB;
using MS.Utilites;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    /// <summary>
    /// Data transfer object для семейств категорий окон и дверей (проемов)
    /// </summary>
    public class OpeningDto
    {
        /// <summary>
        /// Guid параметра ADSK_Марка
        /// </summary>
        private static readonly Guid _parAdskMarkOfSymbol = Guid.Parse("2204049c-d557-4dfc-8d70-13f19715e46d");

        /// <summary>
        /// Guid параметра PGS_МаркаПеремычки
        /// </summary>
        private static readonly Guid _parPgsLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        /// <summary>
        /// Guid параметра ADSK_Толщина стены
        /// </summary>
        private static readonly Guid _parAdskWallWidth = Guid.Parse("9350e48f-842b-4c46-a15d-2e36ab1f352f");

        /// <summary>
        /// Guid параметра Мрк.МаркаКонструкции
        /// </summary>
        private static readonly Guid _parMrkMarkConstruction = Guid.Parse("5d369dfb-17a2-4ae2-a1a1-bdfc33ba7405");


        /// <summary>
        /// Марка перемычки, берущаяся из PGS_МаркаПеремычки окна или двери
        /// </summary>
        private string _pgsLintelMark;

        /// <summary>
        /// Марка проема, берущаяся из Марки двери или окна
        /// </summary>
        private string _openingMark;

        /// <summary>
        /// Словарь марок перемычек по хэш-коду DTO 
        /// </summary>
        private static readonly Dictionary<int, string> _dictLintelMarkByHashCode = new Dictionary<int, string>();

        /// <summary>
        /// Словарь марок проемов по хэш-коду DTO
        /// </summary>
        private static readonly Dictionary<int, string> _dictOpeningMarkByHashCode = new Dictionary<int, string>();

        /// <summary>
        /// Экземпляр семейства категории окна или двери
        /// </summary>
        public FamilyInstance Opening { get; private set; }

        public OpeningDto(FamilyInstance opening)
        {
            if (ValidateInput(opening))
            {
                Opening = opening;
                try
                {
                    _pgsLintelMark = opening
                        .get_Parameter(_parPgsLintelMark)
                        .AsValueString();
                }
                catch (ArgumentNullException)
                {
                    throw new ArgumentNullException($"В экземпляре семейства с Id = {opening.Id} " +
                        $"отсутствует параметр PGS_МаркаПеремычки.");
                }
                _openingMark = opening
                    .get_Parameter(BuiltInParameter.ALL_MODEL_MARK)
                    .AsValueString();
                _dictLintelMarkByHashCode.AddOrUpdate(GetHashCode(), _pgsLintelMark);
                _dictOpeningMarkByHashCode.AddOrUpdate(GetHashCode(), _openingMark);
            }
            else
            {
                throw new ArgumentException(nameof(opening));
            }
        }

        /// <summary>
        /// Категория семейства проема (дверь или окно)
        /// </summary>
        public string OpeningCategory
        {
            get { return Opening.Category.Name; }
        }

        /// <summary>
        /// Уровень семейства проема
        /// </summary>
        public string Level
        {
            get
            {
                return Opening.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                    .AsValueString();
            }
        }

        /// <summary>
        /// ADSK_Марка типа проема
        /// </summary>
        public string AdskMark
        {
            get
            {
                return Opening.Symbol
                    .get_Parameter(_parAdskMarkOfSymbol)
                    .AsValueString();
            }
        }

        /// <summary>
        /// Марка перемычки проема внутри DTO
        /// </summary>
        public string PgsLintelMark
        {
            get
            {
                return _pgsLintelMark;
            }
            set
            {
                _pgsLintelMark = value;
                _dictLintelMarkByHashCode[GetHashCode()] = _pgsLintelMark;
            }
        }

        /// <summary>
        /// ADSK_Толщина стены из проема
        /// </summary>
        public double AdskWallWidth
        {
            get
            {
                if (Opening.get_Parameter(_parAdskWallWidth) != null)
                {
                    return Double.Parse(Opening
                        .get_Parameter(_parAdskWallWidth)
                        .AsValueString());
                }
                else return 0;
            }
        }

        /// <summary>
        /// Встроенный параметр Ширина из проема
        /// </summary>
        public double Width
        {
            get
            {
                return Double.Parse(Opening
                    .get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM)
                    .AsValueString());
            }
        }

        /// <summary>
        /// Марка проема внутри DTO
        /// </summary>
        public string Mark
        {
            get
            {
                return _openingMark;
            }
            set
            {
                _openingMark = value;
                _dictOpeningMarkByHashCode[GetHashCode()] = _openingMark;
            }
        }

        /// <summary>
        /// Словарь хэш-кода и марки перемычки DTO
        /// </summary>
        public static IReadOnlyDictionary<int, string> DictLintelMarkByHashCode
        {
            get
            {
                return _dictLintelMarkByHashCode;
            }
        }

        /// <summary>
        /// Словарь хэш-кода и марки проема DTO
        /// </summary>
        public static IReadOnlyDictionary<int, string> DictOpeningMarkByHashCode
        {
            get
            {
                return _dictOpeningMarkByHashCode;
            }
        }


        /// <summary>
        /// Валидация семейства проема для создания DTO
        /// </summary>
        /// <param name="opening">Экземпляр семейства окна или двери.</param>
        /// <returns>True, если семейство окна или двери валидно для создания DTO, иначе False</returns>
        private bool ValidateInput(FamilyInstance opening)
        {
            if (opening.Host == null)
            {
                return false;
            }
            BuiltInCategory hostCategory = (BuiltInCategory)opening.Host.Category.Id.IntegerValue;
            BuiltInCategory openingCategory = (BuiltInCategory)opening.Category.Id.IntegerValue;
            if (hostCategory == BuiltInCategory.OST_Walls
                && (openingCategory == BuiltInCategory.OST_Doors
                 || openingCategory == BuiltInCategory.OST_Windows)
                 && opening.get_Parameter(_parPgsLintelMark) != null
                 && opening.get_Parameter(_parMrkMarkConstruction) != null
                 && opening.Symbol.get_Parameter(_parAdskMarkOfSymbol) != null)
            {
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Получение хэш-кода из суммы string значений 'Категории', 'ADSK_Толщина стены', 'Ширина'
        /// </summary>
        /// <returns>Хэш-код суммы строк.</returns>
        public override int GetHashCode()
        {
            return (OpeningCategory + AdskWallWidth + Width).GetHashCode();
        }

        /// <summary>
        /// Переопределение сравнения DTO по хэш-коду
        /// </summary>
        /// <param name="obj">Подаваемый объект.</param>
        /// <returns>True, если подаваемый объект является OpeningDto и его хэш-код совпадает с текущим.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is OpeningDto))
                return false;
            else
                return GetHashCode().Equals(obj.GetHashCode());
        }
    }
}
