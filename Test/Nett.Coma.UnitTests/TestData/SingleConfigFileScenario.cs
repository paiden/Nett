using System;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.TestData
{
    internal sealed class SingleConfigFileScenario : IDisposable
    {
        public TestFileName File { get; }

        private SingleConfigFileScenario(string test)
        {
            this.File = TestFileName.Create(test, "conffile", Toml.FileExtension);
        }

        public static SingleConfigFileScenario Setup(string test)
        {
            var scenario = new SingleConfigFileScenario(test);
            return scenario;
        }

        public Config<ConfigContent> CreateConfig()
        {
            return Config.Create(() => new ConfigContent(), this.File);
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
        }
    }
}
