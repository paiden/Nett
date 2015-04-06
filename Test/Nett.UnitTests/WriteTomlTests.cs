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
            Assert.Equal(s, "StringProp = \"\"\r\nIntProp = 0\r\n");
        }

        private class TestClassA
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
        }
    }
}
