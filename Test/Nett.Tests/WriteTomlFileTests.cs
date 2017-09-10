using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Nett.Tests
{
    [ExcludeFromCodeCoverage]
    public class WriteTomlFileTests : IDisposable
    {
        private string fn;
        public WriteTomlFileTests()
        {
            fn = Guid.NewGuid().ToString() + ".toml";
        }

        public void Dispose()
        {
            try
            {
                File.Delete(this.fn);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }

        public void WriteFile_WithDefaultMergecommentsMode_CanWriteFileWhenTargetFileDoesntExistYet()
        {
            // Arrange
            Action a = () => Toml.WriteFile(new Foo(), this.fn);

            // Act + Assert
            a.ShouldNotThrow();
        }

        [Fact]
        public void WriteFile_WritesFile()
        {
            // Arrange
            var foo = new Foo() { X = 1 };

            // Act
            Toml.WriteFile(foo, fn);

            // Assert
            File.Exists(this.fn).Should().Be(true);
            var read = Toml.ReadFile<Foo>(this.fn);
            read.X.Should().Be(1);
        }

        [Fact(DisplayName = "Ensure writing floats works culture invariant (file)")]
        public void WriteFile_WithFloat_WritesCultureInvariant()
        {
            // Arrange
            var tt = Toml.Create();
            tt.Add("TheFloat", 1.2);

            // Act
            Toml.WriteFile(tt, this.fn);

            // Assert
            var s = File.ReadAllText(this.fn);
            s.Should().Be("TheFloat = 1.2\r\n");
        }

        private class Foo
        {
            public int X { get; set; }
        }
    }
}
