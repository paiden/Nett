using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    public partial class TomlObjectFactoryTests
    {

        [Fact]
        public void CreateAttached_WhenTomlIntIsPassed_ThrowsInvalidOperation()
        {
            // Arrange
            var tbl = Toml.Create();
            var ti = tbl.Add("X", 1);

            // Act
            Action a = () => this.rootSource.CreateAttached(ti);

            // Assert
            a.ShouldThrow<ArgumentException>(
                because: "It is not allowed to convert TOML objects to new attached TOML tables");
        }

        [Fact]
        public void CreateAttached_WhenTomlTableIsPassed_ThrowsInvalidOperation()
        {
            Action a = () => this.rootSource.CreateAttached(Toml.Create());

            a.ShouldThrow<ArgumentException>(
                because: "It is not allowed to convert TOML objects to new attached TOML tables");
        }

        [Fact]
        public void CreateAttached_WhenTomlTableInListIsPassed_ThrowsInvalidOperation()
        {
            Action a = () => this.rootSource.CreateAttached(new List<object>() { Toml.Create() });

            a.ShouldThrow<InvalidOperationException>(
                because: "It is not allowed to convert TOML objects to new attached TOML tables");
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

        [Fact]
        public void CreateAttached_WithFooClass_CreatesTable()
        {
            // Act
            var tbl = this.rootSource.CreateAttached(FooClass.Foo1);

            // Assert
            FooClass.Foo1.AssertIs(tbl);
        }

        [Fact]
        public void CreateAttached_WithFooStruct_CreatesTable()
        {
            // Act
            var tbl = this.rootSource.CreateAttached(FooStruct.Foo1);

            // Assert
            FooStruct.Foo1.AssertIs(tbl);
        }

        [Fact]
        public void CreateAttached_WithFooDict_CreatesTable()
        {
            // Act
            var tbl = this.rootSource.CreateAttached(FooDict.Dict2);

            // Assert
            FooDict.Dict2.AssertIs(tbl);
        }

        [Fact]
        public void CreateAttached_WithFooClassList_CreatesTableArray()
        {
            // Act
            var newArr = this.rootSource.CreateAttached(FooClassList.List1);

            // Assert
            FooClassList.List1.AssertIs(newArr);
        }

        [Fact]
        public void CreateAttached_WithFooStructList_CreatesTableArray()
        {
            // Act
            var newArr = this.rootSource.CreateAttached(FooStructList.List1);

            // Assert
            FooStructList.List1.AssertIs(newArr);
        }
    }
}
