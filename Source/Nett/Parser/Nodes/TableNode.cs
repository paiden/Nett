using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class TableNode : ExpressionNode, IHasComments
    {
        public TableNode(Token lbrac, IReq<Node> table, Token rbrac, IEnumerable<Comment> preComments, Comment appComment)
        {
            this.LBrac = new TerminalNode(lbrac).Req();
            this.RBrac = new TerminalNode(rbrac).Req();
            this.Table = table;
            this.PreComments = preComments;
            this.AppComment = appComment;
        }

        public IReq<TerminalNode> LBrac { get; }

        public IReq<Node> Table { get; }

        public IReq<TerminalNode> RBrac { get; }

        public IEnumerable<Comment> PreComments { get; }

        public Comment AppComment { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.LBrac, this.Table, this.RBrac);

        public override string ToString()
            => $"T";
    }

    internal sealed class StandardTableNode : Node
    {
        public StandardTableNode(IReq<KeyNode> key)
        {
            this.Key = key;
        }

        public IReq<KeyNode> Key { get; }

        public override IEnumerable<Node> Children
            => NodesAsEnumerable(this.Key);

        public override string ToString()
            => "TS";
    }

    internal sealed class TableArrayNode : Node
    {
        public TableArrayNode(Token lbrac, IReq<KeyNode> key, Token rbrac)
        {
            this.LBrac = new TerminalNode(lbrac).Req();
            this.Key = key;
            this.RBrac = new TerminalNode(rbrac).Req();
        }

        public IReq<TerminalNode> LBrac { get; }

        public IReq<KeyNode> Key { get; }

        public IReq<TerminalNode> RBrac { get; }

        public override IEnumerable<Node> Children
            => NodesAsEnumerable(this.LBrac, this.Key, this.RBrac);

        public override string ToString()
            => "TA";
    }
}
