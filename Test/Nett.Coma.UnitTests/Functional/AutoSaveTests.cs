namespace Nett.Coma.Tests
{
    using System;
    using System.IO;
    using FluentAssertions;
    using TestData;
    using UnitTests.Util;
    using Xunit;

    public sealed class AutoSaveTests : TestsBase
    {
        private const string FuncSaveMergedConfig = "Save Merged Config";

        [Fact(DisplayName = "AutoSave: When value is changed in memory, new config file gets written to disk.")]
        public void WhenValueChangedInProcess_TheNewConfigGetsAutoSaved()
        {
            string filePath = nameof(this.WhenValueChangedInProcess_TheNewConfigGetsAutoSaved) + Guid.NewGuid() + Toml.FileExtension;

            try
            {
                // Arrange
                const int ExpectedNewValue = 1;
                var f = new SingleLevelConfig();
                Toml.WriteFile(f, filePath);
                var beforeChangeValue = f.IntValue;
                var cfg = ComaConfig.Create(filePath, () => new SingleLevelConfig());

                // Act
                cfg.Set(c => c.IntValue = ExpectedNewValue);

                // Assert
                var onDisk = Toml.ReadFile<SingleLevelConfig>(filePath);
                beforeChangeValue.Should().NotBe(onDisk.IntValue, "otherwise the file had the same value as after the change and we would test nothing here");
                onDisk.IntValue.Should().Be(ExpectedNewValue);
            }
            finally
            {
                TryDeleteFile(filePath);
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

        [FFact(FuncSaveMergedConfig, "When value in sub table is changed, that value gets saved back into the correct originator source.")]
        public void SaveMergedConfig_WhenValueInSubTableIsChanged_ValueGetsSavedBackToOriginatorFile()
        {
            string mainFile = null;
            string userFile = null;

            try
            {
                // Arrange
                const string Changed = "ChangedUserName";
                CreateMergedTestAppConfig(out mainFile, out userFile);
                var cfg = ComaConfig.CreateMerged(() => new TestData.TestAppSettings(), mainFile, userFile);

                // Act
                cfg.Set(c => c.User.UserName = Changed);

                // Assert
                var tml = Toml.ReadFile(userFile);
                ((TomlTable)tml[nameof(TestData.TestAppSettings.User)]).Get<string>(nameof(TestData.TestAppSettings.UserSettings.UserName))
                    .Should().Be(Changed);

            }
            finally
            {
                TryDeleteFile(mainFile);
                TryDeleteFile(userFile);
            }
        }

        [FFact(FuncSaveMergedConfig, "When saved value is help format, value gets saved back into system config file because it originates from there")]
        public void SaveMergeConfig_WhenHelpFormatInGitScenarioIsChanged_ValueGetSavedBackToSytemWideFileBecauseItOriginateFromThere()
        {
            using (var scenario = GitScenario.Setup(nameof(SaveMergeConfig_WhenHelpFormatInGitScenarioIsChanged_ValueGetSavedBackToSytemWideFileBecauseItOriginateFromThere)))
            {
                // Arrange
                var updated = GitScenario.GitConfig.HelpConfig.HelpFormat.Info;
                var cfg = scenario.CreateMergedFromDefaults();

                // Act
                cfg.Set(c => c.Core.Help.Format = updated);

                // Assert
                var tbl = Toml.ReadFile(scenario.SystemFile);
                tbl.Get<TomlTable>("Core")
                    .Get<TomlTable>("Help")
                    .Get<GitScenario.GitConfig.HelpConfig.HelpFormat>("Format").Should().Be(updated);
            }
        }
    }
}
