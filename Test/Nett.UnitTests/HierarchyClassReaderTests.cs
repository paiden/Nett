using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class HierarchyClassReaderTests
    {
        [Fact]
        public void ReadHierarchialClass_ReadsCorrectly()
        {
            // Arrange
            var toml = @"
OuterProp = ""Hello World""
    [A]
    InnerProp = 200";

            // Act
            var parsed = Toml.ReadString<Outer>(toml);

            // Assert
            Assert.Equal("Hello World", parsed.OuterProp);
            Assert.NotNull(parsed.A);
            Assert.Equal(200, parsed.A.InnerProp);
        }

        [Fact]
        public void ReadComplexHierarchialClass_ReadCorrectly()
        {
            // Arrange
            var toml = @"
    OuterProp = ""Hello World""
    [A]
    InnerProp = 300
    [B]
    InnerProp = 400
";

            // Act
            var parsed = Toml.ReadString<Outer>(toml);

            // Assert
            Assert.NotNull(parsed.A);
            Assert.NotNull(parsed.B);
            Assert.Equal(300, parsed.A.InnerProp);
            Assert.Equal(400, parsed.B.InnerProp);

        }

        public class Inner
        {
            public int InnerProp { get; set; }
        }
        public class Outer
        {
            public string OuterProp { get; set; }
            public Inner A { get; set; }
            public Inner B { get; set; }
        }
    }
}
