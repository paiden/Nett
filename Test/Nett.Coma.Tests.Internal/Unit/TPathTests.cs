using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FluentAssertions;
using Nett.Coma.Extensions;
using Xunit;

namespace Nett.Coma.Tests.Internal.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class TPathTests
    {
        private readonly Root r;
        private readonly TomlTable tbl;

        public TPathTests()
        {
            this.r = new Root();
            this.tbl = Toml.Create(this.r);
        }

        [Fact]
        public void Apply_WhenIMemberAccessed_ReturnsTheIMember()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.I;
            var path = e.BuildTPath();

            // Act
            var o = path.Get(this.tbl);

            // Assert
            o.Get<int>().Should().Be(1);
        }

        [Fact]
        public void Apply_WhenAMemberAccessed_ReturnsTheAMember()
        {
            // Arrange
            Expression<Func<Root, int[]>> e = r => r.A;
            var path = e.BuildTPath();

            // Act
            var o = path.Get(this.tbl);

            // Assert
            o.Get<int[]>().Should().Equal(new int[] { 1, 2 });
        }

        [Fact]
        public void Apply_WhenItemIMemberAccessed_ReturnsItemIMember()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.Item.I;
            var path = e.BuildTPath();

            // Act
            var o = path.Get(this.tbl);

            // Assert
            o.Get<int>().Should().Be(this.r.Item.I);
        }

        [Fact]
        public void Apply_WhenAItemIndexAccessed_ReturnsValueOfThatIndex()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.A[1];
            var path = e.BuildTPath();

            // Act
            var o = path.Get(this.tbl);

            // Assert
            o.Get<int>().Should().Be(this.r.A[1]);
        }

        public class Root
        {
            public int I { get; set; } = 1;

            public int[] A { get; set; } = new int[] { 1, 2 };

            public IEnumerable<bool> E { get; set; }

            public List<string> L { get; set; }

            public Item Item { get; set; } = new Item();

            public Item[] Items { get; set; }

            public IEnumerable<Item> EItems { get; set; }

            public List<Item> LItems { get; set; }
        }

        public class Item
        {
            public int I { get; set; } = 2;
        }
    }
}
