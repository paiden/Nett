using System.IO;
using BenchmarkDotNet.Attributes;

namespace Nett.PerfTests
{
    public class Benchmarks
    {
        private static readonly TomlV1 tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
        private static readonly TomlTable sourceTable = TomlTable.From(TomlSettings.Create(), tml);

        [Benchmark(Baseline = true)]
        public object Baseline()
        {
            var p = new BaselineParser();
            return p.Parse<TomlV1>(TomlSource.JsonV1);
        }

        [Benchmark]
        public object ReadV1DataStructure()
        {
            var tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
            return tml;
        }

        [Benchmark]
        public object ReadV1DataStructureFromFile()
        {
            var tml = Toml.ReadString<TomlV1>(TomlSource.TomlV1);
            return tml;
        }

        [Benchmark]
        public object ReadV1FileIntoTomlTable()
        {
            var toml = Toml.ReadString(TomlSource.TomlV1);
            return toml;
        }

        [Benchmark]
        public object ClrToTomlTable()
        {
            var table = TomlTable.From(TomlSettings.Create(), tml);
            return table;
        }

        [Benchmark]
        public object WriteTableToStream()
        {
            var memStream = new MemoryStream(1024);
            Toml.WriteStream(sourceTable, memStream);
            return memStream;
        }
    }
}
