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
ArrayProperty = [10, 20, 30]
";

            // Act
            var sc = Toml.ReadString<SingleClass>(toml);

            // Assert
            Assert.Equal("Hello world", sc.StringProperty);
            Assert.Equal(100, sc.IntProperty);
            Assert.Equal(2.5, sc.DoubleProperty);
            Assert.NotNull(sc.ArrayProperty);
            Assert.Equal(3, sc.ArrayProperty.Length);
            Assert.Equal(10, sc.ArrayProperty[0]);
            Assert.Equal(20, sc.ArrayProperty[1]);
            Assert.Equal(30, sc.ArrayProperty[2]);
        }
    }

    public class SingleClass
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }

        public double DoubleProperty { get; set; }

        public int[] ArrayProperty { get; set; }
    }
}
