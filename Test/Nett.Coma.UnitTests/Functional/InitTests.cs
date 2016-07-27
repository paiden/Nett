using System.IO;
using FluentAssertions;
using Nett.UnitTests.Util;
using Xunit;

namespace Nett.Coma.Tests
{
    public sealed class CreateTests : TestsBase
    {
        private const string FuncInitMergeConfig = "Init merge config";

        [Fact(DisplayName = "If config file doesn't exist yet, the config manager will create one with the provided defaults.")]
        public void ConfigManager_WhenFileDoesntExistYet_WillCreateItInitially()
        {
            string fileName = "autocreated".TestRunUniqueName(Toml.FileExtension);

            try
            {
                // Act
                const int ExpectedIntValue = 3;
                var cfg = new SingleLevelConfig() { IntValue = ExpectedIntValue };
                ComaConfig.Create(fileName, () => cfg);

                // Assert
                File.Exists(fileName).Should().Be(true);
                var read = Toml.ReadFile<SingleLevelConfig>(fileName);
                read.IntValue.Should().Be(ExpectedIntValue);
            }
            finally
            {
                TryDeleteFile(fileName);
            }
        }

        [FFact(FuncInitMergeConfig, "Create a file split config from a single object creates the two files and the are merged correctly")]
        public void Foo()
        {
            string mainFile = "initMainSettings".TestRunUniqueName(Toml.FileExtension);
            string userFile = "initUserSettings".TestRunUniqueName(Toml.FileExtension);

            try
            {
                // Arrange
                var main = TestData.TestAppSettings.GlobalSettings;
                var userSettings = TestData.TestAppSettings.User1Settings;
                var user = Toml.Create();
                user.Add(nameof(main.User), userSettings);

                // Act
                Toml.WriteFile(main, mainFile);
                Toml.WriteFile(user, userFile);
                var merged = ComaConfig.CreateMerged(() => new TestData.TestAppSettings(), mainFile, userFile);

                // Assert
                merged.Get(c => c.BinDir).Should().Be(main.BinDir);
                merged.Get(c => c.User.UserName).Should().Be(userSettings.UserName);
            }
            finally
            {
                TryDeleteFile(mainFile);
                TryDeleteFile(userFile);
            }
        }
    }
}
