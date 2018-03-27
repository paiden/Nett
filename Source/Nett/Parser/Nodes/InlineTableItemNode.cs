using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class InlineTableItemNode : Node
    {
        public InlineTableItemNode(IReq<KeyValueExpressionNode> expression, IOpt<InlineTableNextItemNode> next)
        {
            this.Expression = expression;
            this.Next = next;
        }

        public IReq<KeyValueExpressionNode> Expression { get; }

        public IOpt<InlineTableNextItemNode> Next { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Expression, this.Next);

        public override string ToString()
            => "II";
    }
}
