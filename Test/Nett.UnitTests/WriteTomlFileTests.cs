using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public class WriteTomlFileTests
    {
        [Fact]
        public void WriteFile_WithDefaultMergecommentsMode_CanWriteFileWhenTargetFileDoesntExistYet()
        {
            // Arrange
            string fn = Guid.NewGuid() + ".tml";
            Action a = () => Toml.WriteFile(new Foo(), fn);

            // Act + Assert
            a.ShouldNotThrow();
        }

        [Fact]
        public void WriteFile_WritesFile()
        {
            // Arrange
            var foo = new Foo() { X = 1 };

            // Act
            Toml.WriteFile(foo, "test.tml");

            // Assert
            File.Exists("test.tml").Should().Be(true);
            var read = Toml.ReadFile<Foo>("test.tml");
            read.X.Should().Be(1);
        }

        private class Foo
        {
            public int X { get; set; }
        }
    }
}
