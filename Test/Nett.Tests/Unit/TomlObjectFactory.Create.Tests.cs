using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    public sealed class TomlObjectFactoryCreateTests
    {
        private readonly TomlTable rootSource;

        public TomlObjectFactoryCreateTests()
        {
            this.rootSource = Toml.Create();
        }

        [Fact]
        public void CreateAttached_WhenTomlObjectIsPassed_ThrowsInvalidOperation()
        {
            Action a = () => this.rootSource.CreateAttached(Toml.Create());

            a.ShouldThrow<InvalidOperationException>(
                because: "It is not allowed to convert TOML objects to new attached TOML tables");
        }

        [Fact]
        public void CreateAttached_WhenClassTypeIsPassed_CreateCorrespondingTable()
        {
            // Act
            var tbl = this.rootSource.CreateAttached(new Foo());

            // Assert
            tbl.Rows.Count().Should().Be(1);
            tbl.Get<int>("X").Should().Be(1);
        }

        [Fact]
        public void CreateAttached_WhenValueTypeIsPassed_CreateCorrespondingTable()
        {
            // Act
            var tbl = this.rootSource.CreateAttached(new FooStruct() { X = 1 });

            // Assert
            tbl.Rows.Count().Should().Be(1);
            tbl.Get<int>("X").Should().Be(1);
        }

        [Fact]
        public void CreateAttachedEmptyArray_CreatesEmptyTomlArray()
        {
            // Act
            var a = this.rootSource.CreateEmptyAttachedArray();

            // Assert
            a.Items.Count().Should().Be(0);
        }

        [Fact]
        public void CreateAttachedEmptyTableArray_CretesEmptyTableArray()
        {
            // Act
            var a = this.rootSource.CreateEmptyAttachedTableArray();

            // Assert
            a.Items.Count.Should().Be(0);
        }

        [Fact]
        public void CreateAttachedEmptyTable_CretesEmptyTable()
        {
            // Act
            var a = this.rootSource.CreateEmptyAttachedTable();

            // Assert
            a.Rows.Count().Should().Be(0);
        }

        private struct FooStruct
        {
            private int x;
            public int X { get => this.x; set => this.x = value; }
        }

        private class Foo
        {
            public int X { get; set; } = 1;

            public override bool Equals(object obj) => ((Foo)obj).X == this.X;

            public override int GetHashCode() => this.X;
        }
    }
}
