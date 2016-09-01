using System;
using FluentAssertions;
using Xunit;

namespace Nett.Coma.Tests
{
    public sealed class ExternalChangeTests : TestsBase
    {
        [Fact(DisplayName = "External change: When file changed on disk the next property access will deliver that new value form the file.")]
        public void NextRead_WhenConcfigValueChanged_DelversNewValue()
        {
            string filePath = nameof(this.NextRead_WhenConcfigValueChanged_DelversNewValue) + Guid.NewGuid() + Toml.FileExtension;

            try
            {
                // Arrange
                const int ExpectedNewValue = 1;
                var m = Config.Create(filePath, () => new SingleLevelConfig());
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
