using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nett.Collections;
using Xunit;

namespace Nett.Tests.Internal.Collections
{
    public sealed class TreeOperationsTests
    {
        [Fact]
        public void TraverseBreadthFirst_ProducesCorrectSequence()
        {
            // Arrange
            var t = CreateTree();

            // Act
            var bf = t.TraverseBreadthFirst();

            // Assert
            var seq = string.Concat(bf.Select(i => i.V));
            seq.Should().Be("FBGADICEH");
        }

        [Fact]
        public void TraversePreOrder_ProducesCorrectSequence()
        {
            // Arrange
            var t = CreateTree();

            // Act
            var bf = t.TraversePreOrder();

            // Assert
            var seq = string.Concat(bf.Select(i => i.V));
            seq.Should().Be("FBADCEGIH");
        }

        [Fact]
        public void TraversePreOrderWithDepth_ProducesCorrectSequence()
        {
            // Arrange
            var t = CreateTree();

            // Act
            var bf = t.TraversePreOrderWithDepth();

            // Assert
            var seq = string.Concat(bf.Select(i => i.ToString()));
            seq.Should().Be("0F1B2A2D3C3E1G2I3H");
        }

        private static Node CreateTree()
        {
            return
            new Node("F",
                new Node("B",
                    new Node("A"),
                    new Node("D",
                        new Node("C"),
                        new Node("E"))),
                new Node("G",
                    new Node("I",
                        new Node("H"))));

        }

        private class Node : IGetChildren<Node>
        {
            public Node(string value, params Node[] children)
            {
                this.V = value;
                this.Children = children;
            }

            public string V { get; set; }

            public IEnumerable<Node> Children { get; }

            IEnumerable<Node> IGetChildren<Node>.GetChildren()
                => this.Children;

            public override string ToString()
                => $"{this.V}";
        }
    }
}
