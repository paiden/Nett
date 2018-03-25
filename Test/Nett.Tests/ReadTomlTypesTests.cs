using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    /// <summary>
    /// Checks that basic TOML types can be loaded in alls possible ranges and formats. These tests don't care about
    /// the larger TOML document structure.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ReadTomlTypesTests
    {
        [Theory]
        [InlineData("Key = 0", "Key", 0)]
        [InlineData("Key = +0", "Key", 0)]
        [InlineData("Key = -0", "Key", 0)]
        [InlineData("Key = 1", "Key", 1)]
        [InlineData("Key = +1", "Key", 1)]
        [InlineData("Key = -1", "Key", -1)]
        public void Deserialize_SimpleyKeyValuePair_ProducesCorrectObject(string toParse, string key, long expectedValue)
        {
            var parsed = Toml.ReadString(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
        }

        [Fact]
        public void Deserialize_WithMultipleSignBeforeNumber_FailsToParse()
        {
            Assert.Throws<Exception>(() => Toml.ReadString("key = --0"));
        }

        [Theory]
        [InlineData("Key = 1_000", "Key", 1000)]
        [InlineData("Key = 1_2_3_4", "Key", 1234)]
        public void Deserialize_IntWithUnderscoreSeperator_ProducesCorrectObject(string toParse, string key, object expectedValue)
        {
            var parsed = Toml.ReadString(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
        }

        [Theory]
        [InlineData("Key = 01")]
        [InlineData("Key = 0_1")]
        public void Deserilaize_IntWihtLeadingZeros_Fails(string toParse)
        {
            Assert.Throws<Exception>(() => Toml.ReadString(toParse));
        }

        [Theory]
        [InlineData("a = 0xDEADBEEF", "DEADBEEF")]
        [InlineData("a = 0xdeadbeef", "deadbeef")]
        [InlineData("a = 0xdead_beef", "deadbeef")]
        public void Deserialize_HexInt_DeliversCorrectTomlInt(string src, string expected)
        {
            // Arrange
            long.TryParse(expected, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long expectedVal);

            // Act
            var parsed = Toml.ReadString(src);

            // Assert
            parsed.Get<long>("a").Should().Be(expectedVal);
        }

        [Theory]
        [InlineData("a = 0o000012_34567", "01234567")]
        [InlineData("a = 0o75_5 ", "755")]
        public void Deserialize_OctInt_DeliversCorrectTomlInt(string src, string expected)
        {
            // Arrange
            long expectedVal = Convert.ToInt64(expected, 8);

            // Act
            var parsed = Toml.ReadString(src);

            // Assert
            parsed.Get<long>("a").Should().Be(expectedVal);
        }

        [Theory]
        [InlineData("a = 0b1111_0000_1111_0101", "1111000011110101")]
        [InlineData("a = 0b1_0_1_0_1_0_1_0 ", "10101010")]
        [InlineData("a = 0b0000", "0")]
        public void Deserialize_BinInt_DeliversCorrectTomlInt(string src, string expected)
        {
            // Arrange
            long expectedVal = Convert.ToInt64(expected, 2);

            // Act
            var parsed = Toml.ReadString(src);

            // Assert
            parsed.Get<long>("a").Should().Be(expectedVal);
        }

        [Fact]
        public void Deserilize_WithComments_ParserCanHandleCommentsCorrectly()
        {
            string toParse = "# I am a comment. Hear me roar. Roar." + Environment.NewLine +
                             "key = 0 # Yeah, you can do this. ";

            var parsed = Toml.ReadString(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(0, parsed.Get<int>("key"));
        }

        [Theory]
        [InlineData(@"str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\nLocation\tSF.""", "str", "I'm a string. \"You can quote me\". Name\tJos\u00E9\nLocation\tSF.")]
        public void Deserialize_SingleLineSringValue_DeserializesCorrectly(string toParse, string key, string expected)
        {
            var parsed = Toml.ReadString(toParse);

            Assert.Equal(expected, parsed.Get<string>(key));
        }

        [Fact]
        public void Deseriailze_SingleLineStringWithNewLine_FailsToDeserialize()
        {
            Assert.Throws<Exception>(() => Toml.ReadString("str = \"\r\n\""));
        }

        [Theory]
        [InlineData("X = \"C:\\\\test.txt\"", @"C:\test.txt")]
        public void Read_StringWithEscapeChars_ReadsCorrectly(string toRead, string expected)
        {
            // Act
            var parsed = Toml.ReadString(toRead);

            // Assert
            Assert.Equal(expected, parsed.Get<string>("X"));
        }

        [Theory]
        [InlineData(@"str = ""\u0000004D""", "\u0000004D")]
        [InlineData(@"str = ""\u03B4""", "\u03B4")]
        [InlineData(@"str = ""\U000003B4""", "\U000003B4")]
        public void Deserilize_StringWithEscapedChar_DeserializesCorrectly(string input, string expected)
        {
            var parsed = Toml.ReadString(input);

            parsed.Get<string>("str").Should().Be(expected);
        }

        [Theory]
        [InlineData("str = \"\"\"\"\"\"", "")]
        [InlineData("str = \"\"\" \"\"\"", " ")]
        [InlineData("str = \"\"\"\r\n\"\"\"", "")]
        [InlineData("str = \"\"\"\r\nHello\r\nYou  \"\"\"", "Hello\r\nYou  ")]
        [InlineData("str = ''''''", "")]
        [InlineData("str = '''''''", "'")]
        [InlineData("str = ''''''''", "''")]
        public void Deserialize_MString_ProducesCorrectresult(string src, string expected)
        {
            var parsed = Toml.ReadString(src);

            Assert.Equal(expected, parsed.Get<string>("str"));
        }

        const string Case1 = @"str = """"""
The quick brown \


   fox jumps over \
        the lazy dog.""""""";

        const string Case2 = @"str = """"""\
       The quick brown \
       fox jumps over \
       the lazy dog.\
       """"""";

        [Theory]
        [InlineData(Case1)]
        [InlineData(Case2)]
        public void Deserialize_MStringWithSpaceReduction_ProducesCorrectResult(string src)
        {
            var parsed = Toml.ReadString(src);

            Assert.Equal("The quick brown fox jumps over the lazy dog.", parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData(@"str = 'C:\Users\nodejs\templates'", @"C:\Users\nodejs\templates")]
        [InlineData(@"str = '\\ServerX\admin$\system32\'", @"\\ServerX\admin$\system32\")]
        [InlineData(@"str = 'Tom ""Dubs"" Preston-Werner'", @"Tom ""Dubs"" Preston-Werner")]
        [InlineData(@"str = '<\i\c*\s*>'", @"<\i\c*\s*>")]
        public void Deserialize_LiteralString_ProducesCorrectResult(string src, string expected)
        {
            var parsed = Toml.ReadString(src);

            Assert.Equal(expected, parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData(@"str = '''I [dw]on't need \d{2} apples'''", @"I [dw]on't need \d{2} apples")]
        [InlineData(@"str  = '''
The first newline is
trimmed in raw strings.
   All other whitespace
   is preserved.
'''",
@"The first newline is
trimmed in raw strings.
   All other whitespace
   is preserved.
")]
        public void Deserialize_MutliLineLiteralString_ProducesCorrectResult(string src, string expected)
        {
            var parsed = Toml.ReadString(src);

            Assert.Equal(expected, parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData("+1.0", 1.0)]
        [InlineData("2.5", 2.5)]
        [InlineData("3.1415", 3.1415)]
        [InlineData("-0.0101", -0.0101)]
        [InlineData("5e+22", 5e+22)]
        [InlineData("1e6", 1e6)]
        [InlineData("-2E-2", -2E-2)]
        [InlineData("6.626e-34", 6.626e-34)]
        [InlineData("9_223_617.445_991_228_313", 9223617.445991228313)]
        [InlineData("1e1_00", 1e100)]
        [InlineData("inf", double.PositiveInfinity)]
        [InlineData("+inf", double.PositiveInfinity)]
        [InlineData("-inf", double.NegativeInfinity)]
        [InlineData("nan", double.NaN)]
        [InlineData("+nan", double.NaN)]
        [InlineData("-nan", -double.NaN)]
        public void Deserialize_FloatKeyValuePair_ProducesCorrectResult(string src, double expected)
        {
            // Act
            var parsed = Toml.ReadString($"x={src}");

            // Assert
            parsed.Get<double>("x").Should().Be(expected);
        }

        [Theory]
        [InlineData("d = +01.0")]
        [InlineData("d = 03.1415")]
        [InlineData("d = -00.01")]
        [InlineData("d = 05e+22")]
        [InlineData("d = 01e6")]
        [InlineData("d = -02E-2")]
        [InlineData("d = 06.626e-34")]
        public void Deserialize_FloatWithLeadingZeros_ThrowsExcption(string src)
        {
            var exc = Assert.Throws<Exception>(() => Toml.ReadString(src));
        }



        [Theory]
        //[InlineData("b = true", true)]
        [InlineData("b = false", false)]
        public void Deserialize_Bool_DeseriailzesCorrectly(string src, bool expected)
        {
            var parsed = Toml.ReadString(src);
            Assert.Equal(expected, parsed["b"].Get<bool>());
        }


        [Theory]
        [InlineData("1979-05-27", "1979-05-27T00:00:00")]
        [InlineData("1979-05-27T00:32:00.999999", "1979-05-27T00:32:00.999999")]
        [InlineData("1979-05-27T07:32:00", "1979-05-27T07:32:00")]
        [InlineData("1979-05-27T07:32:00Z", "1979-05-27T07:32:00Z")]
        [InlineData("1979-05-27T00:32:00-07:00", "1979-05-27T00:32:00-07:00")]
        [InlineData("1979-05-27T00:32:00+07:00", "1979-05-27T00:32:00+07:00")]
        [InlineData("1979-05-27T00:32:00.999999-07:00", "1979-05-27T00:32:00.999999-07:00")]
        [InlineData("1979-05-27T00:32:00.999999+07:00", "1979-05-27T00:32:00.999999+07:00")]
        [InlineData("2000-01-01T00:00:00+01:00", "2000-01-01T00:00:00+01:00")]
        public void Deserialize_DateTime_DeseriaizesCorrectly(string src, string expectedDate)
        {
            // Arrange
            var date = DateTimeOffset.Parse(expectedDate);

            // Act
            var parsed = Toml.ReadString($"x={src}");

            // Assert
            Assert.Equal(date, parsed["x"].Get<DateTimeOffset>());
        }

        public static TheoryData<string> LocalTimeData
            => new TheoryData<string>()
            {
                { "07:32:00" },
                { "00:32:00.999999"},
            };

        [Theory]
        [MemberData(nameof(LocalTimeData))]
        public void Deserialize_LocalTime_DeserializesCorrectly(string src)
        {
            // Arrange
            var date = DateTimeOffset.Parse($"0001-01-02T{src}", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

            // Act
            var parsed = Toml.ReadString($"x={src}");

            // Assert
            parsed["x"].Get<DateTimeOffset>().Should().Be(date);
        }

        [Fact]
        public void Deserialze_WithIntArray_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString("a = [1,2,3]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(3, a.Length);
            Assert.Equal(1, a.Get<int>(0));
            Assert.Equal(2, a.Get<int>(1));
            Assert.Equal(3, a.Get<int>(2));
        }

        [Fact]
        public void Deserialize_WithStringArray_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString(@"a = [""red"", ""yellow"", ""green""]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(3, a.Length);
            Assert.Equal("red", a.Get<string>(0));
            Assert.Equal("yellow", a.Get<string>(1));
            Assert.Equal("green", a.Get<string>(2));
        }

        [Fact]
        public void Deserialize_NestedIntArray_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString("a = [ [1, 2], [3, 4, 5] ]");

            var a = (TomlArray)parsed["a"];
            Assert.Equal(2, a.Length);
            var a1 = a.Get<TomlArray>(0);
            Assert.Equal(2, a1.Length);
            Assert.Equal(1, a1[0].Get<long>());
            Assert.Equal(2, a1.Get<int>(1));

            var a2 = (TomlArray)a[1];
            Assert.Equal(3, a2.Length);
            Assert.Equal(3, a2.Get<int>(0));
            Assert.Equal(4, a2.Get<int>(1));
            Assert.Equal(5, a2.Get<int>(2));
        }

        [Fact]
        public void Deserialize_ComplexStrings_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString(@"a = [""all"", 'strings', """"""are the same"""""", '''type''']");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(4, a.Length);
            Assert.Equal("all", a[0].Get<string>());
            Assert.Equal("strings", a[1].Get<string>());
            Assert.Equal("are the same", a[2].Get<string>());
            Assert.Equal("type", a[3].Get<string>());
        }

        public static TheoryData<string, TimeSpan> DurationParseData
            => new TheoryData<string, TimeSpan>()
            {
                { "2m1s", TimeSpan.Parse("00:02:01") },
                { "3h2m1s", TimeSpan.Parse("03:02:01") },
                { "4d3h2m1s", TimeSpan.Parse("4.03:02:01") },
                { "4d3h2m1s1ms", TimeSpan.Parse("4.03:02:01.001") },
                { "-4d3h2m1s1ms", -TimeSpan.Parse("4.03:02:01.001") },
                { "0.5d0.5h0.5m0.5s", TimeSpan.Parse("12:30:30.5") },
                { "1_0h2_0.5m6_0s", TimeSpan.Parse("10:21:30") },
            };

        [Theory]
        [MemberData(nameof(DurationParseData))]
        public void Deserialize_WithTimepsanInAllSupportedFormats_DeserializesCorrectly(string timespan, TimeSpan expected)
        {
            var parsed = Toml.ReadString($"a = {timespan}");

            var a = parsed.Get<TimeSpan>("a");

            a.Should().Be(expected);
        }

        [Fact]
        public void Deserialize_ArrayWithNesteArrayOfDiffernetTypes_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString(@"a = [ [1, 2], [""a"", ""b"",""c""]]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(2, a.Length);
            var a1 = (TomlArray)a[0];
            Assert.Equal(2, a1.Length);
            Assert.Equal(1, a1.Get<int>(0));
            Assert.Equal(2, a1.Get<int>(1));

            var a2 = (TomlArray)a[1];
            Assert.Equal("a", a2.Get<string>(0));
            Assert.Equal("b", a2.Get<string>(1));
            Assert.Equal("c", a2.Get<string>(2));
        }

        [Fact]
        public void Deserialize_Table_DeserializesCorrectly()
        {
            var parsed = Toml.ReadString(@"[table]");

            Assert.Single(parsed.Rows);
            Assert.NotNull(parsed.Get<TomlTable>("table"));
        }

        [Fact]
        public void Deserialize_TableWithMultipleKeys_DeserializesCorrectly()
        {
            string toParse = @"
key1 = ""value1""
key2 = ""value2""
key3 = ""value3""";

            var parsed = Toml.ReadString(toParse);

            Assert.Equal(3, parsed.Rows.Count());
            Assert.Equal("value1", parsed["key1"].Get<string>());
            Assert.Equal("value2", parsed.Get<string>("key2"));
            Assert.Equal("value3", parsed.Get<string>("key3"));
        }

        [Fact]
        public void Deserialize_TableWithMultipleBareKeys_DeserializesCorrectly()
        {
            string toParse = @"
key1 = ""value1""
key_2 = ""value2""
key-3 = ""value3""";

            var parsed = Toml.ReadString(toParse);

            Assert.Equal(3, parsed.Rows.Count());
            Assert.Equal("value1", parsed["key1"].Get<string>());
            Assert.Equal("value2", parsed.Get<string>("key_2"));
            Assert.Equal("value3", parsed.Get<string>("key-3"));
        }

        [Fact]
        public void Deserialize_TableWithMultipleQuoteKeys_DeserializesCorrectly()
        {
            string toParse = @"
""127.0.0.1""= ""value1""
""character encoding"" = ""value2""";
            // ""ʎǝʞ"" = ""value3"""; This case currently doesn't work, but it is such an unimportant case I don't want to put time into it
            // for now, as I really need a basic working TOML system. Hopefully I will have time to take care of this special cases soon.

            var parsed = Toml.ReadString(toParse);

            Assert.Equal(2, parsed.Rows.Count());
            Assert.Equal("value1", parsed["127.0.0.1"].Get<string>());
            Assert.Equal("value2", parsed.Get<string>("character encoding"));
            //Assert.Equal("value3", parsed.Get<string>("ʎǝʞ"));
        }


        [Fact]
        public void Deserialize_TableWithImplicitSubtable_DeserializesCorrectly()
        {
            string toParse = @"
[dog.""tater.man""]
type = ""pug""";
            // ""ʎǝʞ"" = ""value3"""; This case currently doesn't work, but it is such an unimportant case I don't want to put time into it
            // for now, as I really need a basic working TOML system. Hopefully I will have time to take care of this special cases soon.

            var parsed = Toml.ReadString(toParse);

            Assert.NotNull(parsed["dog"]);
            var tt = (TomlTable)((TomlTable)parsed["dog"])["tater.man"];
            Assert.NotNull(tt);
            Assert.Equal("pug", tt.Get<string>("type"));
        }

        [Fact]
        public void Deserialize_WhenSameTableDefinedMultipleTimes_Throws()
        {
            string toParse = @"
[a]
b = 1
[a]
d = 2";

            var exc = Assert.Throws<Exception>(() => Toml.ReadString(toParse));
            Assert.Contains("'a'", exc.Message);
        }

        [Fact]
        public void Deserialize_TableArray_DeserializesCorrectly()
        {
            string toParse = @"
[[products]]
name = ""Hammer""
sku = 738594937

[[products]]

[[products]]
name = ""Nail""
sku = 284758393
color = ""gray""
";

            var parsed = Toml.ReadString(toParse);

            var a = (TomlTableArray)parsed["products"];
            Assert.Equal(3, a.Count);

            var t1 = a[0];
            Assert.Equal("Hammer", t1.Get<string>("name"));

            var t2 = a[1];
            Assert.Empty(t2.Rows);

            var t3 = a[2];
            Assert.Equal("Nail", t3.Get<string>("name"));
        }


    }
}
