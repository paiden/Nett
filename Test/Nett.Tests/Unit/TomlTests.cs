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
    }
}
