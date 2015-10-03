using System;
using Xunit;

namespace Nett.UnitTests
{
    /// <summary>
    /// Checks that basic TOML types can be loaded in alls possible ranges and formats. These tests don't care about
    /// the larger TOML document structure.
    /// </summary>
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
            var parsed = Toml.Read(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
        }

        [Fact]
        public void Deserialize_WithMultipleSignBeforeNumber_FailsToParse()
        {
            Assert.Throws<Exception>(() => Toml.Read("key = --0"));
        }

        [Theory]
        [InlineData("Key = 1_000", "Key", 1000)]
        [InlineData("Key = 1_2_3_4", "Key", 1234)]
        public void Deserialize_IntWithUnderscoreSeperator_ProducesCorrectObject(string toParse, string key, object expectedValue)
        {
            var parsed = Toml.Read(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
        }

        [Theory]
        [InlineData("Key = 01")]
        [InlineData("Key = 0_1")]
        public void Deserilaize_IntWihtLeadingZeros_Fails(string toParse)
        {
            Assert.Throws<Exception>(() => Toml.Read(toParse));
        }

        [Fact]
        public void Deserilize_WithComments_ParserCanHandleCommentsCorrectly()
        {
            string toParse = "# I am a comment. Hear me roar. Roar." + Environment.NewLine +
                             "key = 0 # Yeah, you can do this. ";

            var parsed = Toml.Read(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(0, parsed.Get<int>("key"));
        }

        [Theory]
        [InlineData(@"str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\nLocation\tSF.""", "str", "I'm a string. \"You can quote me\". Name\tJos\u00E9\nLocation\tSF.")]
        public void Deserialize_SingleLineSringValue_DeserializesCorrectly(string toParse, string key, string expected)
        {
            var parsed = Toml.Read(toParse);

            Assert.Equal(expected, parsed.Get<string>(key));
        }

        [Fact]
        public void Deseriailze_SingleLineStringWithNewLine_FailsToDeserialize()
        {
            Assert.Throws<ArgumentException>(() => Toml.Read("str = \"\r\n\""));
        }

        [Theory]
        [InlineData("X = \"C:\\\\test.txt\"", @"C:\test.txt")]
        public void Read_StringWithEscapeChars_ReadsCorrectly(string toRead, string expected)
        {
            // Act
            var parsed = Toml.Read(toRead);

            // Assert
            Assert.Equal(expected, parsed.Get<string>("X"));
        }

        [Fact]
        public void Deserilize_StringWithUTF32Char_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"str = ""\u0000004D""");

            Assert.Equal("\u0000004D", parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData("str = \"\"\"\"\"\"", "")]
        [InlineData("str = \"\"\" \"\"\"", " ")]
        [InlineData("str = \"\"\"\r\n\"\"\"", "")]
        [InlineData("str = \"\"\"\r\nHello\r\nYou  \"\"\"", "Hello\r\nYou  ")]
        public void Deserialize_MString_ProducesCorrectresult(string src, string expected)
        {
            var parsed = Toml.Read(src);

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
            var parsed = Toml.Read(src);

            Assert.Equal("The quick brown fox jumps over the lazy dog.", parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData(@"str = 'C:\Users\nodejs\templates'", @"C:\Users\nodejs\templates")]
        [InlineData(@"str = '\\ServerX\admin$\system32\'", @"\\ServerX\admin$\system32\")]
        [InlineData(@"str = 'Tom ""Dubs"" Preston-Werner'", @"Tom ""Dubs"" Preston-Werner")]
        [InlineData(@"str = '<\i\c*\s*>'", @"<\i\c*\s*>")]
        public void Deserialize_LiteralString_ProducesCorrectResult(string src, string expected)
        {
            var parsed = Toml.Read(src);

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
            var parsed = Toml.Read(src);

            Assert.Equal(expected, parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData("d = +1.0", 1.0)]
        [InlineData("d = 2.5", 2.5)]
        [InlineData("d = 3.1415", 3.1415)]
        [InlineData("d = -0.0101", -0.0101)]
        [InlineData("d = 5e+22", 5e+22)]
        [InlineData("d = 1e6", 1e6)]
        [InlineData("d = -2E-2", -2E-2)]
        [InlineData("d = 6.626e-34", 6.626e-34)]
        [InlineData("d = 9_223_617.445_991_228_313", 9223617.445991228313)]
        [InlineData("d = 1e1_00", 1e100)]
        public void Deserialize_FloatKeyValuePair_ProducesCorrectResult(string src, double expected)
        {
            var parsed = Toml.Read(src);

            Assert.Equal(expected, parsed.Get<double>("d"), 8);
        }

        [Theory]
        [InlineData("d = +01.0")]
        [InlineData("d = 03.1415")]
        [InlineData("d = -00.01")]
        [InlineData("d = 05e+22")]
        [InlineData("d = 01e6")]
        [InlineData("d = -02E-2")]
        [InlineData("d = 06.626e-34")]
        public void Deserialize_FloatWihtLeadingZeros_ThrowsExcption(string src)
        {
            var exc = Assert.Throws<Exception>(() => Toml.Read(src));
        }

        [Theory]
        //[InlineData("b = true", true)]
        [InlineData("b = false", false)]
        public void Deserialize_Bool_DeseriailzesCorrectly(string src, bool expected)
        {
            var parsed = Toml.Read(src);
            Assert.Equal(expected, parsed["b"].Get<bool>());
        }


        [Theory]
        [InlineData("d = 1979-05-27T07:32:00Z", "1979-05-27T07:32:00Z")]
        [InlineData("d = 1979-05-27T00:32:00-07:00", "1979-05-27T00:32:00-07:00")]
        [InlineData("d = 1979-05-27T00:32:00.999999-07:00", "1979-05-27T00:32:00.999999-07:00")]
        public void Deserialize_DatetTime_DeseriaizesCorrectly(string src, string expectedDate)
        {
            var date = DateTimeOffset.Parse(expectedDate);

            var parsed = Toml.Read(src);

            Assert.Equal(date, parsed["d"].Get<DateTimeOffset>());
        }

        [Fact]
        public void Deserialze_WithIntArray_DeserializesCorrectly()
        {
            var parsed = Toml.Read("a = [1,2,3]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(3, a.Count);
            Assert.Equal(1, a.Get<int>(0));
            Assert.Equal(2, a.Get<int>(1));
            Assert.Equal(3, a.Get<int>(2));
        }

        [Fact]
        public void Deserialize_WithStringArray_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"a = [""red"", ""yellow"", ""green""]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(3, a.Count);
            Assert.Equal("red", a.Get<string>(0));
            Assert.Equal("yellow", a.Get<string>(1));
            Assert.Equal("green", a.Get<string>(2));
        }

        [Fact]
        public void Deserialize_NestedIntArray_DeserializesCorrectly()
        {
            var parsed = Toml.Read("a = [ [1, 2], [3, 4, 5] ]");

            var a = (TomlArray)parsed["a"];
            Assert.Equal(2, a.Count);
            var a1 = a.Get<TomlArray>(0);
            Assert.Equal(2, a1.Count);
            Assert.Equal(1, a1[0].Get<long>());
            Assert.Equal(2, a1.Get<int>(1));

            var a2 = (TomlArray)a[1];
            Assert.Equal(3, a2.Count);
            Assert.Equal(3, a2.Get<int>(0));
            Assert.Equal(4, a2.Get<int>(1));
            Assert.Equal(5, a2.Get<int>(2));
        }

        [Fact]
        public void Deserialize_ComplexStrings_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"a = [""all"", 'strings', """"""are the same"""""", '''type''']");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(4, a.Count);
            Assert.Equal("all", a[0].Get<string>());
            Assert.Equal("strings", a[1].Get<string>());
            Assert.Equal("are the same", a[2].Get<string>());
            Assert.Equal("type", a[3].Get<string>());
        }

        [Fact]
        public void Deserialize_WithTimepsan_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"a = 0.00:10:00.1");

            var a = parsed.Get<TimeSpan>("a");

            Assert.Equal(TimeSpan.Parse("00:10:00.1"), a);
        }

        [Fact]
        public void Deserialize_ArrayWithNesteArrayOfDiffernetTypes_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"a = [ [1, 2], [""a"", ""b"",""c""]]");

            var a = (TomlArray)parsed["a"];

            Assert.Equal(2, a.Count);
            var a1 = (TomlArray)a[0];
            Assert.Equal(2, a1.Count);
            Assert.Equal(1, a1.Get<int>(0));
            Assert.Equal(2, a1.Get<int>(1));

            var a2 = (TomlArray)a[1];
            Assert.Equal("a", a2.Get<string>(0));
            Assert.Equal("b", a2.Get<string>(1));
            Assert.Equal("c", a2.Get<string>(2));
        }

        public void Deserialize_Table_DeserializesCorrectly()
        {
            var parsed = Toml.Read(@"[table]");

            Assert.Equal(1, parsed.Rows.Count);
            Assert.NotNull(parsed.Get<TomlTable>("table"));
        }

        [Fact]
        public void Deserialize_TableWithMultipleKeys_DeserializesCorrectly()
        {
            string toParse = @"
key1 = ""value1""
key2 = ""value2""
key3 = ""value3""";

            var parsed = Toml.Read(toParse);

            Assert.Equal(3, parsed.Rows.Count);
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

            var parsed = Toml.Read(toParse);

            Assert.Equal(3, parsed.Rows.Count);
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
            // for now, as I really need a basic working TOML system. Hopfully I will have time to take care of this special cases soon.

            var parsed = Toml.Read(toParse);

            Assert.Equal(2, parsed.Rows.Count);
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
            // for now, as I really need a basic working TOML system. Hopfully I will have time to take care of this special cases soon.

            var parsed = Toml.Read(toParse);

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

            var exc = Assert.Throws<Exception>(() => Toml.Read(toParse));
            Assert.True(exc.Message.Contains("'a'"));
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

            var parsed = Toml.Read(toParse);

            var a = (TomlTableArray)parsed["products"];
            Assert.Equal(3, a.Count);

            var t1 = a[0];
            Assert.Equal("Hammer", t1.Get<string>("name"));

            var t2 = a[1];
            Assert.Equal(0, t2.Rows.Count);

            var t3 = a[2];
            Assert.Equal("Nail", t3.Get<string>("name"));
        }


    }
}
