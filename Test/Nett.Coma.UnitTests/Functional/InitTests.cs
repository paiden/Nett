    using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests
{
    public sealed class CreateTests : TestsBase
    {
        [Fact(DisplayName = "If config file doesn't exist yet, the config manager will create one with the provided defaults.")]
        public void ConfigManager_WhenFileDoesntExistYet_WillCreateItInitially()
        {
            string fileName = Guid.NewGuid() + Toml.FileExtension;

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
    }
}
