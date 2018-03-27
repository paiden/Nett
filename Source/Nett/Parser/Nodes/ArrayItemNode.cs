using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class ArrayItemNode : Node
    {
        public ArrayItemNode(IReq<ValueNode> value, IOpt<ArraySeparatorNode> separator, IEnumerable<Comment> comments)
        {
            this.Value = value;
            this.Separator = separator;
            this.Comments = comments;
        }

        public IEnumerable<Comment> Comments { get; }

        public IReq<ValueNode> Value { get; }

        public IOpt<ArraySeparatorNode> Separator { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.Value, this.Separator);

        public override string ToString()
            => "AI";
    }
}
