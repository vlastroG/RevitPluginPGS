using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    internal class ScheduleFinishWallsCeilingsRowDto
    {
        public string RoomNames { get; private set; }

        public string FintypeWallsCeilings { get; private set; }

        private List<(string Ceiling, double Area)> _ceilingTypeAreas = new List<(string Ceiling, double Area)>();

        private List<(string WallType, double Area)> _wallTypesAreas = new List<(string WallType, double Area)>();

        public IReadOnlyList<(string Ceiling, double Area)> CeilingTypeAreas { get { return _ceilingTypeAreas; } }

        public IReadOnlyList<(string WallType, double Area)> WallTypesAreas { get { return _wallTypesAreas; } }
    }
}
