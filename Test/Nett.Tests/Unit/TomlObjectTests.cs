using System.ComponentModel;
using FluentAssertions;
using Nett.Tests.Util;
using Xunit;

namespace Nett.Tests.Unit
{
    public sealed class TomlObjectTests
    {
        [Theory]
        [InlineData("1", "1")]
        [InlineData("0xAB", "0xab")]
        [InlineData("0b0101", "0b101")]
        [InlineData("0o00734", "0o734")]
        [InlineData("1.0", "1.0")]
        [InlineData("-inf", "-inf")]
        [InlineData("+inf", "+inf")]
        [InlineData("inf", "+inf")]
        [InlineData("-nan", "nan")]
        [InlineData("nan", "nan")]
        [InlineData("+nan", "nan")]
        [InlineData("\"abc\"", "abc")]
        [InlineData("\"\"\"abc\"\"\"", "abc")]
        [InlineData("[1,     2]", "[1, 2]")]
        [InlineData("[1.0, 2.0  ]", "[1.0, 2.0]")]
        [InlineData("true", "true")]
        [InlineData("false", "false")]
        [InlineData("1979-05-27T00:32:00.999999-07:00", "1979-05-27T00:32:00.999999-07:00")]
        [InlineData("1979-05-27 07:32:00Z", "1979-05-27 07:32:00Z")]
        [InlineData("1979-05-27T00:32:00.999999", "1979-05-27T00:32:00.999999")]
        [InlineData("1979-05-27", "1979-05-27")]
        [InlineData("00:32:00.999999", "00:32:00.999999")]
        [InlineData("{x = 1,    y = 2     }", "{ x = 1, y = 2 }")]
        [InlineData("1d2h3m4s5ms", "1d2h3m4s5ms")]
        public void ToString_WithValues_ProducesCorrectOutput(string val, string expected)
        {
            // Arrange
            string tml = $"x = {val}";
            var i = Toml.ReadString(tml);

            // Act
            var r = i["x"].ToString();

            // Assert
            r.Should().Be(expected);
        }

        [Fact]
        [Description("When a subtable invokes ToString it is the root for this ToString operation. All parent information is " +
            "not available. So the subtable key in the result string was changed from 't.s' to 's' only.")]
        public void ToString_WithTables_RemovesParentTableKeyPrefix()
        {
            // Arrange
            const string input = @"
x = 1
y = 2
[t]
z = 100
[t.s]
h = 1
";

            const string expectedSub = @"
z = 100
[s]
h = 1
";

            var r = Toml.ReadString(input);

            // Act
            var t1 = r.ToString();
            var t2 = r["t"].ToString();

            // Assert
            t1.ShouldBeSemanticallyEquivalentTo(input);
            t2.ShouldBeSemanticallyEquivalentTo(expectedSub);
        }

        [Fact]
        [Description("Again the object itself it does not have a key so int the case of table arrays the key is replaced with" +
            "a placeholder key.")]
        public void ToString_WithTableArraysAtRoot_ReplacesRealKeyWithPlaceholderKey()
        {
            // Arrange
            const string input = @"
[[x]]
a = 1
[[x]]
a = 2
";

            const string expectedSub = @"
[[_]]
a = 1
[[_]]
a = 2
";

            var r = Toml.ReadString(input);

            // Act
            var t1 = r.ToString();
            var t2 = r.Get("x").ToString();
            var t3 = r.Get<TomlTableArray>("x")[1].ToString();

            // Assert
            t1.ShouldBeSemanticallyEquivalentTo(input);
            t2.ShouldBeSemanticallyEquivalentTo(expectedSub);
            t3.ShouldBeSemanticallyEquivalentTo("a=2");
        }
    }
}
