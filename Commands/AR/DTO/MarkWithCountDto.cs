using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS.Commands.AR.DTO
{
    public class MarkWithCountDto
    {
        public string Mark { get; private set; }

        public int MarkCount { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Mark);
            sb.Append(" (");
            sb.Append(MarkCount);
            sb.Append(")");
            return sb.ToString();
        }
    }
}
