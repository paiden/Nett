using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nett.UnitTests
{
    public class ReadValidTomlTypedTests
    {
        [Fact]
        public void ReadValidToml_TableArrayNested()
        {
            // Arrange
            var toml = TomlStrings.Valid.TableArrayNested;

            // Act
            var read = Toml.Read<AlbumsRoot>(toml);

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
    }
}
