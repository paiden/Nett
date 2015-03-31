using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class ClassReaderTests
    {
        [Fact]
        public void ReadSingleClass_ReadClassCorrectly()
        {
            // Arrange
            var toml = @"
StringProperty = ""Hello world""
IntProperty = 100
DoubleProperty = 2.5
";

            // Act
            var sc = Toml.Read<SingleClass>(toml);

            // Assert
            Assert.Equal("Hello world", sc.StringProperty);
            Assert.Equal(100, sc.IntProperty);
            Assert.Equal(2.5, sc.DoubleProperty);
        }
    }

    public class SingleClass
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }

        public double DoubleProperty { get; set; }
    }
}
