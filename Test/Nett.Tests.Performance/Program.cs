
#pragma warning disable S125 // Sections of code should not be "commented out"


using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Nett.Tests.Performance.Benchmarks;

namespace Nett.Tests.Performance
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var cfg = ManualConfig.Create(DefaultConfig.Instance)
            //    .With(ExecutionValidator.DontFailOnError);
            ;

            BenchmarkRunner.Run<ReadTomlStringBenchmark>(cfg);
        }
    }
}
