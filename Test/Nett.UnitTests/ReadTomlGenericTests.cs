using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class ReadTomlGenericTests
    {
        [Fact]
        public void ReadTomlGeneric_WithArray_CanReadArray()
        {
            // Arrange
            var toml = @"
StringArray = [""A"", ""B"", ""C""]
IntArray = [1, 2, 3]
StringList = [""A"", ""B"", ""C""]
IntList = [1, 2, 3]";

            // Act
            var read = Toml.Read<WithArray>(toml);

            // Assert
            Assert.NotNull(read);
            Assert.NotNull(read.StringList);
            Assert.Equal(3, read.StringList.Count);
            Assert.NotNull(read.IntList);
            Assert.Equal(3, read.IntList.Count);
            Assert.NotNull(read.StringArray);
            Assert.Equal(3, read.StringArray.Length);
            Assert.NotNull(read.IntArray);
            Assert.Equal(3, read.IntArray.Length);
        }

        private class WithArray
        {
            public List<string> StringList { get; set; }

            public List<int> IntList { get; set; }

            public string[] StringArray { get; set; }

            public int[] IntArray { get; set; }
        }
    }
}
