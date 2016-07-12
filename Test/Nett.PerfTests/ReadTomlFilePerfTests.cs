using NBench;

namespace Nett.PerfTests
{
    public class ReadTomlFilePerfTests
    {
        private Counter counter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            counter = context.GetCounter("TestCounter");
        }


        [PerfBenchmark(
            Description = "Basic test to measure untyped TOML reading throughput",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 5000,
            TestMode = TestMode.Test)]
        [CounterThroughputAssertion("TestCounter", MustBe.GreaterThan, 10000000.0d)]
        [MemoryAssertion(MemoryMetric.TotalBytesAllocated, MustBe.LessThanOrEqualTo, ByteConstants.ThirtyTwoKb)]
        [GcTotalAssertion(GcMetric.TotalCollections, GcGeneration.Gen2, MustBe.ExactlyEqualTo, 0.0d)]
        public void ReadUntypedTomlV1()
        {
            var toml = Toml.ReadString(TomlSource.TomlV1);
            this.counter.Increment();
        }
    }
}
