using FluentAssertions;
using Xunit;

namespace Nett.UnitTests
{
    public sealed class NestedTablesTests
    {
        private const string ExpectedSerForB = @"
Level=1

[A]
Level=0";

        private const string ExpectedSerForC = @"
Level= 2

[B]
Level=1

[B.A]
Level=0";

        private const string ExpectedSerForD = @"
Level = 3

[C]
Level= 2

[C.B]
Level=1

[C.B.A]
Level=0";

        [Fact(DisplayName = "Serializing B writes innermost composed key as [A]")]
        public void Write_WithB_ProducesCorrectComposedKeys()
        {
            // Arrange
            var tw = new B();

            // Act
            var s = Toml.WriteString(tw);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedSerForB);
        }

        [Fact(DisplayName = "Reading B reads the equivalent object structure")]
        public void Read_WithSerializedB_ProducesEquivalentObjectStructure()
        {
            // Act
            var b = Toml.ReadString<B>(ExpectedSerForB);

            // Assert
            b.Should().Be(new B());
        }

        [Fact(DisplayName = "Serializing C writes innermost composed key as [B.A]")]
        public void Write_WithC_ProducesCorrectComposedKeys()
        {
            // Arrange
            var tw = new C();

            // Act
            var s = Toml.WriteString(tw);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedSerForC);
        }

        [Fact(DisplayName = "Reading C reads the equivalent object structure")]
        public void Read_WithSerializedC_ProducesEquivalentObjectStructure()
        {
            // Act
            var c = Toml.ReadString<C>(ExpectedSerForC);

            // Assert
            c.Should().Be(new C());
        }

        [Fact(DisplayName = "Serializing D writes innermost composed key as [C.B.A]")]
        public void Write_WithD_ProducesCorrectComposedKeys()
        {
            // Arrange
            var tw = new D();

            // Act
            var s = Toml.WriteString(tw);

            // Assert
            s.ShouldBeSemanticallyEquivalentTo(ExpectedSerForD);
        }

        [Fact(DisplayName = "Reading D reads the equivalent object structure")]
        public void Read_WithSerializedD_ProducesEquivalentObjectStructure()
        {
            // Act
            var d = Toml.ReadString<D>(ExpectedSerForD);

            // Assert
            d.Should().Be(new D());
        }



        private class WithSubTable
        {
            public OuterTable Outer { get; set; } = new OuterTable();

            public class OuterTable
            {
                public InnerTable Inner { get; set; } = new InnerTable();
            }

            public class InnerTable
            {
                public string Propety { get; set; } = "This is the sub table";
            }
        }

        private class D
        {
            public int Level { get; set; } = 3;

            public C C { get; set; } = new C();

            public override bool Equals(object obj) => this.Level == ((D)obj).Level && this.C.Equals(((D)obj).C);
        }

        private class C
        {
            public int Level { get; set; } = 2;

            public B B { get; set; } = new B();

            public override bool Equals(object obj) => this.Level == ((C)obj).Level && this.B.Equals(((C)obj).B);
        }

        private class B
        {
            public int Level { get; set; } = 1;

            public A A { get; set; } = new A();

            public override bool Equals(object obj) => this.Level == ((B)obj).Level && this.A.Equals(((B)obj).A);
        }

        private class A
        {
            public int Level { get; set; } = 0;

            public override bool Equals(object obj) => this.Level == ((A)obj).Level;
        }
    }
}
