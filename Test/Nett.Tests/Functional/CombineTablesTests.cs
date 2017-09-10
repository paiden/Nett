using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class CombineTablesTests
    {
        private const string XKey = "XKey";
        private const string YKey = "YKey";
        private const string XVal = "XVal";
        private const string YVal = "YVal";
        private const string SameKey = "SameKey";
        private const string SubTableKey = "SubTable";
        private const string SubTableValueKey = "SubTableValue";
        private const string XSubTableVal = "SubTableValX";
        private const string YSubTableVal = "SubTableValY";

        public static readonly TomlTable X;
        public static readonly TomlTable Y;

        public static readonly TomlTable Dx;
        public static readonly TomlTable Dy;

        static CombineTablesTests()
        {
            X = Toml.Create();
            X.Add(XKey, XVal);
            X.Add(SameKey, XVal);
            var xs = X.AddTomlObject(SubTableKey, Toml.Create());
            xs.Add(SubTableValueKey, XSubTableVal);

            Y = Toml.Create();
            Y.Add(YKey, YVal);
            Y.Add(SameKey, YVal);
            var ys = Y.AddTomlObject(SubTableKey, X.CreateEmptyAttachedTable());
            ys.Add(SubTableValueKey, YSubTableVal);

            Dx = Toml.Create();
            Dx.Add("a", (long)1);
            Dx.Add("c", (long)3).AddComment("xcc");

            Dy = Toml.Create();
            Dy.Add("b", (long)2).AddComment("ybc");
            Dy.Add("c", (long)4).AddComment("ycc");
        }

        [Fact]
        public static void Overwrite_XWithYForAllSourceRows_AddsAndOverwritesXRowsWithYRows()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(X).With(Y).ForAllSourceRows());

            // Assert
            AssertYOnlyRowWasAddedTo(r);
            AssertSameKeyRowWasOverwritten(r);
            AssertSubTableRowWasOverwritten(r);
        }

        [Fact]
        public static void Overwrite_XWithYForAllTargetRows_OverwritesRowsThatExistInTarget()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(X).With(Y).ForAllTargetRows());

            // Assert
            AssertNoRowWasAdded(r);
            AssertSameKeyRowWasOverwritten(r);
            AssertSubTableRowWasOverwritten(r);
        }

        [Fact]
        public static void Overwrite_XWithYWithSourceOnlyRows_AddsTheRowThatOnlyExistedInYToResultTable()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(X).With(Y).ForRowsOnlyInSource());

            // Assert
            AssertYOnlyRowWasAddedTo(r);
            AssertSubTableNotTouched(r);
            AssertSameKeyNotTouched(r);
        }

        [Fact]
        public static void Overwrite_XWithYIncludingAllComments_OverwritesCommendsWithSourceComments()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(Dx)
                .With(Dy)
                .IncludingAllComments()
                .ForAllSourceRows());

            // Assert
            AssertCommentsAre(r["a"], new string[] { });
            AssertCommentsAre(r["b"], "ybc");
            AssertCommentsAre(r["c"], "ycc");
        }

        [Fact]
        public static void Overwrite_XWithYIncludingCommentsIfTargetIsUncommented_OverwritesCommendsWithSourceComments()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(Dx)
                .With(Dy)
                .IncludingNewComments()
                .ForAllSourceRows());

            // Assert
            AssertCommentsAre(r["a"], new string[] { });
            AssertCommentsAre(r["b"], "ybc");
            AssertCommentsAre(r["c"], "xcc");
        }

        [Fact]
        public static void Overwrite_XWithYExcludingComments_KeepsOriginalComments()
        {
            // Act
            var r = TomlTable.Combine(op => op.Overwrite(Dx)
                .With(Dy)
                .ExcludingComments()
                .ForAllSourceRows());

            // Assert
            AssertCommentsAre(r["a"], new string[] { });
            AssertCommentsAre(r["b"], new string[] { }); // Do not copy comment => still empty
            AssertCommentsAre(r["c"], "xcc");
        }

        [Fact]
        public void Overwrite_XWithY_ForDocExampleR1_ProducesCorrectResultTables()
        {

            // Act
            var r1 = TomlTable.Combine(op => op.Overwrite(Dx).With(Dy).ForRowsOnlyInSource());

            // Assert
            r1.Count.Should().Be(3);
            r1.Get<int>("a").Should().Be(1);
            r1.Get<int>("b").Should().Be(2);
            r1.Get<int>("c").Should().Be(3);
        }

        [Fact]
        public void Overwrite_XWithY_ForDocExampleR2_ProducesCorrectResultTables()
        {

            // Act
            var r1 = TomlTable.Combine(op => op.Overwrite(Dx).With(Dy).ForAllSourceRows());

            // Assert
            r1.Count.Should().Be(3);
            r1.Get<int>("a").Should().Be(1);
            r1.Get<int>("b").Should().Be(2);
            r1.Get<int>("c").Should().Be(4);
        }

        [Fact]
        public void Overwrite_XWithY_ForDocExampleR3_ProducesCorrectResultTables()
        {

            // Act
            var r1 = TomlTable.Combine(op => op.Overwrite(Dx).With(Dy).ForAllTargetRows());

            // Assert
            r1.Count.Should().Be(2);
            r1.Get<int>("a").Should().Be(1);
            r1.Get<int>("c").Should().Be(4);
        }

        public static IEnumerable<object[]> IsClonedTestCases
        {
            get
            {
                yield return new object[] { (Func<TomlTable, TomlTable, TomlTable>)((x, y) => TomlTable.Combine(op => op.Overwrite(x).With(y).ForAllSourceRows())) };
                yield return new object[] { (Func<TomlTable, TomlTable, TomlTable>)((x, y) => TomlTable.Combine(op => op.Overwrite(x).With(y).ForRowsOnlyInSource())) };
                yield return new object[] { (Func<TomlTable, TomlTable, TomlTable>)((x, y) => TomlTable.Combine(op => op.Overwrite(x).With(y).ForAllTargetRows())) };
            }
        }

        [Theory]
        [MemberData(nameof(IsClonedTestCases))]
        public void CombineOperation_ReturnsDeepClonedDataStructure(Func<TomlTable, TomlTable, TomlTable> f)
        {
            // Act
            var r = f(X, Y);

            // Assert
            r.Should().NotBeSameAs(X);
            r.Should().NotBeSameAs(Y);
        }

        private static void AssertNoRowWasAdded(TomlTable r)
            => r.Count.Should().Be(X.Count);

        private static void AssertYOnlyRowWasAddedTo(TomlTable r)
        {
            r.Count.Should().Be(X.Count + 1);
            r.Get<string>(YKey).Should().Be(YVal);
        }

        private static void AssertSameKeyRowWasOverwritten(TomlTable r)
            => r.Get<string>(SameKey).Should().Be(YVal);

        private static void AssertSameKeyNotTouched(TomlTable r)
            => r.Get<string>(SameKey).Should().Be(XVal);

        private static void AssertSubTableRowWasOverwritten(TomlTable r)
            => r.Get<TomlTable>(SubTableKey).Get<string>(SubTableValueKey).Should().Be(YSubTableVal);

        private static void AssertSubTableNotTouched(TomlTable r)
            => r.Get<TomlTable>(SubTableKey).Get<string>(SubTableValueKey).Should().Be(XSubTableVal);

        private static void AssertCommentsAre(TomlObject o, params string[] comments)
        {
            var tc = comments.Select(c => new TomlComment(c));
            o.Comments.Should().BeEquivalentTo(tc);
        }
    }
}
