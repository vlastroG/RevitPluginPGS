using Autodesk.Revit.DB;
using MS.Shared;
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
        /// Марка перемычки, берущаяся из PGS_МаркаПеремычки окна или двери
        /// </summary>
        private string _lintelMark;

        /// <summary>
        /// Марка проема, берущаяся из Марки двери или окна
        /// </summary>
        private string _openingMark;

        /// <summary>
        /// Словарь марок перемычек по хэш-коду DTO 
        /// </summary>
        private static readonly Dictionary<int, string> _dictLintelMarkByHashCode = new Dictionary<int, string>();


        /// <summary>
        /// Экземпляр семейства категории окна или двери
        /// </summary>
        public FamilyInstance Opening { get; private set; }

        public OpeningDto(Document doc, FamilyInstance opening)
        {
            if (ValidateInput(doc, opening))
            {
                Opening = opening;
                try
                {
                    _lintelMark = opening
                        .get_Parameter(SharedParams.PGS_MarkLintel)
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
                _dictLintelMarkByHashCode.AddOrUpdate(GetHashCode(), _lintelMark);
            }
            else
            {
                throw new ArgumentException(nameof(opening));
            }
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
        /// Марка перемычки проема внутри DTO
        /// </summary>
        public string LintelMark
        {
            get
            {
                return _lintelMark;
            }
            set
            {
                _lintelMark = value;
                _dictLintelMarkByHashCode[GetHashCode()] = _lintelMark;
            }
        }

        /// <summary>
        /// Толщина стены
        /// </summary>
        public double WallWidth
        {
            get
            {
                if (Opening.get_Parameter(SharedParams.ADSK_ThicknessOfWall) != null)
                {
                    return Double.Parse(Opening
                        .get_Parameter(SharedParams.ADSK_ThicknessOfWall)
                        .AsValueString());
                }
                else return 0;
            }
        }

        /// <summary>
        /// Ширина проема
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
        /// Масса перемычки
        /// </summary>
        public double MassOfLintel
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Количество видимых вложенных экземпляров 3D семейств в семействе перемычки
        /// </summary>
        public int LintelSubComponentsCount
        {
            get
            {
                return 0;
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
        /// Валидация семейства проема для создания DTO
        /// </summary>
        /// <param name="opening">Экземпляр семейства окна или двери.</param>
        /// <returns>True, если семейство окна или двери валидно для создания DTO, иначе False</returns>
        private bool ValidateInput(Document doc, FamilyInstance opening)
        {
            if (opening.Host == null)
            {
                return false;
            }
            var lintelId = opening.GetSubComponentIds()
                .Where(
                        id => (doc.GetElement(id) as FamilyInstance)
                        .Symbol
                        .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                        .AsValueString() == SharedValues.LintelDescription)
                .FirstOrDefault();

            if (lintelId == null || lintelId.IntegerValue <= 0)
            {
                return false;
            }

            BuiltInCategory hostCategory = (BuiltInCategory)opening.Host.Category.Id.IntegerValue;
            BuiltInCategory openingCategory = (BuiltInCategory)opening.Category.Id.IntegerValue;
            if (hostCategory == BuiltInCategory.OST_Walls
                && (openingCategory == BuiltInCategory.OST_Doors
                 || openingCategory == BuiltInCategory.OST_Windows)
                 && opening.get_Parameter(SharedParams.PGS_MarkLintel) != null
                 && opening.get_Parameter(SharedParams.Mrk_MarkOfConstruction) != null)
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
            return (WallWidth + Width).GetHashCode();
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
