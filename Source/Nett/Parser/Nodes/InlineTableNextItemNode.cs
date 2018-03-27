using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class InlineTableNextItemNode : Node
    {
        public InlineTableNextItemNode(Token separator, IOpt<InlineTableItemNode> next)
        {
            this.Separator = new TerminalNode(separator).Req();
            this.Next = next;
        }

        public IReq<TerminalNode> Separator { get; }

        public IOpt<InlineTableItemNode> Next { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Separator, this.Next);

        public override string ToString()
            => "NI";
    }
}
