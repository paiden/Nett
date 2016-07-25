using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;

namespace Nett.Coma.Tests.Functional
{
    public sealed class MergeTests : TestsBase
    {
        private const string FuncSaveMergedConfig = "Save Merged Config";
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

        [FFact(FuncSaveMergedConfig, "When values defined in both sources, the values from the 'successor' source overwrite values of the predecessor source")]
        public void LoadMergedConfig_SucessorOverwritesPredecessorValues()
        {
            string f1 = "config1".TestRunUniqueName() + Toml.FileExtension;
            string f2 = "config2".TestRunUniqueName() + Toml.FileExtension;

            try
            {
                // Arrange
                const string Pre = @"
IntValue = 1
StringValue = 'pre'";
                const string Succ = @"
StringValue = 'succ'";

                File.WriteAllText(f1, Pre);
                File.WriteAllText(f2, Succ);

                // Act
                var c = ComaConfig.CreateMerged(() => new SingleLevelConfig(), f1, f2);

                // Assert
                c.Get(cfg => cfg.IntValue).Should().Be(1);
                c.Get(cfg => cfg.StringValue).Should().Be("succ");
            }
            finally
            {
                TryDeleteFile(f1);
                TryDeleteFile(f2);
            }
        }

        [FFact(FuncSaveMergedConfig, "When value was changed, it will be saved back into originator source")]
        public void SaveMergedConfig_ValueWillBeSavedBackToOriginatorSource()
        {
            string f1 = "config1".TestRunUniqueName() + Toml.FileExtension;
            string f2 = "config2".TestRunUniqueName() + Toml.FileExtension;

            try
            {
                // Arrange
                const string Pre = @"IntValue = 1";
                const string Succ = @"StringValue = 'originator'";
                const string NewStringVal = "NewStringValue";

                File.WriteAllText(f1, Pre);
                File.WriteAllText(f2, Succ);
                var c = ComaConfig.CreateMerged(() => new SingleLevelConfig(), f1, f2);

                // Act
                c.Set(cfg => cfg.StringValue = NewStringVal);

                // Assert
                var preTbl = Toml.ReadFile(f1);
                var succTbl = Toml.ReadFile(f2);

                preTbl.Rows.Count.Should().Be(1);
                preTbl.Get<int>("IntValue").Should().Be(1);

                succTbl.Rows.Count.Should().Be(1);
                succTbl.Get<string>("StringValue").Should().Be(NewStringVal);
            }
            finally
            {
                TryDeleteFile(f1);
                TryDeleteFile(f2);
            }
        }
    }
}
