using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(@"C:\\test.txt", @"C:\test.txt")]
        [InlineData(@"\u4000", "\u4000")]
        [InlineData(@"\U0010FFFF", "\U0010FFFF")]
        public void Unescape(string src, string expected)
        {
            // Act
            var s = src.Unescape(new Nett.Parser.Token());

            // Assert
            Assert.Equal(expected, s);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(@"C:\Windows\System32\BestPractices\v1.0\Models\Microsoft\Windows\Hyper-V\en-US\test.txt", @"C:\\Windows\\System32\\BestPractices\\v1.0\\Models\\Microsoft\\Windows\\Hyper-V\\en-US\\test.txt")]
        public void Escape(string src, string expected)
        {
            // Act
            var escaped = src.Escape();

            // Assert
            Assert.Equal(expected, escaped);
        }
    }
}
