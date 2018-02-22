using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Nett.Tests.Util;
using Nett.Tests.Util.TestData;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public sealed class VerifyIssuesTests
    {
        public class RootTable
        {
            public SubTable SubTable { get; set; } = new SubTable();

            public override string ToString()
            {
                return $"RootTable({SubTable})";
            }
        }

        public class SubTable
        {
            public class ListTable
            {
                public int SomeValue { get; set; } = 5;

                public override string ToString()
                {
                    return $"ListTable({SomeValue})";
                }
            }

            public List<ListTable> Values { get; set; } = new List<ListTable>();

            public override string ToString()
            {
                return $"SubTable({string.Join(",", Values.Select(p => p.ToString()))})";
            }
        }

        public class ClassWithStaticProperty
        {
            public const string StaticPropertyValue = "ShouldNotGetSerialized";
            public static string MyStaticProp { get; set; } = StaticPropertyValue;
            public string MyProp { get; set; } = "MyProp";
        }

        public class Size
        {
            public double Width { get; set; } = 1.0;
            public double Height { get; set; } = 2.0;
            public static Size Empty { get; } = new Size { Width = 0, Height = 0 };
        }

        [Fact(DisplayName = "Verify issue #15 was fixed: Serialization ignores static properties")]
        public void WriteToml_WhenInputIsClassWithStaticProperty_StaticPropertyIsIgnored()
        {
            // Act
            var str = Toml.WriteString(new ClassWithStaticProperty());

            // Assert
            str.Should().NotContain(ClassWithStaticProperty.StaticPropertyValue);
        }

        [Fact(DisplayName = "Verify issue #15 was fixed: Deserialization ignores static properties")]
        public void ReadToml_WhenInputContainsKeyForStaticProperty_ThatInputGetsIgnored()
        {
            // Arrange
            var input = $"{nameof(ClassWithStaticProperty.MyProp)} = 'X' \r\n {nameof(ClassWithStaticProperty.MyStaticProp)} = 'DoNotDeserThis'";

            // Act
            var read = Toml.ReadString<ClassWithStaticProperty>(input);

            // Assert
            ClassWithStaticProperty.MyStaticProp.Should().Be(ClassWithStaticProperty.StaticPropertyValue);
        }

        [Fact(DisplayName = "Verify issue #15 was fixed: Deserialization of class with self referencing static property causes StackOverflow")]
        public void ReadToml_WhenObjectHasSelfRefStaticProp_CanBeDeserializedCorrectl()
        {
            // Arrange
            var s = new Size();
            var written = Toml.WriteString(s);

            // Act
            var read = Toml.ReadString<Size>(written);

            // Assert
            read.Width.Should().Be(s.Width);
            read.Height.Should().Be(s.Height);
        }

        [Fact(DisplayName = "Verify issue #14 was fixed: Array of tables serialization forgot parent key")]
        public void WriteWithArrayOfTables_ProducesCorrectToml()
        {
            // Arrange
            var root = new RootTable();
            root.SubTable.Values.AddRange(new[]
            {
                new SubTable.ListTable() { SomeValue = 1 }, new SubTable.ListTable() { SomeValue = 5 }
            });
            const string expected = @"
[SubTable]

[[SubTable.Values]]
SomeValue = 1
[[SubTable.Values]]
SomeValue = 5";

            // Act
            var tml = Toml.WriteString(root);

            // Assert
            tml.ShouldBeSemanticallyEquivalentTo(expected);
        }

        [Fact(DisplayName = "Verify that issue #8 was fixed")]
        public void ReadAndWriteFloat_Issue8_IsFixed()
        {
            // Arrange
            MyObject obj = new MyObject();
            obj.MyFloat = 123;
            string output = Toml.WriteString<MyObject>(obj);

            // Act
            MyObject parsed = Toml.ReadString<MyObject>(output);

            // Assert
            parsed.MyFloat.Should().Be(123.0f);
        }

        [Fact(DisplayName = "Verify issue #16 is fixed: Uint gets serialized correctly.")]
        public void WriteToml_Issue16_Check()
        {
            // Arrange
            var obj = new WithUint();

            // Act
            string output = Toml.WriteString(obj);

            // Assert
            output.Trim().Should().Be("Prop = 1");
        }

        [Fact]
        public void VerifyIssue26_InlineTableWithoutSpaceAfeterValue_CanBeParsed()
        {
            var parsed = Toml.ReadString(TomlStrings.Valid.InlineTableNoSpaces);

            parsed.Get<TomlTable>("Test").Get<TomlTable>("InlineTable").Get<int>("test").Should().Be(1);
        }

        [Fact]
        public void VerifyIssue30_ArrayWithClosingBracketOnNextLine_IsParsedCorrectly()
        {
            const string Tml = @"
""foo"" = [
    ""bar""
]
";

            var a = Toml.ReadString(Tml).Get<string[]>("foo");

            a.Length.Should().Be(1);
            a[0].Should().Be("bar");
        }

        [Fact]
        public void VerifyIssue30_WithCommentBeforeFirstValue_IsParsedCorrectly()
        {
            const string Tml = @"
""foo"" = [
    # Comment
    ""bar"",
]
";

            var a = Toml.ReadString(Tml).Get<string[]>("foo");

            a.Length.Should().Be(1);
            a[0].Should().Be("bar");
        }


        [Fact]
        public void VerifyIssue34_SerializeDictionary_WasFixed()
        {
            // Arrange
            var d = new Dictionary<string, bool>()
            {
                {"Hello World!", true},
            };


            // Act
            var s = Toml.WriteString(d);

            // Assert
            s.Trim().Should().Be(@"'Hello World!' = true".Trim());
        }

        [Fact]
        public void VerifyIssue42_SerializedIntoWrongTable_WasFixed()
        {
            var tbl = Toml.Create();
            tbl.Add("root1", 1);    // These three keys added at root level
            tbl.Add("root2", 2);
            tbl.Add("root3", 3);

            var t2 = tbl.AddTomlObject("Level2", Toml.Create());
            t2.Add("second1", "x");
            t2.Add("second2", "y");

            tbl.Add("root4", 4);    // Added at root level but spuriously written into [Level2.Level3]

            var t3 = t2.AddTomlObject("Level3", Toml.Create());
            t3.Add("third1", "a");
            t3.Add("third2", "b");

            t2.Add("second3", "z"); // Added to [Level2] but spuriously written into [Level2.Level3]
            tbl.Add("root5", 5);    // Added at root level but spuriously written into [Level2.Level3]

            var result = Toml.WriteString(tbl);
            result.Trim().Should().Be(@"
root1 = 1
root2 = 2
root3 = 3
root4 = 4
root5 = 5

[Level2]
second1 = ""x""
second2 = ""y""
second3 = ""z""

    [Level2.Level3]
    third1 = ""a""
    third2 = ""b""".Trim());
        }

        public class WithUint
        {
            public uint Prop { get; set; } = 1;
        }

        public class MyObject
        {
            public float MyFloat { get; set; }
        }

    }
}
