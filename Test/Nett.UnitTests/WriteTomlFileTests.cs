using System;
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

        private class Foo
        {
            public int X { get; set; }
        }
    }
}
