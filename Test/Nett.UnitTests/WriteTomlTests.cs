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
        public void Write_WithArray_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithArray();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("EmptyArray = []\r\nNonEmptyIntArray = [-100, 0, 100]\r\nStringList = [\"A\", \"B\", \"C\"]\r\n", s);
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

        [Fact]
        public void Write_WithClassProperty_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithClassProperty()
            {
                //StringProp = "sp",
                //IntProp = 10,
                //ClassProp = new ClassProperty()
                //{
                //    StringProp = "isp",
                //    IntProp = 100,
                //    IntList = new List<int>() { 1, 2 },
                //    StringArray = new string[] { "A", "B" },
                //    ConvProp = new ConvProp() { Prop = "cp" },
                //},
                Acp = new List<ArrayClassProp>() { new ArrayClassProp() { V = 666 } },
                
            };

            var cfg = TomlConfig.Default().AddConversion().From<ConvProp>().To<TomlString>().As((cp) => new TomlString(cp.Prop));

            // Act
            var s = Toml.WriteString(tc, cfg);

            // Assert
            Assert.Equal(@"IntProp = 10
StringProp = ""sp""

[ClassProp]
StringProp = ""isp""
IntProp = 100
IntList = [1, 2]
StringArray = [A, B]
ConvProp = ""cp""

[[Acp]]
V = 666

", s);
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

        private class WithClassProperty
        {
            //public int IntProp {get; set;}
            //public ClassProperty ClassProp { get; set; }
            //public string StringProp { get; set; }

            public List<ArrayClassProp> Acp { get; set; }
        }

        private class ClassProperty
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }

            public List<int> IntList { get; set; }

            public string[] StringArray { get; set; }

            public ConvProp ConvProp { get; set; }
        }

        public class ConvProp
        {
            public string Prop { get; set; }
        }

        public class ArrayClassProp
        {
            public int V { get; set; }
        }
    }
}
