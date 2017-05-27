using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Nett.Coma.Tests.TestData;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Coma.Tests
{
    [ExcludeFromCodeCoverage]
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
                Config.CreateAs()
                    .MappedToType(() => cfg)
                    .StoredAs(store => store.File(fileName))
                    .Initialize();

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
            string mainFile = null;
            string userFile = null;

            try
            {
                // Arrange
                CreateMergedTestAppConfig(out mainFile, out userFile);

                // Act
                var merged = Config.CreateAs()
                    .MappedToType(() => new TestData.TestAppSettings())
                    .StoredAs(store => store
                        .File(mainFile)
                        .MergeWith().File(userFile))
                    .Initialize();

                // Assert
                merged.Get(c => c.BinDir).Should().Be(TestData.TestAppSettings.GlobalSettings.BinDir);
                merged.Get(c => c.User.UserName).Should().Be(TestData.TestAppSettings.User1Settings.UserName);
            }
            finally
            {
                TryDeleteFile(mainFile);
                TryDeleteFile(userFile);
            }
        }

        [Fact]
        public void SaveSetting_WhenItDoesNotExistYetInConfigFile_GetsCreatedAndSaved()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(SaveSetting_WhenItDoesNotExistYetInConfigFile_GetsCreatedAndSaved)))
            {
                // Arrange
                var cfg = Config.CreateAs()
                    .MappedToType(() => new SingleConfigFileScenario.ConfigContent())
                    .StoredAs(store => store.File(scenario.File))
                    .Initialize();

                // Act
                cfg.Set(c => c.Sub.Z, 1);

                // Assert
                File.ReadAllText(scenario.File).Should().Be("X = 1\r\nY = \"Y\"\r\n\r\n[Sub]\r\nZ = 1\r\n");
            }
        }

        [Fact]
        public void SaveSetting_WhenItDoesNotExistYetInConfigFileAndTargetExplicitelySpecified_GetsCreatedAndSaved()
        {
            using (var scenario = SingleConfigFileScenario.Setup(nameof(SaveSetting_WhenItDoesNotExistYetInConfigFileAndTargetExplicitelySpecified_GetsCreatedAndSaved)))
            {
                // Arrange
                IConfigSource src = null;
                var cfg = Config.CreateAs()
                    .MappedToType(() => new SingleConfigFileScenario.ConfigContent())
                    .StoredAs(store => store.File(scenario.File).AsSource(s => src = s))
                    .Initialize();

                // Act
                cfg.Set(c => c.Sub.Z, 1, src);

                // Assert
                File.ReadAllText(scenario.File).Should().Be("X = 1\r\nY = \"Y\"\r\n\r\n[Sub]\r\nZ = 1\r\n");
            }
        }

        [Fact]
        public void SaveSetting_WhenMovedBetweenConfigScopes_SavesThatSettingCorrectly()
        {
            using (var scenario = GitScenario.Setup(nameof(SaveSetting_WhenMovedBetweenConfigScopes_SavesThatSettingCorrectly)))
            {
                // Arrange
                var cfg = scenario.CreateMergedFromDefaults();

                // Act
                cfg.Set(c => c.Core.AutoClrf, true, scenario.UserFileSource);

                // Assert
                File.ReadAllText(scenario.UserFile).Should().Be("\r\n[User]\r\nName = \"Test User\"\r\nEMail = \"test@user.com\"\r\n\r\n[Core]\r\nAutoClrf = true\r\n");
            }
        }
    }
}
