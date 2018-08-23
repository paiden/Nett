using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nett.Collections;

namespace Nett.Parser.Nodes
{
    internal abstract class Node : IGetChildren<Node>
    {
        public abstract IEnumerable<Node> Children { get; }

        public virtual SourceLocation Location
            => SourceLocation.None;

        IEnumerable<Node> IGetChildren<Node>.GetChildren()
            => this.Children;

        public string PrintTree()
        {
            var builder = new StringBuilder();
            var allNodes = this.TraversePreOrderWithDepth();

            foreach (var n in allNodes)
            {
                builder.Append(new string(' ', n.Depth))
                    .AppendLine(n.Node.ToString());
            }

            return builder.ToString();
        }

        protected static IEnumerable<Node> NodesAsEnumerable(params IReq<Node>[] nodes)
            => nodes.Select(n => n.Node());

        protected static IEnumerable<Node> NonNullNodesAsEnumerable(params IParsed<Node>[] nodes)
            => nodes.Where(n => n.HasNode).Select(n => n.NodeOrDefault());

        protected static IEnumerable<ExpressionNode> LinearizeExpressions(NextExpressionNode root)
            => EnumerableFromBranch(root, c => c.Expression.SyntaxNode(), c => c.Next.SyntaxNodeOrDefault());

        protected static IEnumerable<R> EnumerableFromBranch<R, T>(T root, Func<T, R> selectItem, Func<T, T> selectNext)
            where T : Nodes.Node
            where R : Nodes.Node
        {
            for (T cur = root; cur != null; cur = selectNext(cur))
            {
                yield return selectItem(cur);
            }
        }

        protected Token CheckTokenType(TokenType expected, Token t)
        {
            if (t.Type != expected)
            {
                throw new ArgumentException($"Expected token of type '{expected}' but actual token has type '{t.Type}'.");
            }

            return t;
        }
    }
}
