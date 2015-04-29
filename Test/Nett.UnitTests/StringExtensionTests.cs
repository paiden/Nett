using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData(@"C:\\test.txt", @"C:\test.txt")]
        [InlineData(@"\u4000", "\u4000")]
        [InlineData(@"\uAAAAAAAA", "\uAAAAAAAA")]
        public void Unescape(string src, string expected)
        {
            // Act
            var s = src.Unescape();

            // Assert
            Assert.Equal(expected, s);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(@"C:\Windows\System32\BestPractices\v1.0\Models\Microsoft\Windows\Hyper-V\en-US\test.txt", @"C:\\Windows\\System32\\BestPractices\\v1.0\\Models\\Microsoft\\Windows\\Hyper-V\\en-US\\test.txt" )]
        public void Escape(string src, string expected)
        {
            // Act
            var escaped = src.Escape();

            // Assert
            Assert.Equal(expected, escaped);
        }
    }
}
