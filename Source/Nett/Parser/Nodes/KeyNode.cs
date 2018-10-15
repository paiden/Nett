using System;
using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class KeyNode : Node
    {
        public KeyNode(Token key, IOpt<KeySeparatorNode> next)
        {
            this.Key = new TerminalNode(key).Req();
            this.Next = next;
        }

        public IReq<TerminalNode> Key { get; }

        public IOpt<KeySeparatorNode> Next { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Key, this.Next);

        public TomlKey ExpressionKey()
        {
            if (this.Next.HasNode)
            {
                throw new InvalidOperationException("Cannot convert this key to a expression key.");
            }

            return TomlKey.FromToken(this.Key.SyntaxNode().Terminal);
        }

        public bool IsDottedKey()
            => this.Next.HasNode;

        public IEnumerable<TerminalNode> GetSegments()
            => EnumerableFromBranch(this, n => n.Key.SyntaxNode(), n => n.Next.SyntaxNodeOrDefault()?.Next.SyntaxNode());

        public override string ToString()
            => $"K";
    }

    internal sealed class KeySeparatorNode : Node
    {
        public KeySeparatorNode(Token separtor, IReq<KeyNode> next)
        {
            this.Separator = new TerminalNode(separtor).Req();
            this.Next = next;
        }

        public IReq<TerminalNode> Separator { get; }

        public IReq<KeyNode> Next { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Separator, this.Next);

        public override string ToString()
            => "KS";
    }
}
