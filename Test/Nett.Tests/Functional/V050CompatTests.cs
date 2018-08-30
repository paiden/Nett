using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class V050CompatTests
    {
        [Fact]
        public void ReadToml_WithDottedKeys_CanReadThatStuff()
        {
            // Arrange
            const string TomlInput = @"
name = ""Orange""
physical.color = ""orange""
physical.shape = ""round""
site.""google.com"" = true
";
            // Act
            var read = Toml.ReadString(TomlInput);

            // Assert
            read.Get<string>("name").Should().Be("Orange");
            read.Get<TomlTable>("physical").Get<string>("color").Should().Be("orange");
            read.Get<TomlTable>("physical").Get<string>("shape").Should().Be("round");
            read.Get<TomlTable>("site").Get<bool>("google.com").Should().Be(true);
        }

        [Theory]
        [InlineData("x=12", 12)]
        [InlineData("x=0xAF", 0xAF)]
        [InlineData("x=0o77", 0x3F)]
        [InlineData("x=0b10", 0b10)]
        public void Read_CanReadAllIntTypes(string input, int expected)
        {
            // Act
            var read = Toml.ReadString(input);

            // Assert
            read.Get<int>("x").Should().Be(expected);
        }

        [Theory]
        [InlineData("x=12")]
        [InlineData("x=0xAF")]
        [InlineData("x=0o77")]
        [InlineData("x=0b10")]
        public void Write_WhenIntIsReadWithSomeType_ItAlsoGetsWrittenBackWithTheSameType(string tml)
        {
            // Arrange
            var read = Toml.ReadString(tml);

            // Act
            var written = Toml.WriteString(read);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo(tml);
        }
    }
}
