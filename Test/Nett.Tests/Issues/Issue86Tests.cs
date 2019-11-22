using FluentAssertions;
using Xunit;

namespace Nett.Tests.Issues
{
    public class Issue86Tests
    {

        [Fact]
        public void VerifyIssue86_IsFixed()
        {
            // Arrange
            const string input = @"
[app]
value1 = ""string""
obj.a = 1
obj.b = 2
";

            var tml = Toml.ReadString(input);

            // Act
            var written = Toml.WriteString(tml);

            // Assert
            var readBack = Toml.ReadString(written);
            readBack.Get<TomlTable>("app").Get<TomlTable>("obj").Get<int>("b").Should().Be(2);
        }
    }
}
