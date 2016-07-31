using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Functional
{
    public sealed class MergeTests : TestsBase
    {
        private const string FuncLoadMergedConfig = "Load Merged Config";

        const string Config1 = "IntValue = 1";
        const string Config2 = "StringValue = 'test'";

        [FFact(FuncLoadMergedConfig, "When multiple sources used, merges those into one in process config object.")]
        public void LoadMergedConfig_MergesSourcesIntoOneInProcessConfig()
        {
            string f1 = "config1".TestRunUniqueName() + Toml.FileExtension;
            string f2 = "config2".TestRunUniqueName() + Toml.FileExtension;

            try
            {
                // Arrange
                File.WriteAllText(f1, Config1);
                File.WriteAllText(f2, Config2);

                // Act
                var c = ComaConfig.CreateMerged(() => new SingleLevelConfig(), f1, f2);

                // Assert
                c.Get(cfg => cfg.IntValue).Should().Be(1);
                c.Get(cfg => cfg.StringValue).Should().Be("test");
            }
            finally
            {
                TryDeleteFile(f1);
                TryDeleteFile(f2);
            }
        }
    }
}
