namespace Nett.PerfTests
{
    using NBench;

    public sealed class ClassToTomlTablePerfTestsV1
    {
        private const string CounterName = "ClrToTomlTable";
        private static readonly TomlV1 tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
        private static readonly TomlTable sourceTable = TomlTable.From(TomlConfig.Create(), tml);

        private Counter counter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            this.counter = context.GetCounter(CounterName);
        }

        [PerfBenchmark(
            Description = "Basic test to measure conversion of .Net structure to TOML table.",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(CounterName)]
        public void BenchmarkClrToTomlTable()
        {
            var table = TomlTable.From(TomlConfig.Create(), tml);
            this.counter.Increment();
        }
    }
}
