using Autodesk.Revit.DB;
using MS.Shared;
using MS.Utilites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

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
        /// True, если в вычисление HashCode включено значение свойства Level, иначе False
        /// </summary>
        public bool HashWithLevel { get; private set; }

        /// <summary>
        /// Словарь марок перемычек по хэш-коду DTO 
        /// </summary>
        private static readonly Dictionary<int, string> _dictLintelMarkByHashCode = new Dictionary<int, string>();

        /// <summary>
        /// Документ Revit, в котором размещено семейство проема
        /// </summary>
        public Document Doc { get; private set; }

        /// <summary>
        /// Экземпляр семейства категории окна или двери
        /// </summary>
        public FamilyInstance Opening { get; private set; }

        /// <summary>
        /// Экземпляр семейства перемычки, вложенный в семейство окна/двери
        /// </summary>
        public FamilyInstance Lintel { get; private set; }

        /// <summary>
        /// Конструктор Dto проема
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <param name="opening">Семейство Окна/Двери</param>
        /// <param name="hashWithLevel">Включать в хэш-код Уровень: Да/Нет</param>
        /// <exception cref="ArgumentNullException">В семействе <paramref name="opening"/> отсутствует параметр PGS_МаркаПеремычки</exception>
        /// <exception cref="ArgumentException">Нельзя создать из семейства <paramref name="opening"/> Dto.</exception>
        public OpeningDto(Document doc, FamilyInstance opening, bool hashWithLevel)
        {
            if (ValidateInput(doc, opening))
            {
                Opening = opening;
                Doc = doc;
                var lintelId = Opening.GetSubComponentIds()
                                .FirstOrDefault(
                                                id => (doc.GetElement(id) as FamilyInstance).Symbol
                                                .get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION)
                                                .AsValueString() == SharedValues.LintelDescription);
                Lintel = doc.GetElement(lintelId) as FamilyInstance;
                HashWithLevel = hashWithLevel;
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
                if (Lintel.get_Parameter(SharedParams.ADSK_MassElement) != null)
                {
                    return Lintel.get_Parameter(SharedParams.ADSK_MassElement).AsDouble();
                }
                else
                {
                    MessageBox.Show(
                        $"У перемычки с Id = {Lintel.Id} отсутствует общий параметр \"ADSK_Масса элемента\"",
                        "Предупреждение");
                    return 0;
                }
            }
        }

        /// <summary>
        /// Количество видимых вложенных экземпляров 3D семейств в семействе перемычки
        /// </summary>
        public int LintelSubComponentsCount
        {
            get
            {
                return Lintel.GetSubComponentIds().Count;
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

        private string GetSubComponentsADSKNames()
        {
            var subs = Lintel.GetSubComponentIds().Select(sc => Doc.GetElement(sc)).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var sub in subs)
            {
                var adskName = sub.get_Parameter(SharedParams.ADSK_Name);
                if (adskName != null)
                {
                    sb.Append(adskName.AsValueString());
                }
            }
            return sb.ToString();
        }



        /// <summary>
        /// Если HashWithLevel == True, то возвращается хэш код суммы:
        /// Level + MassOfLintel + Width + WallWidth + LintelSubComponentsCount
        /// Иначе:
        /// MassOfLintel + Width + WallWidth + LintelSubComponentsCount
        /// </summary>
        /// <returns>Хэш-код суммы свойств.</returns>
        public override int GetHashCode()
        {
            if (HashWithLevel)
            {
                return (Level + MassOfLintel + Width + WallWidth + LintelSubComponentsCount + GetSubComponentsADSKNames()).GetHashCode();
            }
            else
            {
                return (MassOfLintel + Width + WallWidth + LintelSubComponentsCount + GetSubComponentsADSKNames()).GetHashCode();
            }
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
