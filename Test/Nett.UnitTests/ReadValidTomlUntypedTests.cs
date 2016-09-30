using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Nett.UnitTests.Util.TestData;
using Xunit;

namespace Nett.UnitTests
{
    /// <summary>
    /// Test for the basic test examples from https://github.com/BurntSushi/toml-test/tree/master/tests
    /// These cases handle some special cases and some document structure cases.
    /// Everything will be deserialized into a generic TomlTable data structure. Extracting the data has to be done by hand.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ReadValidTomlUntypedTests
    {
        [Fact]
        public void ReadValidTomlUntyped_EmptyArray()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayEmpty;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.NotNull(read);
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(typeof(TomlArray), read["thevoid"].GetType());
            var rootArray = read["thevoid"].Get<TomlArray>();
            Assert.Equal(1, rootArray.Length);
            var subArray = rootArray.Get<TomlArray>(0);
            Assert.Equal(0, subArray.Length);
        }

        [Fact]
        public void ReadValidToml_ArrayNoSpaces()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayNoSpaces;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Get<TomlArray>("ints").Get<int>(0));
            Assert.Equal(2, read.Get<TomlArray>("ints").Get<int>(1));
            Assert.Equal(3, read.Get<TomlArray>("ints").Get<int>(2));
        }

        [Fact]
        public void ReadValidToml_HetArray()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayHeterogenous;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            var a = read.Get<TomlArray>("mixed");
            Assert.NotNull(a);
            Assert.Equal(3, a.Length);

            var intArray = a.Get<TomlArray>(0);
            Assert.Equal(1, intArray.Get<int>(0));
            Assert.Equal(2, intArray.Get<int>(1));

            var stringArray = a.Get<TomlArray>(1);
            Assert.Equal("a", stringArray.Get<string>(0));
            Assert.Equal("b", stringArray.Get<string>(1));

            var doubleArray = a.Get<TomlArray>(2);
            Assert.Equal(1.1, doubleArray.Get<double>(0));
            Assert.Equal(2.1, doubleArray.Get<double>(1));
        }

        [Theory]
        [InlineData("a = []", new object[] { })]
        [InlineData("a = [0]", new object[] { 0 })]
        [InlineData("a = [0,]", new object[] { 0 })]
        [InlineData("a = [0,1]", new object[] { 0, 1 })]
        [InlineData("a = [0, 1,  ]", new object[] { 0, 1 })]
        public void ReadValidTomlIntArray(string toml, object[] expected)
        {
            var t = Toml.ReadString(toml);

            var a = t.Get<TomlArray>("a");

            a.Length.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                ((TomlInt)a[i]).Value.Should().Be((int)expected[i]);
            }
        }

        [Theory]
        [InlineData("a = []", new object[] { })]
        [InlineData("a = [0.0]", new object[] { 0.0 })]
        [InlineData("a = [0.0,]", new object[] { 0.0 })]
        [InlineData("a = [0.0,1.0]", new object[] { 0.0, 1.0 })]
        [InlineData("a = [0.0,  1.0,  ]", new object[] { 0.0, 1.0 })]
        public void ReadValidTomlDoubleArray(string toml, object[] expected)
        {
            var t = Toml.ReadString(toml);

            var a = t.Get<TomlArray>("a");

            a.Length.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                ((TomlFloat)a[i]).Value.Should().Be((double)expected[i]);
            }
        }

        [Theory]
        [InlineData("a = []", new object[] { })]
        [InlineData("a = [\"a\"]", new object[] { "a" })]
        [InlineData("a = [\"a\", 'b', '''cf''']", new object[] { "a", "b", "cf" })]
        [InlineData("a = ['''s''']", new object[] { "s" })]
        public void ReadValidTomlStringArray(string toml, object[] expected)
        {
            var t = Toml.ReadString(toml);

            var a = t.Get<TomlArray>("a");

            a.Length.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                ((TomlString)a[i]).Value.Should().Be((string)expected[i]);
            }
        }

        [Fact]
        public void ReadValidTomlArrayOfIntArray()
        {
            var tml = Toml.ReadString("a = [[1]]");

            var a = tml.Get<TomlArray>("a");
            var aa = a.Get<TomlArray>(0);

            a.Length.Should().Be(1);
            aa.Length.Should().Be(1);
            aa.Get<int>(0).Should().Be(1);
        }

        [Fact]
        public void ReadValidTomlArrayOfIntArrays()
        {
            var tml = Toml.ReadString("a = [[1], [2, 3], ]");

            var a = tml.Get<TomlArray>("a");
            var aa = a.Get<TomlArray>(0);
            var aaa = a.Get<TomlArray>(1);

            a.Length.Should().Be(2);
            aa.Length.Should().Be(1);
            aa.Get<int>(0).Should().Be(1);

            aaa.Length.Should().Be(2);
            aaa.Get<int>(0).Should().Be(2);
            aaa.Get<int>(1).Should().Be(3);
        }


        [Fact]
        public void RealValidToml_NestedArrays()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArraysNested;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            var a = read.Get<TomlArray>("nest");
            Assert.Equal("a", a.Get<TomlArray>(0).Get<string>(0));
            Assert.Equal("b", a.Get<TomlArray>(1).Get<string>(0));
        }

        [Fact]
        public void ReadValidToml_Arrays()
        {
            // Arrange
            var toml = TomlStrings.Valid.Arrays;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(3, read.Get<TomlArray>("ints").Get<TomlArray>().Length);
            Assert.Equal(3, read.Get<TomlArray>("floats").Get<TomlArray>().Length);
            Assert.Equal(3, read.Get<TomlArray>("strings").Get<TomlArray>().Length);
            Assert.Equal(3, read.Get<TomlArray>("dates").Get<TomlArray>().Length);
        }

        [Fact]
        public void ReadValidToml_TableArrayNested()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayNested;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(typeof(TomlTableArray), read["albums"].GetType());
            var arr = read.Get<TomlTableArray>("albums");
            Assert.Equal(2, arr.Count);

            var t0 = arr[0];
            Assert.Equal("Born to Run", t0.Get<string>("name"));
            var t0s = t0.Get<TomlTableArray>("songs");
            Assert.Equal(2, t0s.Count);
            var s0 = t0s[0];
            var s1 = t0s[1];
            Assert.Equal("Jungleland", s0.Get<string>("name"));
            Assert.Equal("Meeting Across the River", s1.Get<string>("name"));
        }

        [Fact]
        public void ReadValidTomlUntyped_Boolean()
        {
            // Arrange
            var toml = TomlStrings.Valid.Boolean;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(true, read.Get<bool>("t"));
            Assert.Equal(false, read.Get<bool>("f"));
        }

        [Fact]
        public void ReadValidTomlUntyped_CommentsEverywere()
        {
            // Arrange
            var toml = TomlStrings.Valid.CommentsEverywhereABNFCompatible;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(42, read.Get<TomlTable>("group").Get<int>("answer"));
            Assert.Equal(2, ((TomlTable)read["group"]).Get<int[]>("more").Length);
            Assert.Equal(42, ((TomlTable)read["group"]).Get<List<int>>("more")[0]);
            Assert.Equal(42, ((TomlTable)read["group"]).Get<int[]>("more")[0]);
        }

        [Fact]
        public void ReadValidTomlUntyped_DateTime()
        {
            // Arrange
            var toml = TomlStrings.Valid.DateTime;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(DateTime.Parse("1987-07-05T17:45:00Z"), read.Get<DateTimeOffset>("bestdayever"));
        }

        [Fact]
        public void ReadValidTomlUntyped_Empty()
        {
            // Arrange
            var toml = TomlStrings.Valid.Empty;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(0, read.Rows.Count());
        }

        [Fact]
        public void ReadValidToml_Example()
        {
            // Arrange
            var toml = TomlStrings.Valid.Example;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count());
            Assert.Equal(DateTime.Parse("1987-07-05T17:45:00Z"), read.Get<DateTimeOffset>("best-day-ever"));
            var tt = (TomlTable)read["numtheory"];
            Assert.Equal(false, tt.Get<bool>("boring"));
            var ta = (TomlArray)tt["perfection"];
            Assert.Equal(3, ta.Length);
            Assert.Equal(6, ta.Get<int>(0));
            Assert.Equal(28, ta.Get<char>(1));
            Assert.Equal(496, ta.Get<int>(2));
        }

        [Fact]
        public void ReadValidTomlUntyped_Floats()
        {
            // Arrange
            var toml = TomlStrings.Valid.Floats;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(3.14, read.Get<float>("pi"), 2);
            Assert.Equal(-3.14, read.Get<double>("negpi"), 2);
        }

        [Fact]
        public void ReadValidTomlUntyped_ImplicitAndExplicitAfter()
        {
            // Arrange
            var toml = TomlStrings.Valid.ImplicitAndExplicitAfter;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(42, read.Get<TomlTable>("a").Get<TomlTable>("b").Get<TomlTable>("c").Get<int>("answer"));
            Assert.Equal(43, read.Get<TomlTable>("a").Get<long>("better"));
        }

        [Fact]
        public void ReadValidTomlUntyped_ImplicitAndExplicitBefore()
        {
            // Arrange
            var toml = TomlStrings.Valid.ImplicitAndExplicitBefore;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(42, read.Get<TomlTable>("a").Get<TomlTable>("b").Get<TomlTable>("c").Get<int>("answer"));
            Assert.Equal(43, read.Get<TomlTable>("a").Get<long>("better"));
        }

        [Fact]
        public void ReadValidToml_ImplicitGroups()
        {
            // Arrange
            var toml = TomlStrings.Valid.ImplicitGroups;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(42, read.Get<TomlTable>("a").Get<TomlTable>("b").Get<TomlTable>("c").Get<char>("answer"));
        }

        [Fact]
        public void ReadValidTomlUntyped_Integer()
        {
            // Arrange
            var toml = TomlStrings.Valid.Integer;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count());
            Assert.Equal(42, read.Get<int>("answer"));
            Assert.Equal(-42, read.Get<short>("neganswer"));
        }

        [Fact]
        public void ReadValidTomlUntyped_KeyEqualsNoSpace()
        {
            // Arrange
            var toml = TomlStrings.Valid.KeyEqualsNoSpace;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(42, read.Get<int>("answer"));
        }

        [Fact]
        public void ReadValidTomlUntyped_KeyEqualsSpace()
        {
            // Arrange
            var toml = TomlStrings.Valid.KeyEqualsSpace;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(42, read.Get<int>("a b"));
        }

        [Fact]
        public void ReadValidTomlUntyped_KeySpecialChars()
        {
            // Arrange
            var toml = TomlStrings.Valid.KeySpecialChars;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(42, read.Get<int>("~!@$^&*()_+-`1234567890[]|/?><.,;:'"));
        }


        [Fact]
        public void ReadValidTomlUntyped_LongFloats()
        {
            // Arrange
            var toml = TomlStrings.Valid.LongFloats;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count());
            Assert.Equal(double.Parse("3.141592653589793", CultureInfo.InvariantCulture), read.Get<double>("longpi"));
            Assert.Equal(double.Parse("-3.141592653589793", CultureInfo.InvariantCulture), read.Get<double>("neglongpi"));
        }

        [Fact]
        public void ReadValidTomlUntyped_LongInts()
        {
            // Arrange
            var toml = TomlStrings.Valid.LongInts;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count());
            Assert.Equal(9223372036854775806, read.Get<long>("answer"));
            Assert.Equal(-9223372036854775807, read.Get<long>("neganswer"));
        }


        [Fact]
        public void ReadValidTomlUntyped_MultiLineStrings()
        {
            // Arrange
            var toml = TomlStrings.Valid.MultiLineStrings;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(7, read.Rows.Count());
            Assert.Equal("", read.Get<string>("multiline_empty_one"));
            Assert.Equal("", read.Get<string>("multiline_empty_two"));
            Assert.Equal("", read.Get<string>("multiline_empty_three"));
            Assert.Equal("", read.Get<string>("multiline_empty_four"));
            Assert.Equal("The quick brown fox jumps over the lazy dog.", read.Get<string>("equivalent_one"));
            Assert.Equal("The quick brown fox jumps over the lazy dog.", read.Get<string>("equivalent_two"));
            Assert.Equal("The quick brown fox jumps over the lazy dog.", read.Get<string>("equivalent_three"));
        }

        [Fact]
        public void ReadValidTomlUntyped_RawMultineStrings()
        {
            // Arrange
            var toml = TomlStrings.Valid.RawMultilineStrings;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(3, read.Rows.Count());
            Assert.Equal("This string has a ' quote character.", read.Get<string>("oneline"));
            Assert.Equal("This string has a ' quote character.", read.Get<string>("firstnl"));
            Assert.Equal("This string\r\n has a ' quote character\r\n and more than\r\n one newline\r\n in it.", read.Get<string>("multiline"));
        }

        [Fact]
        public void ReadValidStringsUntyped_RawStrings()
        {
            // Arrange
            var toml = TomlStrings.Valid.RawStrings;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(7, read.Rows.Count());
            Assert.Equal("This string has a \\b backspace character.", read.Get<string>("backspace"));
            Assert.Equal("This string has a \\t tab character.", read.Get<string>("tab"));
            Assert.Equal("This string has a \\n new line character.", read.Get<string>("newline"));
            Assert.Equal("This string has a \\f form feed character.", read.Get<string>("formfeed"));
            Assert.Equal("This string has a \\r carriage return character.", read.Get<string>("carriage"));
            Assert.Equal("This string has a \\/ slash character.", read.Get<string>("slash"));
            Assert.Equal("This string has a \\\\ backslash character.", read.Get<string>("backslash"));
        }

        [Theory(DisplayName = "Verify all valid TOML key types are supported")]
        [InlineData(@"1234 = ""foo""", "1234", "foo")]
        public void ReadValidTomls_SupportsAllKeyTypes(string toml, string key, object expected)
        {
            // Act
            var read = Toml.ReadString(toml);

            // Assert
            read[key].Get<string>().As<object>().Should().Be(expected);
        }

        [Fact]
        public void ReadValidStringsUntyped_StringEmpty()
        {
            // Arrange
            var toml = TomlStrings.Valid.StringEmpty;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal("", read.Get<string>("answer"));
        }

        [Fact]
        public void ReadValidStringsUntyped_StringEsapces()
        {
            // Arrange
            var toml = TomlStrings.Valid.StringEscapes;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(11, read.Rows.Count());
            Assert.Equal("This string has a \b backspace character.", read.Get<string>("backspace"));
            Assert.Equal("This string has a \t tab character.", read.Get<string>("tab"));
            Assert.Equal("This string has a \n new line character.", read.Get<string>("newline"));
            Assert.Equal("This string has a \f form feed character.", read.Get<string>("formfeed"));
            Assert.Equal("This string has a \r carriage return character.", read.Get<string>("carriage"));
            Assert.Equal("This string has a \" quote character.", read.Get<string>("quote"));
            Assert.Equal("This string has a \\ backslash character.", read.Get<string>("backslash"));
            Assert.Equal("This string does not have a unicode \\u escape.", read.Get<string>("notunicode1"));
            Assert.Equal("This string does not have a unicode \u005Cu escape.", read.Get<string>("notunicode2"));
            Assert.Equal("This string does not have a unicode \\u0075 escape.", read.Get<string>("notunicode3"));
            Assert.Equal("This string does not have a unicode \\\u0075 escape.", read.Get<string>("notunicode4"));
        }

        [Fact]
        public void ReadValidStringsUntyped_StringWihtPound()
        {
            // Arrange
            var toml = TomlStrings.Valid.StringWithPound;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count());
            Assert.Equal("We see no # comments here.", read.Get<string>("pound"));
            Assert.Equal("But there are # some comments here.", read.Get<string>("poundcomment"));
        }

        [Fact]
        public void ReadValidStringsUntyped_TableArrayImplicit()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayImplicit;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal("Glory Days", ((read.Get<TomlTable>("albums")).Get<TomlTableArray>("songs")[0]).Get<string>("name"));
        }

        [Fact]
        public void ReadValidStringsUntyped_NestedArraysOfTables()
        {
            // Arrange
            var toml = TomlStrings.Valid.NestedArrayOfTables;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            read.Get<TomlTableArray>("fruits").Count.Should().Be(2);
            read.Get<TomlTableArray>("fruits")[0].Get<TomlTableArray>("variety").Count.Should().Be(2);
            read.Get<TomlTableArray>("fruits")[1].Get<TomlTableArray>("variety").Count.Should().Be(1);
        }

        [Fact]
        public void ReadValidStringsUntyped_TableArrayMany()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayMany;

            // Act
            var read = Toml.ReadString(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count());
            Assert.Equal(3, read.Get<TomlTableArray>("people").Count);
            var tt = read.Get<TomlTableArray>("people")[0];
            Assert.Equal("Bruce", tt.Get<string>("first_name"));
            Assert.Equal("Springsteen", tt.Get<string>("last_name"));
            tt = read.Get<TomlTableArray>("people")[1];
            Assert.Equal("Eric", tt.Get<string>("first_name"));
            Assert.Equal("Clapton", tt.Get<string>("last_name"));
            tt = read.Get<TomlTableArray>("people")[2];
            Assert.Equal("Bob", tt.Get<string>("first_name"));
            Assert.Equal("Seger", tt.Get<string>("last_name"));
        }

        [Fact]
        public void ReadTomlFileWithSpecialChars_ReadsTomlCorrectly()
        {
            var tml = Toml.ReadFile("TomlWithSpecialCharacters.tml");

            tml.Get<string>("p0").Should().Be(@"c:\äöü");
        }

        [Fact]
        public void ReadTom_WithInlineTable_ReadsItCorrect()
        {
            // Act
            var tml = Toml.ReadString("name = { first = \"Tom\", last = \"Preston-Werner\" }");

            // Assert
            (tml.TryGetValue("name") as TomlTable).Should().NotBeNull();
            var t = tml.Get<TomlTable>("name");
            t.Rows.Count().Should().Be(2);
            t.Get<string>("first").Should().Be("Tom");
            t.Get<string>("last").Should().Be("Preston-Werner");
            t.TableType.Should().Be(TomlTable.TableTypes.Inline);
        }
    }
}
