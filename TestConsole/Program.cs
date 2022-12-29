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
            var t = Enumerable.Range(0, 10).Select(i => (i, i * i)).ToDictionary(tuple => tuple.i, tuple => tuple.Item2);




            Console.WriteLine(t.ToString());
            //.ToList()
            //.ForEach(t => Console.WriteLine($"Key: {t.Key},\tValue: {t.Value}"));
        }


    }
}
