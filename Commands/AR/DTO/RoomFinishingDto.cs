using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    internal class RoomFinishingDto
    {
        private List<(string Ceiling, double Area)> _ceilingsAreas = new List<(string Ceiling, double Area)>();

        private List<(string Floor, double Area)> _floorsAreas = new List<(string Floor, double Area)>();

        private List<(string WallType, double Area)> _wallsAreas = new List<(string WallType, double Area)>();


        public string RoomName { get; private set; }

        public string RoomNumber { get; private set; }

        public string FintypeWallsCeilings { get; private set; }

        public string FintypeFloors { get; private set; }

        public IReadOnlyCollection<(string Ceiling, double Area)> CeilingFinishing
        {
            get { return _ceilingsAreas; }
        }

        public IReadOnlyCollection<(string Floor, double Area)> FloorFinishing
        {
            get { return _floorsAreas; }
        }

        public IReadOnlyCollection<(string WallType, double Area)> WallFinishing
        {
            get { return _wallsAreas; }
        }
    }
}
