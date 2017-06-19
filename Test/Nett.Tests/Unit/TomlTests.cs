using System;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    public class TomlTests
    {
        [Fact]
        public void Create_WhenClrObjectIsATomlTable_()
        {
            var x = Toml.Create(Toml.Create());
        }

        [Fact]
        public void Create_WhenObjIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.Create(null);

            a.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void ReadFile_WhenFileNameIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile<object>((string)null);

            a.ShouldThrow<ArgumentNullException>().WithMessage("*filePath*");
        }

        [Fact]
        public void ReadFile_WhenSettingsIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile<object>("test.toml", null);

            a.ShouldThrow<ArgumentNullException>().WithMessage("*settings*");
        }

        [Fact]
        public void ReadFileTT_WhenFileNameIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile((string)null);

            a.ShouldThrow<ArgumentNullException>().WithMessage("*filePath*");
        }

        [Fact]
        public void ReadFileTT_WhenSettingsIsNull_ThrowsArgNull()
        {
            Action a = () => Toml.ReadFile("test.toml", null);

            a.ShouldThrow<ArgumentNullException>().WithMessage("*settings*");
        }
    }
}
