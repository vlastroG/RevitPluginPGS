using MS.Utilites.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.RevitCommands.AR.DTO
{
    internal class ScheduleFinishWallsCeilingsRowDto
    {
        /// <summary>
        /// Названия помещений с одинаковым типом отделки
        /// </summary>
        public string RoomNames { get; private set; }

        /// <summary>
        /// Тип отделки стен и потолка
        /// </summary>
        public string FintypeWallsCeilings { get; private set; }

        /// <summary>
        /// Список кортежей названия отделки потолка и ее площади
        /// </summary>
        private List<(string Ceiling, double Area)> _ceilingTypeAreas = new List<(string Ceiling, double Area)>();

        /// <summary>
        /// Список кортежей названия отделки стен и ее площади
        /// </summary>
        private List<(string WallType, double Area)> _wallTypesAreas = new List<(string WallType, double Area)>();

        /// <summary>
        /// Список кортежей названия отделки потолка и ее площади
        /// </summary>
        public IReadOnlyList<(string Ceiling, double Area)> CeilingTypeAreas { get { return _ceilingTypeAreas; } }

        /// <summary>
        /// Список кортежей названия отделки стен и ее площади
        /// </summary>
        public IReadOnlyList<(string WallType, double Area)> WallTypesAreas { get { return _wallTypesAreas; } }

        public ScheduleFinishWallsCeilingsRowDto(string Fintype, string RoomNames)
        {
            this.RoomNames = RoomNames;
            this.FintypeWallsCeilings = Fintype;
        }

        /// <summary>
        /// Добавляет тип отделки стен с площадью, если его еще нет в списке, или прибавляет площадь у существующего
        /// </summary>
        /// <param name="WallType">Описание отделочной стены, которое должно писаться в ведомость отделки</param>
        /// <param name="Area">Площадь этого типа отделки</param>
        public void AddWallFinType((string WallType, double Area) walltypeArea)
        {
            _wallTypesAreas.AddOrUpdate(walltypeArea);
        }

        /// <summary>
        /// Добавляет тип отделки потолка с площадью, если его еще нет в списке, или прибавляет площадь у существующего
        /// </summary>
        /// <param name="Ceiling">Описание отделки потолка, которое должно писаться в ведомость отделки</param>
        /// <param name="Area">Площадь отделки потолка</param>
        public void AddCeilingFinType((string Ceiling, double Area) ceilingArea)
        {
            _ceilingTypeAreas.AddOrUpdate(ceilingArea);
        }
    }
}