using FluentAssertions;
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
    }
}
