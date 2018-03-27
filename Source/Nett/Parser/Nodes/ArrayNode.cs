using System.Collections.Generic;

namespace Nett.Parser.Nodes
{
    internal sealed class ArrayNode : Node, IHasComments
    {
        private ArrayNode(IReq<TerminalNode> lbrac, IReq<TerminalNode> rbrac, IEnumerable<Comment> preComment, Comment appComment)
            : this(lbrac, AstNode.None<ArrayItemNode>(), rbrac, preComment, appComment)
        {
        }

        private ArrayNode(
            IReq<TerminalNode> lbrac,
            IOpt<ArrayItemNode> item,
            IReq<TerminalNode> rbrac,
            IEnumerable<Comment> preComments,
            Comment appComment)
        {
            this.LBrac = lbrac;
            this.RBrac = rbrac;
            this.Item = item;
            this.PreComments = preComments;
            this.AppComment = appComment;
        }

        public IReq<TerminalNode> LBrac { get; }

        public IReq<TerminalNode> RBrac { get; }

        public IOpt<ArrayItemNode> Item { get; }

        public IEnumerable<Comment> PreComments { get; }

        public Comment AppComment { get; }

        public override IEnumerable<Node> Children
            => NonNullNodesAsEnumerable(this.LBrac, this.Item, this.RBrac);

        public static ArrayNode Empty(Token lbrac, Token rbrac, IEnumerable<Comment> preComments, Comment appComment)
            => new ArrayNode(AstNode.Required(new TerminalNode(lbrac)), AstNode.Required(new TerminalNode(rbrac)), preComments, appComment);

        public static ArrayNode Create(Token lbrac, IOpt<ArrayItemNode> item, Token rbrac, IEnumerable<Comment> preComments, Comment appComment)
            => new ArrayNode(new TerminalNode(lbrac).Req(), item, new TerminalNode(rbrac).Req(), preComments, appComment);

        public IEnumerable<ValueNode> GetValues()
            => EnumerableFromBranch(
                this.Item.SyntaxNodeOrDefault(),
                n => n.Value.SyntaxNode(),
                n => n.Separator.SyntaxNodeOrDefault()?.NextItem.SyntaxNodeOrDefault());

        public override string ToString()
            => "A";
    }
}
