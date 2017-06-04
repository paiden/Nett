using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class ExternalChangeTests : TestsBase
    {
        [Fact(DisplayName = "External change: When file changed on disk the next property access will deliver that new value form the file.")]
        public void NextRead_WhenConfigValueChanged_DeliversNewValue()
        {
            string filePath = nameof(this.NextRead_WhenConfigValueChanged_DeliversNewValue) + Guid.NewGuid() + Toml.FileExtension;

            try
            {
                // Arrange
                const int ExpectedNewValue = 1;
                var m = Config.CreateAs()
                    .MappedToType(() => new SingleLevelConfig())
                    .StoredAs(store => store.File(filePath))
                    .Initialize();
                var afterInitialLoad = m.Get(cfg => cfg.IntValue);
                ModifyFileOnDisk(filePath, cfg => cfg.IntValue = ExpectedNewValue);

                // Act
                var nextReadValue = m.Get(cfg => cfg.IntValue);

                // Assert
                nextReadValue.Should().Be(ExpectedNewValue);
                afterInitialLoad.Should().NotBe(nextReadValue, "otherwise we would only test nothing at all changed");
            }
            finally
            {
                TryDeleteFile(filePath);
            }
        }
    }
}
