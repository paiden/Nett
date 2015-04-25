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
            var toml = TomlStrings.Valid.ArrayEmpty;

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

        [Fact]
        public void ReadValidToml_ArrayNoSpaces()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayNoSpaces;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(1, read.Get<TomlArray>("ints").Get<int>(0));
            Assert.Equal(2, read.Get<TomlArray>("ints").Get<int>(1));
            Assert.Equal(3, read.Get<TomlArray>("ints").Get<int>(2));
        }

        [Fact]
        public void ReadValidToml_HetArray()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayHeterogenous;

            // Act
            var read = Toml.Read(toml);

            // Assert
            var a = read.Get<TomlArray>("mixed");
            Assert.NotNull(a);
            Assert.Equal(3, a.Count);

            var intArray = a.Get<TomlArray>(0);
            Assert.Equal(1, intArray.Get<int>(0));
            Assert.Equal(2, intArray.Get<int>(1));

            var stringArray = a.Get<TomlArray>(1);
            Assert.Equal("a", stringArray.Get<string>(0));
            Assert.Equal("b", stringArray.Get<string>(1));

            var doubleArray = a.Get<TomlArray>(2);
            Assert.Equal(1.1, doubleArray.Get<double>(0));
            Assert.Equal(2.1, doubleArray.Get<double>(1));
        }

        [Fact]
        public void RealValidToml_NestedArrays()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArraysNested;

            // Act
            var read = Toml.Read(toml);

            // Assert
            var a = read.Get<TomlArray>("nest");
            Assert.Equal("a", a.Get<TomlArray>(0).Get<string>(0));
            Assert.Equal("b", a.Get<TomlArray>(1).Get<string>(0));
        }

        [Fact]
        public void ReadValidToml_Arrays()
        {
            // Arrange
            var toml = TomlStrings.Valid.Arrays;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(3, read.Get<TomlArray>("ints").Get<TomlArray>().Count);
            Assert.Equal(3, read.Get<TomlArray>("floats").Get<TomlArray>().Count);
            Assert.Equal(3, read.Get<TomlArray>("strings").Get<TomlArray>().Count);
            Assert.Equal(3, read.Get<TomlArray>("dates").Get<TomlArray>().Count);
        }

        [Fact]
        public void ReadValidToml_TableArrayNested()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayNested;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(1, read.Rows.Count);
            Assert.Equal(typeof(TomlArray), read.Rows["albums"].GetType());
            var arr = read.Get<TomlArray>("albums");
            Assert.Equal(2, arr.Count);

            var t0 = arr.Get<TomlTable>(0);
            Assert.Equal("Born to Run", t0.Get<string>("name"));
            var t0s = t0.Get<TomlArray>("songs");
            Assert.Equal(2, t0s.Count);
            var s0 = t0s.Get<TomlTable>(0);
            var s1 = t0s.Get<TomlTable>(1);
            Assert.Equal("Jungleland", s0.Get<string>("name"));
            Assert.Equal("Meeting Across the River", s1.Get<string>("name"));
        }

        [Fact]
        public void ReadValidTomlUntyped_Boolean()
        {
            // Arrange
            var toml = TomlStrings.Valid.Boolean;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(true, read.Get<bool>("t"));
            Assert.Equal(false, read.Get<bool>("f"));
        }

        [Fact]
        public void ReadValidTomlUntyped_CommentsEverywere()
        {
            // Arrange
            var toml = TomlStrings.Valid.CommentsEverywhere;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(42, read.Get<TomlTable>("group").Get<int>("answer"));
            Assert.Equal(2, ((TomlTable)read["group"]).Get<int[]>("more").Length);
            Assert.Equal(42, ((TomlTable)read["group"]).Get<List<int>>("more")[0]);
            Assert.Equal(42, ((TomlTable)read["group"]).Get<int[]>("more")[0]);
        }

        [Fact]
        public void ReadValidTomlUntyped_DateTime()
        {
            // Arrange
            var toml = TomlStrings.Valid.DateTime;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(DateTime.Parse("1987-07-05T17:45:00Z"), read.Get<DateTime>("bestdayever"));
        }

        [Fact]
        public void ReadValidTomlUntyped_Empty()
        {
            // Arrange
            var toml = TomlStrings.Valid.Empty;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(0, read.Rows.Count);
        }

        [Fact]
        public void ReadValidToml_Example()
        {
            // Arrange
            var toml = TomlStrings.Valid.Example;

            // Act
            var read = Toml.Read(toml);

            // Assert
            Assert.Equal(2, read.Rows.Count);
            Assert.Equal(DateTime.Parse("1987-07-05T17:45:00Z"), read.Get<DateTime>("best-day-ever"));
            var tt = (TomlTable)read["numtheory"];
            Assert.Equal(false, tt.Get<bool>("boring"));
            var ta = (TomlArray)tt["perfection"];
            Assert.Equal(3, ta.Count);
            Assert.Equal(6, ta.Get<int>(0));
            Assert.Equal(28, ta.Get<char>(1));
            Assert.Equal(496, ta.Get<ushort>(2));
        }
    }
}
