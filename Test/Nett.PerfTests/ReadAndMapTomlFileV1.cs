using NBench;

namespace Nett.PerfTests
{
    public class ReadAndMapTomlFileV1
    {
        private Counter counter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            counter = context.GetCounter("TestCounter");
        }

        [PerfBenchmark(
            Description = "Basic test to measure parsing and mapping of TOML files",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement("TestCounter")]
        public void Benchmark()
        {
            var tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
            this.counter.Increment();
        }
    }
}
