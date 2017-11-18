using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nett.Tests.Util.TestData;
using Xunit;

namespace Nett.Tests
{
    /// <summary>
    /// Test for the basic test examples from https://github.com/BurntSushi/toml-test/tree/master/tests
    /// These cases handle some special cases and some document structure cases.
    /// This test can only work when the Untyped* equivalent tests are OK. These tests proof that the transformation from a
    /// generic TomlTable to some typed C# class via reflection works correctly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ReadValidTomlTypedTests
    {
        [Fact]
        public void ReadValidTomlTyped_EmptyArray()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayEmpty;

            // Act
            var read = Toml.ReadString<EmptyArray>(toml);

            // Assert
            Assert.NotNull(read);
            Assert.NotNull(read.thevoid);
            Assert.Single(read.thevoid);
            Assert.Empty(read.thevoid[0]);
        }

        [Fact]
        public void ReadValidTomlTyped_ArrayNoSpaces()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArrayNoSpaces;

            // Act
            var read = Toml.ReadString<ArrayNoSpaces>(toml);

            // Assert
            Assert.Equal(3, read.ints.Length);
            Assert.Equal(1, read.ints[0]);
            Assert.Equal(2, read.ints[1]);
            Assert.Equal(3, read.ints[2]);
        }

        [Fact]
        public void ReadValidTomlTyped_ArraysNested()
        {
            // Arrange
            var toml = TomlStrings.Valid.ArraysNested;

            // Act
            var read = Toml.ReadString<ArraysNested>(toml);

            // Assert
            Assert.Equal(2, read.nest.Count);
            Assert.Equal("a", read.nest[0][0]);
            Assert.Equal("b", read.nest[1][0]);
        }

        [Fact]
        public void ReadValidToml_TableArrayNested()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayNested;

            // Act
            var read = Toml.ReadString<AlbumsRoot>(toml);

            // Assert
            Assert.NotNull(read);
            Assert.Equal(2, read.albums.Count);
            Assert.Equal("Born to Run", read.albums[0].name);
            var a0 = read.albums[0];
            Assert.Equal("Jungleland", a0.songs[0].name);
            Assert.Equal("Meeting Across the River", a0.songs[1].name);

            Assert.Equal("Born in the USA", read.albums[1].name);
            var a1 = read.albums[1];
            Assert.Equal("Glory Days", a1.songs[0].name);
            Assert.Equal("Dancing in the Dark", a1.songs[1].name);
        }

        private class AlbumsRoot
        {
            public List<Album> albums { get; set; }
        }

        private class Album
        {
            public string name { get; set; }
            public Song[] songs { get; set; }
        }

        private class Song
        {
            public string name { get; set; }
        }

        private class EmptyArray
        {
            public List<List<double>> thevoid { get; set; }
        }

        private class ArrayNoSpaces
        {
            public int[] ints { get; set; }
        }

        private class ArraysNested
        {
            public List<string[]> nest { get; set; }
        }
    }
}
