using System;
using System.Diagnostics.CodeAnalysis;
using Nett.Tests.Util;

namespace Nett.Coma.Tests.TestData
{
    [ExcludeFromCodeCoverage]
    internal sealed class SingleConfigFileScenario : IDisposable
    {
        public TestFileName File { get; }

        private SingleConfigFileScenario(string test)
        {
            this.File = TestFileName.Create("conffile", Toml.FileExtension, test);
        }

        public static SingleConfigFileScenario Setup(string test)
        {
            var scenario = new SingleConfigFileScenario(test);
            return scenario;
        }

        public Config<ConfigContent> CreateConfig()
        {
            return Config.CreateAs()
                .MappedToType(() => new ConfigContent())
                .StoredAs(store => store.File(this.File))
                .Initialize();
        }

        public ConfigContent ReadFile()
        {
            return Toml.ReadFile<ConfigContent>(this.File);
        }

        public void Dispose()
        {
            this.File.Dispose();
        }

        public class ConfigContent
        {
            public int X { get; set; } = 1;
            public string Y { get; set; } = "Y";

            public SubContent Sub { get; set; }
        }
        public class SubContent
        {
            public int Z { get; set; } = 1;
        }
    }
}
