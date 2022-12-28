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
            Guid guid = Guid.NewGuid();
            Console.WriteLine(guid);

            var test = "vgewr";
            Guid.TryParse(test, out guid);
            Console.WriteLine(guid);
        }
    }
}
