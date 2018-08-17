using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;

namespace Nett.Tests.Performance.Benchmarks
{
    [ShortRunJob()]
    [MarkdownExporter]
    public class ReadTomlStringBenchmark
    {
        private string InputToml;
        private string InputJson;

        [GlobalSetup]
        public void Setup()
        {
            var input = new InputData();

            this.InputToml = Toml.WriteString(input);
            this.InputJson = JsonConvert.SerializeObject(input);
        }

        [Benchmark(Baseline = true)]
        public object ReadJson()
        {
            return JsonConvert.DeserializeObject(this.InputJson);
        }

        [Benchmark]
        public object ReadToml()
        {
            return Toml.ReadString(this.InputToml);
        }


        public class InputData
        {
            public long A { get; set; } = 1;

            public double B { get; set; } = 2;

            public List<long> C { get; set; } = new List<long>() { 1, 2, 3 };

            public string D { get; set; } = "Hello this is a string";

            public Inner I { get; set; } = new Inner();

            public List<Inner> ThisIsAListOfInner { get; set; } = new List<Inner>() { new Inner(), new Inner(), new Inner() };

            public List<Name> Names { get; set; } = new List<Name>()
            {
                new Name("Peter", "Luxer"),
                new Name("Sergio", "Combucha"),
                new Name("Salasar", "Smith"),
            };
        }

        [TreatAsInlineTable]
        public class Inner
        {
            public int[][] A { get; set; } = new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 } };
        }

        public class Name
        {
            public string ForeName { get; set; }

            public string SurName { get; set; }

            public Name(string f, string s)
            {
                this.ForeName = f;
                this.SurName = s;
            }
        }
    }
}
