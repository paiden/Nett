using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Nett.Tests.Util;
using Nett.Tests.Util.TestData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                return $"RootTable({this.SubTable})";
            }
        }

        public class SubTable
        {
            public class ListTable
            {
                public int SomeValue { get; set; } = 5;

                public override string ToString()
                {
                    return $"ListTable({this.SomeValue})";
                }
            }

            public List<ListTable> Values { get; set; } = new List<ListTable>();

            public override string ToString()
            {
                return $"SubTable({string.Join(",", this.Values.Select(p => p.ToString()))})";
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

            var t2 = tbl.Add("Level2", Toml.Create());
            t2.Add("second1", "x");
            t2.Add("second2", "y");

            tbl.Add("root4", 4);    // Added at root level but spuriously written into [Level2.Level3]

            var t3 = t2.Add("Level3", Toml.Create());
            t3.Add("third1", "a");
            t3.Add("third2", "b");

            t2.Add("second3", "z"); // Added to [Level2] but spuriously written into [Level2.Level3]
            tbl.Add("root5", 5);    // Added at root level but spuriously written into [Level2.Level3]

            var result = Toml.WriteString(tbl);
            result.Trim().ShouldBeNormalizedEqualTo(@"
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

        [Fact]
        public void VerifyIssue44_UpdateTomlTableArrayRow_WasFixed()
        {
            // Arrange
            var tb = Toml.Create();
            var tList = new List<TestTable>();
            tList.Add(new TestTable("first", DateTimeOffset.MaxValue));
            tb.Add("test", tList);
            tList.Add(new TestTable("second", DateTimeOffset.MinValue));

            // Act
            var ta = tb.Update("test", tList);

            // Assert
            tb["test"].Should().BeOfType<TomlTableArray>();
            ta[0].Get<TestTable>().timestamp.Should().Be(DateTimeOffset.MaxValue);
            ta[1].Get<TestTable>().timestamp.Should().Be(DateTimeOffset.MinValue);
        }

        [Fact]
        public void VerifyIssue51_InlineTableSerializedInWrongParentTable_IsFixed()
        {
            // Arrange
            var table = Toml.Create();
            var root = table.Add("Root", new object(), TomlTable.TableTypes.Default);
            root.Add("A", new object(), TomlTable.TableTypes.Default);
            root.Add("B", new object(), TomlTable.TableTypes.Inline);

            // Act
            var ser = Toml.WriteString(table);

            // Assert
            ser.ShouldBeSemanticallyEquivalentTo(@"
[Root]
B = {  }

[Root.A]
");
        }

        [Fact]
        public void VerifyIssue53_EmptyInlineTableFailsToBeParsed_IsFixed()
        {
            // Arrange
            const string Tml = @"
[Root]
A = {  }
";
            Action action = () => Toml.ReadString(Tml);

            // Assert
            action.ShouldNotThrow();
        }

        [Fact]
        public void VerifyIssue54_WriteTomlWithMultiArrays_ProducesCorrectTomlOutput()
        {
            // Arrange
            var obj = new MultiDimArray();

            // Act
            string output = Toml.WriteString(obj);
            var x = new int[,] { { 1 }, { 2 } };

            // Assert
            output.Trim().ShouldBeSemanticallyEquivalentTo(MultiDimArray.TomlRep);
        }

        [Fact]
        public void VerifyIssue54_ReadTomlWithMultiArrays_ProducesCorrectObject()
        {
            // Act
            var obj = Toml.ReadString<MultiDimArray>(MultiDimArray.TomlRep);

            // Assert
            obj.ShouldBeEquivalentTo(new MultiDimArray());
        }

        [Fact]
        public void VerifyIssue57_EscapedQuoteAtStartOfStringFailsToBeParsed_IsFixed()
        {
            // Arrange
            const string Tml = "a=\"\\\"\""; // a = "\""

            // Act
            var table = Toml.ReadString(Tml);

            // Assert
            table.Get<string>("a").Should().Be("\"");
        }

        public static TheoryData<string, object> EquivalentMappingTestData
        {
            get
            {
                const string tbl = @"
[x]
r = 1";
                const string tbla = @"
[[x]]
r = 1";

                var extbl = new Dictionary<string, object>()
                {
                    { "r", 1 }
                };

                var extbla = new List<Dictionary<string, object>>() { extbl };

                var d = new TheoryData<string, object>();
                d.Add("x=1", 1L);
                d.Add("x=1.0", 1.0);
                d.Add("x=\"b\"", "b");
                d.Add(tbl, extbl);
                d.Add(tbla, extbla);
                return d;
            }
        }

        [Theory]
        [MemberData(nameof(EquivalentMappingTestData))]
        public void VerifyIssue63_GetWithObjectType_MapsItToEquivalentClrTypeAutomatically(string input, object expected)
        {
            // Arrange
            var tml = Toml.ReadString(input);

            // Act
            var converted = tml.Get<object>("x");

            // Assert
            converted.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void VerifyIssue65_WriteToml_WhenObjectIsJsonStructure_CreatesCorrectContent()
        {
            // Arrange
            const string jsonString = @"{""MongoURL"":""localhost"",""ElasticsearchUrls"":""http://localhost:9200""}";
            object configurationObject = JsonConvert.DeserializeObject(jsonString);

            // Act
            string contents = Toml.WriteString(configurationObject);

            // Assert
            contents.ShouldBeSemanticallyEquivalentTo(
                @"
Type = ""Object""
HasValues = true
First = [[]]
Last = [[]]
Count = 2
Root = [[[]], [[]]]
Path = """"");
        }


        [Fact]
        public void VerifyIssue65_WriteToml_WhenObjectIsJsonStructureDict_CreatesCorrectContent()
        {
            // Arrange
            const string jsonString = @"{""MongoURL"":""localhost"",""ElasticsearchUrls"":""http://localhost:9200""}";
            JObject configurationObject = (JObject)JsonConvert.DeserializeObject(jsonString);
            var serMe = configurationObject.ToObject<Dictionary<string, object>>();

            // Act
            string contents = Toml.WriteString(serMe);

            // Assert
            contents.ShouldBeSemanticallyEquivalentTo(
                @"
MongoURL = ""localhost""
ElasticsearchUrls = ""http://localhost:9200""");
        }

        [Fact]
        public void VerifyIssue66_ReadWriteToml_DoesWriteDateTimeWithSpace()
        {
            // Arrane
            const string input = @"x = 2018-12-26 22:31:33.109707-07:00";

            // Act
            var read = Nett.Toml.ReadString(input);
            var written = Toml.WriteString(read);

            // Assert
            written.Trim().Should().Be(input);
        }

        public class TestTable
        {
            public string label { get; set; }
            public DateTimeOffset timestamp { get; set; }

            public TestTable(string l, DateTimeOffset t) { this.label = l; this.timestamp = t; }
            public TestTable() { }
        };


        public class WithUint
        {
            public uint Prop { get; set; } = 1;
        }

        public class MyObject
        {
            public float MyFloat { get; set; }
        }

        public class MultiDimArray
        {
            public const string TomlRep = @"
A1D = [1, 2]
A2D = [[1, 2], [3, 4]]
A3DE = [[[]]]
A3D = [[[1, 2, 3]], [[4, 5, 6]], [[7, 8, 9]]]
L1D = [1, 2]
L2D = [[1, 2], [3, 4]]
L3DE = [[[]]]
L3D = [[[1, 2, 3]], [[4, 5, 6]], [[7, 8, 9]]]";

            public int[] A1D { get; set; } = new int[] { 1, 2 };

            public int[][] A2D { get; set; } = { new int[] { 1, 2 }, new int[] { 3, 4 } };

            public int[][][] A3DE { get; set; } = new int[][][] { new int[][] { new int[] { } } };

            public int[][][] A3D { get; set; } = new int[][][]
            {
                new int[][]
                {
                    new int[] { 1, 2, 3 }
                },

                new int[][]
                {
                    new int[] { 4, 5, 6 }
                },


                new int[][]
                {
                    new int [] {7, 8, 9}
                },
            };

            public List<int> L1D { get; set; } = new List<int> { 1, 2 };

            public List<List<int>> L2D { get; set; } = new List<List<int>> { new List<int> { 1, 2 }, new List<int> { 3, 4 } };

            public List<List<List<int>>> L3DE { get; set; } = new List<List<List<int>>>() { new List<List<int>> { new List<int>() { } } };

            public List<List<List<int>>> L3D { get; set; } = new List<List<List<int>>>
            {
                new List<List<int>>
                {
                    new List<int> { 1, 2, 3 }
                },

                new List<List<int>>
                {
                    new List<int> { 4, 5, 6 }
                },


                new List<List<int>>
                {
                    new List<int> {7, 8, 9}
                },
            };
        }
    }
}
