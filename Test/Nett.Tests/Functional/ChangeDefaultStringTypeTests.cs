using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class ChangeDefaultStringTypeTests
    {
        public static IEnumerable<object[]> ChangeStringTypeData
        {
            get
            {
                const string StdCase = "C:\\\r\n'";

                yield return new object[] { TomlString.TypeOfString.Auto, StdCase, "'''C:\\\r\n''''" };

                // The ' disallows literal no matter if the user sets it as the default string type, so fallback to
                // 'closest' string type multiline literal string
                yield return new object[] { TomlString.TypeOfString.Literal, StdCase, "'''C:\\\r\n''''" };
                yield return new object[] { TomlString.TypeOfString.Literal, @"C:\'", @"'''C:\''''" };
                yield return new object[] { TomlString.TypeOfString.Multiline, StdCase, "\"\"\"C:\\\\\r\n'\"\"\"" };
                yield return new object[] { TomlString.TypeOfString.MultilineLiteral, StdCase, "'''C:\\\r\n''''" };
                yield return new object[] { TomlString.TypeOfString.Normal, StdCase, "\"C:\\\\\\r\\n'\"" };
            }
        }

        [Theory]
        [MemberData(nameof(ChangeStringTypeData))]
        public void ToToml_UsesDefaultStringTypeFromConfig(TomlString.TypeOfString t, string input, string expected)
        {
            // Act
            var settings = TomlSettings.Create(sb => sb.UseDefaultStringType(t));

            // Arrange
            var tbl = Toml.Create(settings);
            tbl.Add("X", input);

            // Assert
            var result = Toml.WriteString(tbl).Trim();
            result.Should().Be($"X = {expected}");
        }
    }
}
