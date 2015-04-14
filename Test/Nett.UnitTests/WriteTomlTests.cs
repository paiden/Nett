using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class WriteTomlTests
    {
        [Fact]
        public void WriteObjectTests()
        {
            // Arrange
            var tc = new TestClassA();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("StringProp = \"\"\r\nIntProp = 0\r\n", s);
        }

        [Fact]
        public void Write_WithArray_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithArray();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("EmptyArray = []\r\nNonEmptyIntArray = [-100, 0, 100]\r\nNullArray = []\r\nStringList = [\"A\", \"B\", \"C\"]\r\n", s);
        }

        [Fact]
        public void Write_WithComment_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithDefaultComment();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("# This is a comment\r\nCommented = 0\r\n", s);
        }

        [Fact]
        public void Write_WithAppendComment_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithAppendComment();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("Commented = 0 # This is a comment\r\n", s);
        }

        private class TestClassA
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
        }

        private class WithArray
        {
            public int[] EmptyArray => new int[0];
            public int[] NonEmptyIntArray => new int[] { -100, 0, 100 };
            public TimeSpan[] NullArray => null;

            public List<string> StringList => new List<string>() {  "A", "B", "C" };
        }

        private class WithDefaultComment
        {
            [TomlComment("This is a comment")]
            public int Commented { get; set; }
        }

        private class WithAppendComment
        {
            [TomlComment("This is a comment", CommentLocation.Append)]
            public int Commented { get; set; }
        }
    }
}
