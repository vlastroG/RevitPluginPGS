using BenchmarkDotNet.Attributes;

namespace TestBenchmark
{

    [MemoryDiagnoser]
    public class Test
    {
        private int[] __values = new int[10000];

        [Benchmark(Baseline = true)]
        public int For()
        {
            int sum = 0;
            for (int i = 0; i < __values.Length; i++)
            {
                sum += __values[i];
            }
            return sum;
        }

        [Benchmark]
        public int Foreach()
        {
            int sum = 0;
            foreach (var item in __values)
            {
                sum += item;
            }
            return sum;
        }

        [Benchmark]
        public int While()
        {
            int i = 0;
            int sum = 0;
            while (i < __values.Length)
            {
                sum += __values[i++];
            }
            return sum;
        }
    }
}