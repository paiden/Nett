using System.Collections.Generic;
using System.Linq;

namespace Nett.Parser.Nodes
{
    internal sealed class CommentNode : Node
    {
        public CommentNode(Token comment)
        {
            this.Comment = new TerminalNode(comment).Req();
        }

        public IReq<TerminalNode> Comment { get; }

        public override IEnumerable<Node> Children
            => NodesAsEnumerable(this.Comment);

        public override string ToString()
            => "C";
    }

    internal sealed class CommentExpressionNode : ExpressionNode
    {
        public CommentExpressionNode(IEnumerable<Comment> comments)
        {
            this.Comments = comments;
        }

        public IEnumerable<Comment> Comments { get; }

        public override IEnumerable<Node> Children
            => Enumerable.Empty<Node>();

        public override string ToString()
            => "CE";
    }
}
