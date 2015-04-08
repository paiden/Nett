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
    }
}
