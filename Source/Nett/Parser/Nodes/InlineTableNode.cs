using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class InlineTableNode : Node, IHasComments
    {
        public InlineTableNode(
            Token lcurly,
            IOpt<InlineTableItemNode> first,
            Token rcurly,
            IEnumerable<Comment> preComments,
            Comment appComment)
        {
            this.LCurly = new TerminalNode(lcurly).Req();
            this.RCurly = new TerminalNode(rcurly).Req();
            this.PreComments = preComments;
            this.AppComment = appComment;
            this.First = first;
        }

        public IReq<TerminalNode> LCurly { get; }

        public IReq<TerminalNode> RCurly { get; }

        public IOpt<InlineTableItemNode> First { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.LCurly, this.First, this.RCurly);

        public Comment AppComment { get; }

        public IEnumerable<Comment> PreComments { get; }

        public override string ToString()
            => "I";

        public IEnumerable<KeyValueExpressionNode> GetExpressions()
            => EnumerableFromBranch(
                this.First.SyntaxNodeOrDefault(),
                n => n.Expression.SyntaxNode(),
                n => n.Next.SyntaxNodeOrDefault()?.Next.SyntaxNodeOrDefault());
    }
}
