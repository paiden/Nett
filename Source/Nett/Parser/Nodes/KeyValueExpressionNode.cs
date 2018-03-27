using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class KeyValueExpressionNode : ExpressionNode, IHasComments
    {
        public KeyValueExpressionNode(
            IReq<KeyNode> key, Token assignment, IReq<ValueNode> value, IEnumerable<Comment> preComments, Comment appComment)
        {
            this.Key = key;
            this.Assignment = new TerminalNode(assignment).Req();
            this.Value = value;
            this.PreComments = preComments;
            this.AppComment = appComment;
        }

        public IReq<KeyNode> Key { get; }

        public IReq<TerminalNode> Assignment { get; }

        public IReq<ValueNode> Value { get; }

        public IEnumerable<Comment> PreComments { get; }

        public Comment AppComment { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Key, this.Assignment, this.Value);
    }
}
