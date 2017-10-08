using BenchmarkDotNet.Running;

namespace Nett.PerfTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // InProcessDebug();

            var summary = BenchmarkRunner.Run<Benchmarks>();
        }

        private static void InProcessDebug()
        {
            var b = new Benchmarks();
            b.Baseline();
        }
    }
}
