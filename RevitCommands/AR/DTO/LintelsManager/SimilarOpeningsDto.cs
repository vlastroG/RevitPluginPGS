using Autodesk.Revit.DB;
using MS.GUI.ViewModels.Base;
using MS.RevitCommands.AR.Models;
using MS.RevitCommands.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.DTO.LintelsManager
{
    /// <summary>
    /// DTO для одинаковых по габаритам проемов
    /// </summary>
    public class SimilarOpeningsDto : OpeningDto
    {
        /// <summary>
        /// Список проемов с одинаковыми габаритными размерами
        /// </summary>
        public List<OpeningDto> Openings { get; }

        /// <summary>
        /// Количество проемов с одинаковыми размерами
        /// </summary>
        public int OpeningsCount => Openings.Count;

        /// <summary>
        /// Конструктор обертки списка проемов с одинаковыми габаритными размерами
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="width"></param>
        /// <param name="wallThick"></param>
        /// <param name="wallHeightOverOpening"></param>
        /// <param name="distanceToRightEnd"></param>
        /// <param name="distanceToLeftEnd"></param>
        /// <param name="wallMaterial"></param>
        /// <param name="level"></param>
        /// <param name="openings"></param>
        public SimilarOpeningsDto(
            Guid guid,
            double width,
            double wallThick,
            double wallHeightOverOpening,
            string wallMaterial,
            string level,
            string distanceConditionToLeftEnd,
            string distanceConditionToRightEnd,
            List<OpeningDto> openings,
            double distanceToRightEnd = 0,
            double distanceToLeftEnd = 0
            ) : base(guid,
                width,
                wallThick,
                wallHeightOverOpening,
                distanceToRightEnd,
                distanceToLeftEnd,
                wallMaterial,
                level)
        {
            Openings = openings;
            DistanceConditionToLeftEnd = distanceConditionToLeftEnd;
            DistanceConditionToRightEnd = distanceConditionToRightEnd;

            foreach (var opening in openings)
            {
                if ((opening.Width != Width)
                    || (opening.WallThick != WallThick)
                    || (opening.WallHeightOverOpening != WallHeightOverOpening)
                    || (opening.DistanceConditionToRightEnd != DistanceConditionToRightEnd)
                    || (opening.DistanceConditionToLeftEnd != DistanceConditionToLeftEnd)
                    || (opening.WallMaterial != WallMaterial)
                    || (opening.Level != Level))
                {
                    throw new ArgumentException($"Переданные в конструктор проемы не одинаковые!");
                }
            }
        }

        public override string DistanceConditionToLeftEnd { get; private protected set; }

        public override string DistanceConditionToRightEnd { get; private protected set; }

        public override string Mark
        {
            get => base.Mark;
            set
            {
                base.Mark = value;
                foreach (var opening in Openings)
                {
                    if (!opening.Mark.Equals(Mark))
                    {
                        opening.Mark = Mark;
                    }
                }
            }
        }

        public override Lintel Lintel
        {
            get => base.Lintel;
            set
            {
                base.Lintel = value;
                foreach (var opening in Openings)
                {
                    if ((opening.Lintel is null) || !opening.Lintel.Equals(Lintel))
                    {
                        opening.Lintel = Lintel;
                    }
                }
            }
        }
    }
}
