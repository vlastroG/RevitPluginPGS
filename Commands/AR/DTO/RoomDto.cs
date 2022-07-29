using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    public class RoomDto
    {
        public Room RoomRevit
        {
            get;
            private set;
        }


        public RoomDto(Room room)
        {
            RoomRevit = room;
            DoOpeningsAreaCalculation = true;
        }

        public bool DoOpeningsAreaCalculation { get; set; }

        public string Number
        {
            get
            {
                if (RoomRevit != null && RoomRevit.IsValidObject)
                    return RoomRevit.Number;
                else
                    return String.Empty;
            }
        }

        public string Name
        {
            get
            {
                if (RoomRevit != null && RoomRevit.IsValidObject)
                    return RoomRevit.Name;
                else
                    return String.Empty;
            }

        }

        public string Level
        {
            get
            {
                if (RoomRevit != null && RoomRevit.IsValidObject)
                    return RoomRevit.Level.Name;
                else
                    return String.Empty;
            }
        }

        public string Comment
        {
            get
            {
                if (RoomRevit != null && RoomRevit.IsValidObject)
                    return RoomRevit
                        .get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                        .AsValueString();
                else
                    return String.Empty;
            }
        }


        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RoomDto))
            {
                return false;
            }
            else
            {
                RoomDto other = (RoomDto)obj;
                return EqualsToRoomDto(other);
            }
        }

        public override int GetHashCode()
        {
            return (Number + Name + Level).GetHashCode();
        }

        private bool EqualsToRoomDto(RoomDto rDtoOther)
        {
            return rDtoOther.RoomRevit.IsValidObject
                && Number == rDtoOther.Number
                && Name == rDtoOther.Name
                && Level == rDtoOther.Level;
        }
    }
}
