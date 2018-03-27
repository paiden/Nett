using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class ArraySeparatorNode : Node
    {
        public ArraySeparatorNode(Token symbol, IOpt<ArrayItemNode> nextItem, Comment comment)
        {
            this.Seprator = new TerminalNode(symbol).Req();
            this.NextItem = nextItem;
            this.Comment = comment;
        }

        public IReq<TerminalNode> Seprator { get; }

        public IOpt<ArrayItemNode> NextItem { get; }

        public Comment Comment { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Seprator, this.NextItem);

        public override string ToString()
            => "AS";
    }
}
