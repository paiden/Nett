using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class ReadValidTomlUntypedTests
    {
        [Fact, ]
        public void ReadValidToml_EmptyArray()
        {
            // Arrange
            var toml = TomlStrings.Valid.EmptyArray;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.NotNull(read);
            Assert.Equal(1, read.Rows.Count);
            Assert.Equal(typeof(TomlArray), read.Rows["thevoid"].GetType());
            var rootArray = read.Rows["thevoid"].Get<TomlArray>();
            Assert.Equal(1, rootArray.Count);
            var subArray = rootArray.Get<TomlArray>(0);
            Assert.Equal(0, subArray.Count);
        }
    }
}
