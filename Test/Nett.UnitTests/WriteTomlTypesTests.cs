using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

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

        public class StringType
        {
            public string S { get; set; }
        }
    }
}
