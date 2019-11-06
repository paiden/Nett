using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FluentAssertions;
using Nett.Coma.Extensions;
using Xunit;

namespace Nett.Coma.Tests.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class LambdaExpressionExtensionTests
    {
        [Fact]
        public void BuildTPath_WhenValueMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.I;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/I");
        }

        [Fact]
        public void BuildTPath_WhenArrayMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, int[]>> e = r => r.A;
            object x = e;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/A");
        }

        [Fact]
        public void BuildTPath_WhenEnumerableMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, IEnumerable<bool>>> e = r => r.E;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/E");
        }

        [Fact]
        public void BuildTPath_WhenListMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, List<string>>> e = r => r.L;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/L");
        }

        [Fact]
        public void BuildTPath_WhenComplexMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, Item>> e = r => r.Item;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/Item");
        }

        [Fact]
        public void BuildTPath_WhenComplexArrayMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, Item[]>> e = r => r.Items;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/Items");
        }

        [Fact]
        public void BuildTPath_WhenComplexEnumMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, IEnumerable<Item>>> e = r => r.EItems;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/EItems");
        }

        [Fact]
        public void BuildTPath_WhenComplexListMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, List<Item>>> e = r => r.LItems;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/LItems");
        }

        [Fact]
        public void BuildTPath_WhenItemIntMemberAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.Item.I;

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/Item/I");
        }

        [Fact]
        public void BuildTPath_WhenAMemberIndexAccessed_CreatesCorrectTPath()
        {
            // Arrange
            Expression<Func<Root, int>> e = r => r.A[0];

            // Act
            var path = e.BuildTPath();

            // Assert
            path.ToString().Should().Be("/A[0]");
        }

        public class Root
        {
            public int I { get; set; }

            public int[] A { get; set; }

            public IEnumerable<bool> E { get; set; }

            public List<string> L { get; set; }

            public Item Item { get; set; }

            public Item[] Items { get; set; }

            public IEnumerable<Item> EItems { get; set; }

            public List<Item> LItems { get; set; }
        }

        public class Item
        {
            public int I { get; set; }
        }
    }
}
