using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public class WriteTomlTypesTests
    {
        public static IEnumerable<object[]> WriteTomlStringData
        {
            get
            {
                yield return new object[] { new StringType { S = "C:\\dir1" }, "S = \"C:\\\\dir1\"" };
                yield return new object[] { new StringType { S = "C:\\\r\nX" }, "S = \"C:\\\\\\r\\nX\"" };
                yield return new object[] { new StringType { S = @"C:\Windows\System32\BestPractices\v1.0\Models\Microsoft\Windows\Hyper-V\en-US\test.txt" }, @"S = ""C:\\Windows\\System32\\BestPractices\\v1.0\\Models\\Microsoft\\Windows\\Hyper-V\\en-US\\test.txt""" };
            }
        }

        [Theory]
        [MemberData(nameof(WriteTomlStringData))]
        public void WriteTomlStrings(StringType s, string expected)
        {
            // Act
            var written = Toml.WriteString(s);

            // Assert
            Assert.Equal(expected, written.Trim());
        }

        [Theory]
        [InlineData("02:01")]
        [InlineData("03:02:01")]
        [InlineData("4.03:02:01")]
        [InlineData("4.03:02:01.001")]
        public void WriteTimespan_WritesTheTimepspansInCultureInvariantFormatAndMinimized(string span)
        {
            var t = new TimespanType() { Ts = TimeSpan.Parse(span) };

            var written = Toml.WriteString(t);

            written.Should().Be($"Ts = {span}\r\n");
        }

        public class TimespanType
        {
            public TimeSpan Ts { get; set; }
        }

        public class StringType
        {
            public string S { get; set; }
        }
    }
}
