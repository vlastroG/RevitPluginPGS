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
    public class OpeningDto
    {
        private static readonly Guid _parAdskMarkOfSymbol = Guid.Parse("2204049c-d557-4dfc-8d70-13f19715e46d");

        private static readonly Guid _parPgsLintelMark = Guid.Parse("aee96840-3b85-4cb6-a93e-85acee0be8c7");

        private static readonly Guid _parAdskWallWidth = Guid.Parse("9350e48f-842b-4c46-a15d-2e36ab1f352f");

        private string _pgsLintelMark;

        private string _openingMark;

        private static readonly Dictionary<int, string> _dictLintelMarkByHashCode = new Dictionary<int, string>();

        private static readonly Dictionary<int, string> _dictOpeningMarkByHashCode = new Dictionary<int, string>();


        public FamilyInstance Opening { get; private set; }

        public OpeningDto(FamilyInstance opening)
        {
            if (ValidateInput(opening))
            {
                Opening = opening;
                _pgsLintelMark = opening
                    .get_Parameter(_parPgsLintelMark)
                    .AsValueString();
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


        public string OpeningCategory
        {
            get { return Opening.Category.Name; }
        }

        public string Level
        {
            get
            {
                return Opening.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM)
                    .AsValueString();
            }
        }

        public string AdskMark
        {
            get
            {
                return Opening.Symbol
                    .get_Parameter(_parAdskMarkOfSymbol)
                    .AsValueString();
            }
        }

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

        public double AdskWallWidth
        {
            get
            {
                try
                {
                    return Double.Parse(Opening
                        .get_Parameter(_parAdskWallWidth)
                        .AsValueString());
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public double Width
        {
            get
            {
                return Double.Parse(Opening
                    .get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM)
                    .AsValueString());
            }
        }

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

        public static IReadOnlyDictionary<int, string> DictLintelMarkByHashCode
        {
            get
            {
                return _dictLintelMarkByHashCode;
            }
        }

        public static IReadOnlyDictionary<int, string> DictOpeningMarkByHashCode
        {
            get
            {
                return _dictOpeningMarkByHashCode;
            }
        }

        

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
                 || openingCategory == BuiltInCategory.OST_Windows))
            {
                return true;
            }
            else
                return false;
        }

        public int GetOpeningAndWallWidthHash()
        {
            return (AdskWallWidth + Width).GetHashCode();
        }


        public override int GetHashCode()
        {
            return (OpeningCategory + AdskWallWidth + Width).GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is OpeningDto))
                return false;
            else
                return GetHashCode().Equals(obj.GetHashCode());
        }
    }
}
