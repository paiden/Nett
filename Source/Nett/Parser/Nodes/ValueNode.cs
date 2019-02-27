using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal class ValueNode : Node
    {
        private ValueNode(IReq<Node> value, IOpt<TerminalNode> unit)
        {
            this.Value = value;
            this.Unit = unit;
        }

        public IReq<Node> Value { get; }

        public IOpt<TerminalNode> Unit { get; }

        public override IEnumerable<Node> Children
            => NodesAsEnumerable(this.Value);

        public override SourceLocation Location
            => this.Value.Node().Location;

        public static ValueNode CreateTerminalValue(Token value)
            => new ValueNode(new TerminalNode(value).Req(), Opt<TerminalNode>.None);

        public static ValueNode CreateNonTerminalValue(IReq<Node> value)
            => new ValueNode(value, Opt<TerminalNode>.None);

        public IReq<ValueNode> WithUnitAttached(IOpt<TerminalNode> unit)
            => new ValueNode(this.Value, unit).Req();

        public override string ToString()
            => "V";
    }
}
