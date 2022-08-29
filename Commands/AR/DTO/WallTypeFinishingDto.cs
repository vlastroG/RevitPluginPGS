using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    public class WallTypeFinishingDto
    {
        public string Description { get; }
        public string WallType { get; set; }

        public WallTypeFinishingDto(string description)
        {
            Description = description;
        }
    }
}
