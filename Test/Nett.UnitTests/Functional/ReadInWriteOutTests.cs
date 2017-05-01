using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    /// <summary>
    /// Tests that show, that data is written back correctly after a read of a file.
    /// </summary>
    public sealed class ReadInNWriteOutTests
    {
        [Theory]
        [InlineData("\"a.value\" = 1")]
        [InlineData("'a.value\\' = 1")]
        public void ReadInNWriteOut_WithNonBareKey_WritesBackWithSameKeyType(string input)
        {
            var written = Toml.WriteString(Toml.ReadString(input));

            written.TrimEnd().Should().Be(input);
        }
    }
}
