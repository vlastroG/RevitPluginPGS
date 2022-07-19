using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.Models
{
    public class RoomDto
    {
        private Room _room;


        public RoomDto(Room room)
        {
            _room = room;
        }

        public bool DoWallAreaCalculation = true;

        public string Number
        {
            get
            {
                return _room.Number;
            }
        }

        public string Name
        {
            get { return _room.Name; }

        }

        public string Level
        {
            get { return _room.Level.Name; }
        }
    }
}
