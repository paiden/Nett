namespace Nett.Coma.Tests
{
    using System;
    using FluentAssertions;
    using Xunit;

    public sealed class AutoSaveTests : TestsBase
    {
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
    }
}
