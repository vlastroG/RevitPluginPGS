using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    /// <summary>
    /// Сравнитель OpeningDto по свойствам: Level, MassOfLintel, Width, WallWidth, LintelSubComponentsCount.
    /// </summary>
    public class OpeningDtoComparerWithLevel : IEqualityComparer<OpeningDto>
    {
        /// <summary>
        /// Равенство OpeningDto при равенстве HashCode по сравнителю
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(OpeningDto x, OpeningDto y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        /// <summary>
        /// HashCode суммы свойств Level, MassOfLintel, Width, WallWidth, LintelSubComponentsCount.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(OpeningDto obj)
        {
            return (
                obj.Level +
                obj.MassOfLintel +
                obj.Width +
                obj.WallWidth +
                obj.LintelSubComponentsCount).GetHashCode();
        }
    }
}
