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
    }
}
