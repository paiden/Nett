using System;
using Xunit;
using Xunit.Extensions;

namespace Nett.UnitTests
{
    public class StringTomlSerializerTests
    {
        [Theory]
        [InlineData("Key = 0", "Key", 0)]
        [InlineData("Key = +0", "Key", 0)]
        [InlineData("Key = -0", "Key", 0)]
        [InlineData("Key = 1", "Key", 1)]
        [InlineData("Key = +1", "Key", 1)]
        [InlineData("Key = -1", "Key", -1)]
        public void Deserialize_SimpleyKeyValuePair_ProducesCorrectObject(string toParse, string key, object expectedValue)
        {
            var parsed = StringTomlSerializer.Deserialize(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
            Assert.Equal((long)(int)expectedValue, (long)parsed[key]);
        }

        [Fact]
        public void Deserialize_WithMultipleSignBeforeNumber_FailsToParse()
        {
            Assert.Throws<Exception>(() => StringTomlSerializer.Deserialize("key = --0"));
        }

        [Theory]
        [InlineData("Key = 1_000", "Key", 1000)]
        [InlineData("Key = 1_2_3_4", "Key", 1234)]
        public void Deserialize_IntWithUnderscoreSeperator_ProducesCorrectObject(string toParse, string key, object expectedValue)
        {
            var parsed = StringTomlSerializer.Deserialize(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(expectedValue, parsed.Get<int>(key));
            Assert.Equal((long)(int)expectedValue, (long)parsed[key]);
        }

        [Theory]
        [InlineData("Key = 01")]
        [InlineData("Key = 0_1")]
        public void Deserilaize_IntWihtLeadingZeros_Fails(string toParse)
        {
            Assert.Throws<Exception>(() => StringTomlSerializer.Deserialize(toParse));
        }

        [Fact]
        public void Deserilize_WithComments_ParserCanHandleCommentsCorrectly()
        {
            string toParse = "# I am a comment. Hear me roar. Roar." + Environment.NewLine +
                             "key = 0 # Yeah, you can do this. ";

            var parsed = StringTomlSerializer.Deserialize(toParse);

            Assert.NotNull(parsed);
            Assert.Equal(0, parsed.Get<int>("key"));
        }

        [Theory]
        [InlineData(@"str = ""I'm a string. \""You can quote me\"". Name\tJos\u00E9\nLocation\tSF.""", "str", "I'm a string. \"You can quote me\". Name\tJos\u00E9\nLocation\tSF.")]
        public void Deserialize_SingleLineSringValue_DeserializesCorrectly(string toParse, string key, string expected)
        {
            var parsed = StringTomlSerializer.Deserialize(toParse);

            Assert.Equal(expected, parsed.Get<string>(key));
        }

        [Fact]
        public void Deseriailze_SingleLineStringWithNewLine_FailsToDeserialize()
        {
            Assert.Throws<Exception>(() => StringTomlSerializer.Deserialize("str = \"\r\n\""));
        }

        [Fact]
        public void Deserilize_StringWithUTF32Char_DeserializesCorrectly()
        {
            var parsed = StringTomlSerializer.Deserialize(@"str = ""\u0000004D""");

            Assert.Equal("\u0000004D", parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData("str = \"\"\"\"\"\"", "")]
        [InlineData("str = \"\"\" \"\"\"", " ")]
        [InlineData("str = \"\"\"\r\n\"\"\"", "")]
        [InlineData("str = \"\"\"\r\nHello\r\nYou  \"\"\"", "Hello\r\nYou  ")]
        public void Deserialize_MString_ProducesCorrectresult(string src, string expected)
        {
            var parsed = StringTomlSerializer.Deserialize(src);

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
            var parsed = StringTomlSerializer.Deserialize(src);

            Assert.Equal("The quick brown fox jumps over the lazy dog.", parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData(@"str = 'C:\Users\nodejs\templates'", @"C:\Users\nodejs\templates")]
        [InlineData(@"str = '\\ServerX\admin$\system32\'", @"\\ServerX\admin$\system32\")]
        [InlineData(@"str = 'Tom ""Dubs"" Preston-Werner'", @"Tom ""Dubs"" Preston-Werner")]
        [InlineData(@"str = '<\i\c*\s*>'", @"<\i\c*\s*>")]
        public void Deserialize_LiteralString_ProducesCorrectResult(string src, string expected)
        {
            var parsed = StringTomlSerializer.Deserialize(src);

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
            var parsed = StringTomlSerializer.Deserialize(src);

            Assert.Equal(expected, parsed.Get<string>("str"));
        }

        [Theory]
        [InlineData("d = +1.0", 1.0)]
        [InlineData("d = 3.1415", 3.1415)]
        [InlineData("d = -0.01", -0.01)]
        [InlineData("d = 5e+22", 5e+22)]
        [InlineData("d = 1e6", 1e6)]
        [InlineData("d = -2E-2", -2E-2)]
        [InlineData("d = 6.626e-34", 6.626e-34)]
        public void Deserialize_FloatKeyValuePair_ProducesCorrectResult(string src, double expected)
        {
            var parsed = StringTomlSerializer.Deserialize(src);

            Assert.Equal(expected, parsed.Get<double>("d"));
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
            var exc = Assert.Throws<Exception>(() => StringTomlSerializer.Deserialize(src));
            Assert.Equal("Leading zeros are not allowed.", exc.Message);
        }

    }
}
