using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Unit
{
    public sealed class TomlObjectFactoryTests
    {
        private static readonly TimeSpan Ts = TimeSpan.FromSeconds(1.0);
        private static readonly DateTimeOffset Dto = DateTimeOffset.UtcNow;

        public static IEnumerable<object> AddArrayTestData
        {
            get
            {
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new bool[] { true, false })), new bool[] { true, false } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new string[] { "X", "Y" })), new string[] { "X", "Y" } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new long[] { 1L, 0L })), new long[] { 1L, 0L } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new int[] { 1, 0 })), new long[] { 1, 0 } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new double[] { 1.0, 0.0 })), new double[] { 1.0, 0.0 } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new float[] { 1.0f, 0.0f })), new float[] { 1.0f, 0.0f } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new DateTimeOffset[] { Dto, Dto })), new DateTimeOffset[] { Dto, Dto } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new DateTime[] { Dto.UtcDateTime, Dto.UtcDateTime })), new DateTime[] { Dto.UtcDateTime, Dto.UtcDateTime } };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddArray(k, new TimeSpan[] { Ts, Ts })), new TimeSpan[] { Ts, Ts } };
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

        public static IEnumerable<object> AddValueTestData
        {
            get
            {
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, true)), true };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, "x")), "x" };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, 1L)), 1L };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, 1)), 1L };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, 1.0)), 1.0 };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, 1.0f)), 1.0 };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, Dto)), Dto };
                yield return new object[] { new Action<string, TomlTable>((k, t) => t.AddValue(k, Ts)), Ts };
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

            var newTbl = tbl.AddTable("x");

            tbl["x"].Should().BeOfType<TomlTable>()
                .Which.Count.Should().Be(0);
        }


        [Fact]
        public void AddTable_WhenCLRObjectIsPassed_AddsCorrespondingTable()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.AddTableFromClass("x", new TestObj());

            tbl["x"].Should().BeOfType<TomlTable>()
                .Which.Get<int>("Y").Should().Be(1);
        }

        [Fact]
        public void AddTable_WhenEnumerablePassed_CreatesCorrespondingTable()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.AddTable("x", new List<KeyValuePair<string, TomlObject>>()
            {
                new KeyValuePair<string, TomlObject>("Y", tbl.CreateAttachedValue(1)),
            });

            tbl["x"].Should().BeOfType<TomlTable>()
                .Which.Get<int>("Y").Should().Be(1);
        }

        [Fact]
        public void AddTableArray_WhenNoArgsPassed_AddsEmptyTableArray()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.AddTableArray("x");

            tbl["x"].Should().BeOfType<TomlTableArray>()
                .Which.Count.Should().Be(0);
        }

        [Fact]
        public void AddTableArray_WhenEmptyTableArrayPassed_AddsEmptyTableArray()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.AddTableArray("x", new List<TomlTable>());

            tbl["x"].Should().BeOfType<TomlTableArray>()
                .Which.Count.Should().Be(0);
        }

        [Fact]
        public void AddTableArray_WhenTableArrayWithEmptyTablePassed_AddsCorrespondingTableArray()
        {
            var tbl = Toml.Create();

            var newTbl = tbl.AddTableArray("x", new List<TomlTable>() { tbl.CreateAttachedTable() });

            tbl["x"].Should().BeOfType<TomlTableArray>()
                .Which.Count.Should().Be(1);
        }

        [Fact]
        public void AddTableArray_WhenTableArrayBelongsToDifferentRoot_AddsCorrespondingTableArray()
        {
            var tbl = Toml.Create();
            var diffRoot = Toml.Create().CreateAttachedTableArray();

            Action a = () => tbl.AddTableArray("x", diffRoot);

            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void CreateArray_WhenNoArgumentGetsPassed_CretesEmptyArray()
        {
            var tbl = Toml.Create();

            var a = tbl.CreateAttachedArray();

            a.Length.Should().Be(0);
        }

        [Fact]
        public void CreateArray_WhenGivenValueBelgonsToDiffObjGraph_ThrowsException()
        {
            var tbl = Toml.Create();
            var diff = Toml.Create().CreateAttachedValue(true);

            Action a = () => tbl.CreateAttachedArray(new List<TomlValue>() { diff });

            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void CreateArray_WhenSingleValueGiven_CreatesCorrespondingArray()
        {
            var tbl = Toml.Create();
            var diff = Toml.Create().CreateAttachedValue(true);

            var a = tbl.CreateAttachedArray(new List<TomlValue>() { tbl.CreateAttachedValue(true) });

            a.Length.Should().Be(1);
            a[0].Get<bool>().Should().Be(true);
        }

        [Fact]
        public void CreateTableFromClass_WhenGivenObjectIsTomlObject_ThrowsInvalidOp()
        {
            var tbl = Toml.Create();

            Action a = () => tbl.CreateAttachedTableFromClass(Toml.Create());

            a.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void CreateAttachedTable_WhenEnumerableEleBelongsToDifferentGraph_TrowsInvalidOp()
        {
            var tbl = Toml.Create();
            var diff = Toml.Create().CreateAttachedValue(true);

            Action a = () => tbl.CreateAttachedTable(new List<KeyValuePair<string, TomlObject>>()
            {
                new KeyValuePair<string, TomlObject>("x", diff),
            });

            a.ShouldThrow<InvalidOperationException>();
        }

        private class TestObj
        {
            public int Y { get; set; } = 1;
        }
    }
}
