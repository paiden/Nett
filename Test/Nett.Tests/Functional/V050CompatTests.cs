using System;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

#pragma warning disable S4144 // Methods should not have identical implementations

namespace Nett.Tests.Functional
{
    public sealed class V050CompatTests
    {
        [Fact]
        public void ReadToml_WithDottedKeys_CanReadThatStuff()
        {
            // Arrange
            const string TomlInput = @"
name = ""Orange""
physical.color = ""orange""
physical.shape = ""round""
site.""google.com"" = true
";
            // Act
            var read = Toml.ReadString(TomlInput);

            // Assert
            read.Get<string>("name").Should().Be("Orange");
            read.Get<TomlTable>("physical").Get<string>("color").Should().Be("orange");
            read.Get<TomlTable>("physical").Get<string>("shape").Should().Be("round");
            read.Get<TomlTable>("site").Get<bool>("google.com").Should().Be(true);
        }

        [Theory]
        [InlineData("x=12", 12)]
        [InlineData("x=0xAF", 0xAF)]
        [InlineData("x=0o77", 0x3F)]
        [InlineData("x=0b10", 0b10)]
        public void Read_CanReadAllIntTypes(string input, int expected)
        {
            // Act
            var read = Toml.ReadString(input);

            // Assert
            read.Get<int>("x").Should().Be(expected);
        }

        [Theory]
        [InlineData("x=12")]
        [InlineData("x=0xAF")]
        [InlineData("x=0o77")]
        [InlineData("x=0b10")]
        public void Write_WhenIntIsReadWithSomeType_ItAlsoGetsWrittenBackWithTheSameType(string tml)
        {
            // Arrange
            var read = Toml.ReadString(tml);

            // Act
            var written = Toml.WriteString(read);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo(tml);
        }

        [Theory]
        [InlineData("x=+inf", double.PositiveInfinity)]
        [InlineData("x=-inf", double.NegativeInfinity)]
        [InlineData("x=inf", double.PositiveInfinity)]
        [InlineData("x=nan", double.NaN)]
        [InlineData("x=-nan", double.NaN)]
        [InlineData("x=+nan", double.NaN)]
        public void Read_CanHandleSpecialFloats(string input, double expected)
        {
            // Act
            var read = Toml.ReadString(input);

            // Assert
            read.Get<double>("x").Should().Be(expected);
        }

        public static TheoryData<string, Type> ReadCorrectDateTimeTypeData
        {
            get
            {
                var data = new TheoryData<string, Type>();

                data.Add("x=1979-05-27T07:32:00Z", typeof(TomlOffsetDateTime));
                data.Add("x=1979-05-27T00:32:00-07:00", typeof(TomlOffsetDateTime));
                data.Add("x=1979-05-27T00:32:00.999999-07:00", typeof(TomlOffsetDateTime));
                data.Add("x=1979-05-27T00:32:00.999999", typeof(TomlLocalDateTime));
                data.Add("x=1979-05-27T07:32:00", typeof(TomlLocalDateTime));
                data.Add("x=07:32:00", typeof(TomlLocalTime));
                data.Add("x=00:32:00.999999", typeof(TomlLocalTime));
                data.Add("x=1979-05-27", typeof(TomlLocalDate));

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(ReadCorrectDateTimeTypeData))]
        public void ReadDateTime_ReadsCorrecTypeOfDateTime(string tml, Type expected)
        {
            // Act
            var r = Toml.ReadString(tml);

            // Assert
            r.Get("x").Should().BeOfType(expected);
        }

        [Theory]
        [InlineData("x=1979-05-27T07:32:00Z", "x=1979-05-27T07:32:00Z")]
        [InlineData("x=1979-05-27T00:32:00-07:00", "x=1979-05-27T00:32:00-07:00")]
        [InlineData("x=1979-05-27T00:32:00.999999-07:00", "x=1979-05-27T00:32:00.999999-07:00")]
        [InlineData("x=1979-05-27T07:32:00+00:00", "x=1979-05-27T07:32:00+00:00")]
        [InlineData("x=07:32:00", "x=07:32:00")]
        [InlineData("x=00:32:00.999999", "x=00:32:00.999999")]
        [InlineData("x=1979-05-27", "x=1979-05-27")]
        public void ReadWriteCycle_WhenReadWithSpecificFormat_WritesAgainWithSameFormat(string tml, string expected)
        {
            // Arrange
            var read = Toml.ReadString(tml);

            // Act
            var written = Toml.WriteString(read);

            // Assert
            written.ShouldBeSemanticallyEquivalentTo(expected);
        }

        [Fact]
        public void CanRadTomlWithAccidentalWhiteSpaceInMultiLineBasicStrings()
        {
            // Arrange (string contains whitespaces at end of line! Test is useless without them!)
            //            const string tml = @"x=""""""
            //l1 \   
            //l2\   
            //l3""""""";

            const string tml = @"x=""""""\
  \
  \ 
""""""";

            // Act
            var read = Toml.ReadString(tml);

            // Assert
            read.Get<string>("x").Should().Be("");
        }

        [Fact]
        public void Api_StandardFileExtensionIsToml()
        {
            Toml.FileExtension.Should().Be(".toml");
        }
    }
}
