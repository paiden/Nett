using System.Collections.Generic;
using System.Linq;

namespace Nett.Parser.Nodes
{
    internal class TerminalNode : Node
    {
        public TerminalNode(Token terminal)
        {
            this.Terminal = terminal;
        }

        public Token Terminal { get; }

        public override SourceLocation Location
            => this.Terminal.Location;

        public override IEnumerable<Node> Children
            => Enumerable.Empty<Node>();

        public override string ToString()
            => $"{this.Terminal.Value}";
    }
}
