using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class WriteTomlTests
    {
        [Fact(DisplayName = "Ensure writing floats works culture invariant")]
        public void Write_WithFloat_WritesCultureInvariant()
        {
            // Arrange
            var tt = Toml.Create();
            tt.Add("TheFloat", 1.2);

            // Act
            var s = Toml.WriteString(tt);
            Toml.WriteFile(tt, "test123.toml");

            // Assert
            s.Should().Be("TheFloat = 1.2\r\n");
        }

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
            Assert.Equal("#This is a comment\r\nCommented = 0\r\n", s);
        }

        [Fact]
        public void Write_WithMultilineComment_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithMultilineComment();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            s.Should().Be("#This is a\r\n# multiline comment\r\nCommentedA = 0\r\n#This is a \n#multiline comment\r\nCommentedB = 0\r\n");
        }

        [Fact]
        public void Write_WithAppendComment_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithAppendComment();

            // Act
            var s = Toml.WriteString(tc);

            // Assert
            Assert.Equal("Commented = 0 #This is a comment\r\n", s);
        }

        [Fact]
        public void Write_WithClassProperty_WritesObjectCorrectly()
        {
            // Arrange
            var tc = new WithClassProperty()
            {
                StringProp = "sp",
                IntProp = 10,
                ClassProp = new ClassProperty()
                {
                    StringProp = "isp",
                    IntProp = 100,
                    IntList = new List<int>() { 1, 2 },
                    StringArray = new string[] { "A", "B" },
                    ConvProp = new ConvProp() { Prop = "cp" },
                },
                Acp = new List<ArrayClassProp>() { new ArrayClassProp() { V = 666 } },
            };

            var cfg = TomlSettings.Create(config => config
                .ConfigureType<ConvProp>(ct => ct
                    .WithConversionFor<TomlString>(conv => conv
                        .ToToml(cp => cp.Prop))));

            // Act
            var exp = @"IntProp = 10
StringProp = ""sp""

[ClassProp]
StringProp = ""isp""
IntProp = 100
IntList = [1, 2]
StringArray = [""A"", ""B""]
ConvProp = ""cp""

[[Acp]]
V = 666
";
            var s = Toml.WriteString(tc, cfg);

            // Assert
            Assert.Equal(exp.Trim(), s.Trim());
        }

        [Fact]
        public void WriteClassWithBoolCreatesCorrecToml()
        {
            var s = Toml.WriteString(new WithBoolProperty());

            s.Should().Be(
                @"TheInt = 0
TheBool = false
");
        }

        [Fact]
        public void Write_WritesTableArraysAfterProperties()
        {
            var s = Toml.WriteString(new TestOrder());

            s.Should().StartWith("Property");
        }

        [Fact]
        public void Write_WhenMarkedAsInlineTableInConfig_WritesTableAsInlineTable()
        {
            var cfg = TomlSettings.Create(config => config
                .ConfigureType<TestClassA>(ct =>
                    ct.TreatAsInlineTable()
                )
            );

            var s = Toml.WriteString(new TestClassA(), cfg);

            s.Should().Be("IntProp = 0\r\n");
        }

        [Fact]
        public void Write_OrderABCreatedTable_WritesTableInThatOrder()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.Add("a", 1);
            tbl.Add("b", 2);

            // Act
            var s = Toml.WriteString(tbl);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo("a=1b=2");
        }

        [Fact]
        public void Write_OrderBACreatedTable_WritesTableInThatOrder()
        {
            // Arrange
            var tbl = Toml.Create();
            tbl.Add("b", 2);
            tbl.Add("a", 1);

            // Act
            var s = Toml.WriteString(tbl);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo("b=2a=1");
        }

        [Fact]
        public void Write_WhenClrObjectIsDictionary_WritesCorrectToml()
        {
            // Arrange
            Dictionary<string, object> d = new Dictionary<string, object>()
            {
                { "i", 1 },
                { "s", "s" },
                { "sub", new Dictionary<string, object>()
                    {
                        { "x", "2" }
                    }
                }
            };

            // Act
            var s = Toml.WriteString(d);

            // Assert
            s.Trim().Should().Be(@"
i = 1
s = ""s""

[sub]
x = ""2""".Trim());
        }



        private class TestClassA
        {
            public string StringProp { get; set; }
            public int IntProp { get; set; }
        }

        private class TestOrder
        {
            public List<TestClassA> TableArray { get; set; }

            public int Property { get; set; }

            public TestOrder()
            {
                TableArray = new List<TestClassA>()
                {
                    new TestClassA(),
                };
            }
        }

        private class WithArray
        {
            public int[] EmptyArray => new int[0];
            public int[] NonEmptyIntArray => new int[] { -100, 0, 100 };

            public List<string> StringList => new List<string>() { "A", "B", "C" };
        }

        private class WithDefaultComment
        {
            [TomlComment("This is a comment")]
            public int Commented { get; set; }
        }

        private class WithMultilineComment
        {
            [TomlComment("This is a\r\n multiline comment")]
            public int CommentedA { get; set; }

            [TomlComment("This is a \nmultiline comment")]
            public int CommentedB { get; set; }
        }


        private class WithAppendComment
        {
            [TomlComment("This is a comment", CommentLocation.Append)]
            public int Commented { get; set; }
        }

        private class WithBoolProperty
        {
            public int TheInt { get; set; }
            public bool TheBool { get; set; }
        }

        private class WithClassProperty
        {
            public int IntProp { get; set; }
            public ClassProperty ClassProp { get; set; }
            public string StringProp { get; set; }

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

        public class WithInlineTable
        {
            public InlineTable Table { get; set; }

            public WithInlineTable()
            {
                this.Table = new InlineTable();
            }
        }

        public class InlineTable
        {
            public int Ival { get; set; }
            public string Sval { get; set; }

            public InlineTable()
            {
                this.Ival = 1;
                this.Sval = "x";
            }
        }

        private class MultipleInline
        {
            public InlineTableWithInlineTable Inline { get; set; }

            public MultipleInline()
            {
                this.Inline = new InlineTableWithInlineTable();
            }
        }

        public class InlineTableWithInlineTable
        {
            public InlineTable SubInline { get; set; }

            public InlineTableWithInlineTable()
            {
                this.SubInline = new InlineTable();
            }
        }

        private class MultiLevelInlineTable
        {
            public NormalTableInsideInlineTable InlineL1 { get; set; }

            public MultiLevelInlineTable()
            {
                this.InlineL1 = new NormalTableInsideInlineTable();
            }

        }
        private class NormalTableInsideInlineTable
        {
            public ClassProperty InlineL2 { get; set; }


            public NormalTableInsideInlineTable()
            {
                this.InlineL2 = new ClassProperty();
            }
        }
    }
}
