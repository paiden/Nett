namespace Nett.PerfTests
{
    using System.IO;
    using NBench;

    public sealed class WriteTomlTablePerfTestsV1
    {
        private const string CounterName = "WriteTomlTableV1Counter";

        private static readonly TomlV1 tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
        private static readonly TomlTable sourceTable = TomlTable.From(TomlSettings.Create(), tml);

        private Counter counter;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            this.counter = context.GetCounter(CounterName);
        }

        [PerfBenchmark(
            Description = "Basic test to measure writing of a TomlTable structure to a stream.",
            NumberOfIterations = 3,
            RunMode = RunMode.Throughput,
            RunTimeMilliseconds = 1000,
            TestMode = TestMode.Measurement)]
        [CounterMeasurement(CounterName)]
        public void BenchmarkWriteTomlTable()
        {
            var memStream = new MemoryStream(1024);
            Toml.WriteStream(sourceTable, memStream);
            this.counter.Increment();
        }
    }
}
