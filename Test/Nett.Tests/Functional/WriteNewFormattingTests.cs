using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class WriteNewFormattingTests
    {
        [Fact]
        public void Write_WithPropertyOnly_WritesFirstPropertyOnFirstLineOfOutput()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.Add("x", 1);

            // Act
            var tml = Toml.WriteString(tbl);

            // Assert
            tml.TrimEnd().Should().Be("x = 1");
        }

        [Fact]
        public void Write_WithSubTable_WritesFirstPropertyOnFirstLineOfOutput()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.Add("s", new SubTable());

            // Act
            var tml = Toml.WriteString(tbl);

            // Assert
            tml.TrimEnd().Should().Be("\r\n[s]\r\nX = 2");
        }

        [Fact]
        public void Write_WritesOneFinalNewLine()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.Add("x", 1);

            // Act
            var tml = Toml.WriteString(tbl);

            // Assert
            tml.Should().Be("x = 1\r\n");
        }

        private class SubTable
        {
            public int X { get; set; } = 2;
        }
    }
}
