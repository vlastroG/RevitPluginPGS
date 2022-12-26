using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<string> list = new List<string>() { "12e", "abba", "AbBa", "vdsg", "12E", "VSsg", "vDsg"};
            var t = list.GroupBy(x => x.Length + x.ToLower());
        }
    }
}
