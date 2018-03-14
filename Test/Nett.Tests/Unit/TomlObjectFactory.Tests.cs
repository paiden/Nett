using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace Nett.Tests.Unit
{
    public sealed partial class TomlObjectFactoryTests
    {
        private readonly TomlTable rootSource;
        private TomlTable updateThis;

        public TomlObjectFactoryTests()
        {
            this.rootSource = Toml.Create();

            this.updateThis = Toml.Create();
            this.updateThis.Add("x", 1);
        }

        private class FooDict : Dictionary<string, int>
        {
            public static readonly FooDict Dict1 = new FooDict(1);
            public static readonly FooDict Dict2 = new FooDict(2);

            public FooDict(int value)
            {
                this.Add("X", value);
            }

            public void AssertIs(TomlObject obj)
            {
                var w = obj.Should().BeOfType<TomlTable>().Which;
                w.Count.Should().Be(1);
                w["X"].Get<int>().Should().Be(this["X"]);
            }
        }

        private struct FooStruct
        {
            public static readonly FooStruct Foo1 = new FooStruct() { X = 2 };

            public int X { get; set; }

            public void AssertIs(TomlObject o)
            {
                var tbl = o.Should().BeOfType<TomlTable>().Which;
                tbl.Count.Should().Be(1);
                tbl.Rows.Single().Value.Get<int>().Should().Be(this.X);
            }
        }

        private class FooClass
        {
            public static readonly FooClass Foo1 = new FooClass() { X = 2 };

            public int X { get; set; } = 1;

            public override bool Equals(object obj) => ((FooClass)obj).X == this.X;

            public override int GetHashCode() => this.X;

            public void AssertIs(TomlObject o)
            {
                var tbl = o.Should().BeOfType<TomlTable>().Which;
                tbl.Count.Should().Be(1);
                tbl.Rows.Single().Value.Get<int>().Should().Be(this.X);
            }
        }

        private class FooStructList : List<FooStruct>
        {
            public static readonly FooStructList List1 = new FooStructList();

            public FooStructList()
            {
                this.Add(new FooStruct() { X = 1 });
                this.Add(new FooStruct() { X = 2 });
                this.Add(new FooStruct() { X = 3 });
            }

            public void AssertIs(TomlObject obj)
            {
                var a = obj.Should().BeOfType<TomlTableArray>().Which;

                a.Count.Should().Be(this.Count);

                for (int i = 0; i < this.Count; i++)
                {
                    a[i].Get<FooStruct>().X.Should().Be(this[i].X);
                }
            }
        }

        private class FooClassList : List<FooClass>
        {
            public static readonly FooStructList List1 = new FooStructList();

            public FooClassList()
            {
                this.Add(new FooClass() { X = 1 });
                this.Add(new FooClass() { X = 2 });
                this.Add(new FooClass() { X = 3 });
            }

            public void AssertIs(TomlObject obj)
            {
                var a = obj.Should().BeOfType<TomlArray>().Which;

                a.Length.Should().Be(this.Count);

                for (int i = 0; i < this.Count; i++)
                {
                    a[i].Get<FooStruct>().X.Should().Be(this[i].X);
                }
            }
        }
    }
}
