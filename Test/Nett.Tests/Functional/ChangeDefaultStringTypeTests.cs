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

                yield return new object[] { TomlStringType.Auto, StdCase, "'''C:\\\r\n''''" };

                // The ' disallows literal no matter if the user sets it as the default string type, so fallback to
                // 'closest' string type multiline literal string
                yield return new object[] { TomlStringType.Literal, StdCase, "'''C:\\\r\n''''" };
                yield return new object[] { TomlStringType.Literal, @"C:\'", @"'''C:\''''" };
                yield return new object[] { TomlStringType.Multiline, StdCase, "\"\"\"C:\\\\\r\n'\"\"\"" };
                yield return new object[] { TomlStringType.MultilineLiteral, StdCase, "'''C:\\\r\n''''" };
                yield return new object[] { TomlStringType.Basic, StdCase, "\"C:\\\\\\r\\n'\"" };
            }
        }

        [Theory]
        [MemberData(nameof(ChangeStringTypeData))]
        public void ToToml_UsesDefaultStringTypeFromConfig(TomlStringType t, string input, string expected)
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
