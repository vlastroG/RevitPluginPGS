using BenchmarkDotNet.Attributes;

namespace TestBenchmark
{

    [MemoryDiagnoser]
    [WarmupCount(1)]
    [IterationCount(5)]
    public class Test
    {
        private int[] __values = new int[10000];

        [Benchmark(Baseline = true)]
        public Dictionary<int, int> For()
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            for (int i = 0; i < 100000; i++)
            {
                result.Add(i, i * i);
            }
            return result;
        }

        //[Benchmark]
        //public int Foreach()
        //{
        //    int sum = 0;
        //    foreach (var item in __values)
        //    {
        //        sum += item;
        //    }
        //    return sum;
        //}

        //[Benchmark]
        //public int While()
        //{
        //    int i = 0;
        //    int sum = 0;
        //    while (i < __values.Length)
        //    {
        //        sum += __values[i++];
        //    }
        //    return sum;
        //}
    }
}