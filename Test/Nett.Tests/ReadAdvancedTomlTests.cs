using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class ReadAdvancedTomlTests
    {
        [Fact]
        public void ReadAdvancedToml_WithArray()
        {
            // Arrange
            var toml = @"
StringArray = [""A"", ""B"", ""C""]
IntArray = [1, 2, 3]
StringList = [""A"", ""B"", ""C""]
IntList = [1, 2, 3]";

            // Act
            var read = Toml.ReadString<WithArray>(toml);

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


        [Fact]
        public void ReadAdvancedToml_TableArrayAfterOtherItems()
        {
            string toParse = @"
X = 0
[Tbl]
    [[Tbl.Array]]
        A = ""test""
        B = [ ""SA"", ""SB"" ]
";

            var read = Toml.ReadString<Root>(toParse);

            Assert.NotNull(read);
            Assert.NotNull(read.Tbl);
            Assert.NotNull(read.Tbl.Array);
            Assert.Single(read.Tbl.Array);
            var r = read.Tbl.Array[0];
            Assert.Equal("test", r.A);
            Assert.NotNull(r.B);
            Assert.Equal(2, r.B.Length);
            Assert.Equal("SA", r.B[0]);
            Assert.Equal("SB", r.B[1]);
        }



        [Fact]
        public void ReadAdvancedToml_DbConfig()
        {
            string toParse = @"
[database]
server = ""192.168.1.1""
ports = [8001, 8001, 8002]
";

            var read = Toml.ReadString<DbConfig>(toParse);

        }

        public class Root
        {
            public int X { get; set; }

            public Tbl Tbl { get; set; }

        }

        public class DbConfig
        {

        }

        public class DataBase
        {
            public string Server { get; set; }

            public int[] Ports { get; set; }
        }

        public class Tbl
        {
            public List<ArrayType> Array { get; set; }
        }

        public class ArrayType
        {
            public string A { get; set; }
            public string[] B { get; set; }
        }
    }
}
