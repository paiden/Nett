using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Unit
{
    public sealed partial class TomlObjectFactoryTests
    {
        private static readonly TimeSpan Ts = TimeSpan.FromSeconds(1.0);
        private static readonly DateTimeOffset Dto = DateTimeOffset.UtcNow;

        public static IEnumerable<object[]> AddArrayTestData
        {
            get
            {
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<bool>)(new bool[] { true, false }))), new bool[] { true, false } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<string>)(new string[] { "X", "Y" }))), new string[] { "X", "Y" } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<long>)(new long[] { 1L, 0L }))), new long[] { 1L, 0L } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<int>)(new int[] { 1, 0 }))), new long[] { 1, 0 } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<double>)(new double[] { 1.0, 0.0 }))), new double[] { 1.0, 0.0 } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<float>)(new float[] { 1.0f, 0.0f }))), new float[] { 1.0f, 0.0f } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<DateTimeOffset>)(new DateTimeOffset[] { Dto, Dto }))), new DateTimeOffset[] { Dto, Dto } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<DateTime>)(new DateTime[] { Dto.UtcDateTime, Dto.UtcDateTime }))), new DateTime[] { Dto.UtcDateTime, Dto.UtcDateTime } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (IEnumerable<TimeSpan>)(new TimeSpan[] { Ts, Ts }))), new TimeSpan[] { Ts, Ts } };
            }
        }

        [MemberData(nameof(AddArrayTestData))]
        [Theory]
        public void AddArrayExtensions_AddsObjectToTable(Action<string, TomlTable> adder, object expected)
        {
            var tbl = Toml.Create();

            adder("x", tbl);

            var x = tbl["x"].Get(expected.GetType());
            ((IEnumerable)x).Should().Equal((IEnumerable)expected);
        }

        public static IEnumerable<object[]> AddValueTestData
        {
            get
            {
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, true)), true };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, "x")), "x" };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, 1L)), 1L };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (long)1)), 1L };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, 1.0)), 1.0 };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, 1.0f)), 1.0 };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, Dto)), Dto };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, Ts)), Ts };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.Add(k, (char)1)), 1L };
            }
        }

        [MemberData(nameof(AddValueTestData))]
        [Theory]
        public void AddValue_AddsTheGivenObjectToTheTable(Action<string, TomlTable> adder, object expected)
        {
            var tbl = Toml.Create();

            adder("x", tbl);

            var x = tbl["x"].Get(expected.GetType());
            x.Should().Be(expected);
        }

        [Fact]
        public void AddTable_WhenNoArgumentsPassed_AddsEmptyTable()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.Add("x", Toml.Create());

            tbl["x"].Should().BeOfType<TomlTable>()
                .Which.Count.Should().Be(0);
        }


        [Fact]
        public void Add_WithFooClass_AddsTable()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var newTbl = tbl.Add("x", FooClass.Foo1);

            // Assert
            FooClass.Foo1.AssertIs(tbl["x"]);
            FooClass.Foo1.AssertIs(newTbl);
        }

        [Fact]
        public void Add_WithFooDict_AddsTable()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var newTbl = tbl.Add("x", FooDict.Dict1);

            // Assert
            FooDict.Dict1.AssertIs(tbl["x"]);
            FooDict.Dict1.AssertIs(newTbl);
        }


        [Fact]
        public void Add_WithFooStruct_AddsTable()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var newTbl = tbl.Add("x", FooStruct.Foo1);

            // Assert
            FooStruct.Foo1.AssertIs(tbl["x"]);
            FooStruct.Foo1.AssertIs(newTbl);
        }

        [Fact]
        public void Add_WithFooClassListSource_AddsTableArray()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var newTbl = tbl.Add("x", FooClassList.List1);

            // Assert
            FooClassList.List1.AssertIs(newTbl);
            FooClassList.List1.AssertIs(tbl["x"]);
        }

        [Fact]
        public void Add_WithFooStructListSource_AddsTableArray()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            var newTbl = tbl.Add("x", FooClassList.List1);

            // Assert
            FooClassList.List1.AssertIs(newTbl);
        }


        [Fact]
        public void AddTableArray_WhenNoArgsPassed_AddsEmptyTableArray()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.Add("x", tbl.CreateEmptyAttachedTableArray());

            tbl["x"].Should().BeOfType<TomlTableArray>()
                .Which.Count.Should().Be(0);
        }

        [Fact]
        public void AddTableArray_WhenEmptyTableArrayPassed_AddsEmptyTableArray()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.Add("x", new List<object>());

            tbl["x"].Should().BeOfType<TomlTableArray>()
                .Which.Count.Should().Be(0);
        }

        [Fact]
        public void AddTableArray_WhenTableArrayBelongsToDifferentRoot_ANewArrayIsCreatedForTheObjectGraph()
        {
            var tbl = Toml.Create();
            var diffRoot = Toml.Create().CreateEmptyAttachedTableArray();

            var newArray = tbl.Add("x", diffRoot);

            newArray.Should().NotBeSameAs(diffRoot);
        }

        [Fact]
        public void CreateArray_WhenNoArgumentGetsPassed_CretesEmptyArray()
        {
            var tbl = Toml.Create();

            var a = tbl.CreateEmptyAttachedArray();

            a.Length.Should().Be(0);
        }

        [Fact]
        public void CreateTableFromClass_WhenGivenObjectIsTomlObject_ThrowsInvalidOp()
        {
            // Arrange
            var tbl = Toml.Create();

            // Act
            Action a = () => tbl.CreateAttached(Toml.Create());

            // Assert
            a.ShouldThrow<ArgumentException>();
        }

        private class TestStruct
        {
            public int Y { get; set; }
        }
    }
}
