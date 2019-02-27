using System.Collections.Generic;
using Nett.Extensions;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal sealed class Parser
    {
        private readonly MultiParseInput input;
        private readonly TomlSettings settings;

        public Parser(IParseInput input, TomlSettings settings)
        {
            this.input = new MultiParseInput(input, new SkippingParseInput(input, toSkip: TokenType.NewLine));
            this.settings = settings.CheckNotNull(nameof(settings));
        }

        public IOpt<StartNode> Parse()
        {
            this.input.AcceptNewLines();

            if (this.input.IsFinished) { return Opt<StartNode>.None; }

            var expressions = new List<IReq<ExpressionNode>>();

            var exp = this.Expression();
            var next = this.NextExpression();

            return new StartNode(exp, next).Opt();
        }

        private static bool IsKey(Token t)
            => t.Type == TokenType.BareKey
                || t.Type == TokenType.DoubleQuotedKey
                || t.Type == TokenType.SingleQuotedKey
                || t.Type == TokenType.DottedKey;

        private IReq<ExpressionNode> Expression()
        {
            using (var cctx = this.input.CreateConsumeCommentContext())
            {
                if (this.input.Peek(t => t.Type == TokenType.Eof))
                {
                    return new CommentExpressionNode(cctx.Consume()).Req();
                }

                return KeyValueExpression()
                    .Or<ExpressionNode>(Table)
                    .OrNode(() => SyntaxErrorNode.Unexpected("Expected TOML table or row key", this.input.Current));

                IOpt<KeyValueExpressionNode> KeyValueExpression()
                {
                    var key = this.input.Accept(IsKey)
                        .CreateNode(t => new KeyNode(t, this.KeySeparator()).Opt());
                    if (key.HasNode)
                    {
                        var comments = cctx.Consume();
                        return this.input.Expect(t => t.Type == TokenType.Assign)
                            .CreateNode(
                                assignToken => new KeyValueExpressionNode(
                                    key.AsReq(),
                                    assignToken,
                                    this.Value(NoCommentsHere.Instance),
                                    comments,
                                    this.AppComment()).Opt(),
                                t => new SyntaxErrorNode("Key value expression's '=' is missing.", t.Location));
                    }
                    else
                    {
                        return Opt<KeyValueExpressionNode>.None;
                    }
                }

                IOpt<TableNode> Table()
                    => this.input
                        .Accept(t => t.Type == TokenType.LBrac)
                        .CreateNode(lbrac => this.Table(lbrac, cctx.Consume()).AsOpt());
            }
        }

        private IReq<TableNode> Table(Token lbrac, IEnumerable<Comment> comments)
        {
            var tbl = this.TableArray()
                .Orr<Node>(this.StandardTable);

            return this.input.Expect(t => t.Type == TokenType.RBrac)
                .CreateNode(rbrac => new TableNode(lbrac, tbl, rbrac, comments, this.AppComment()).Req());
        }

        private IReq<StandardTableNode> StandardTable()
            => new StandardTableNode(this.Key()).Req();

        private IOpt<TableArrayNode> TableArray()
        {
            return this.input.Accept(t => t.Type == TokenType.LBrac)
                .CreateNode(lb => CreateNode(lb));

            IOpt<TableArrayNode> CreateNode(Token lbrac)
            {
                var key = this.Key();
                return this.input.Expect(t => t.Type == TokenType.RBrac)
                    .CreateNode(rb => new TableArrayNode(lbrac, key, rb).Opt());
            }
        }

        private IReq<KeyNode> Key()
            => this.input
                .Expect(IsKey)
                .CreateNode(t => new KeyNode(t, this.KeySeparator()).Req());

        private IOpt<KeySeparatorNode> KeySeparator()
        {
            if (Epsilon()) { return Opt<KeySeparatorNode>.None; }

            return this.input.Expect(t => t.Type == TokenType.Dot)
                .CreateNode(t => new KeySeparatorNode(t, this.Key()).Opt());

            bool Epsilon()
                => this.input.Peek(t => t.Type == TokenType.RBrac || t.Type == TokenType.Assign || t.Type == TokenType.Eof);
        }

        private IOpt<NextExpressionNode> NextExpression()
        {
            if (this.input.AcceptNewLines() && !this.input.IsFinished)
            {
                return new NextExpressionNode(this.Expression(), this.NextExpression()).Opt();
            }
            else if (!this.input.IsFinished)
            {
                return new Opt<NextExpressionNode>(SyntaxErrorNode.Unexpected("Expected newline after expression ", this.input.Current));
            }

            return Opt<NextExpressionNode>.None;
        }

        private IReq<ValueNode> Value(ICommentsContext cctx)
        {
            var value = SimpleValue()
                .Or(Array)
                .Or(InlineTable)
                .OrNode(() => SyntaxErrorNode.Unexpected("Expected TOML value", this.input.Current));

            if (this.settings.IsFeautureEnabled(ExperimentalFeature.ValueWithUnit) && value.HasNode)
            {
                var unit = this.input.Accept(t => t.Type == TokenType.BareKey || t.Type == TokenType.Unit)
                    .CreateNode(t => new TerminalNode(t.Trimmed()).Opt());
                if (unit.HasNode)
                {
                    return value.SyntaxNode().WithUnitAttached(unit);
                }
            }

            return value;

            IOpt<ValueNode> SimpleValue()
                => this.input
                    .Accept(t => t.Type == TokenType.Float
                        || t.Type == TokenType.Integer
                        || t.Type == TokenType.HexInteger
                        || t.Type == TokenType.BinaryInteger
                        || t.Type == TokenType.OctalInteger
                        || t.Type == TokenType.Bool
                        || t.Type == TokenType.LocalDate
                        || t.Type == TokenType.OffsetDateTime
                        || t.Type == TokenType.LocalTime
                        || t.Type == TokenType.LocalDateTime
                        || t.Type == TokenType.Duration
                        || t.Type == TokenType.String
                        || t.Type == TokenType.LiteralString
                        || t.Type == TokenType.MultilineString
                        || t.Type == TokenType.MultilineLiteralString)
                    .CreateNode(t => ValueNode.CreateTerminalValue(t).Opt());

            IOpt<ValueNode> Array()
                => this.input
                    .Accept(t => t.Type == TokenType.LBrac)
                    .CreateNode(t => ValueNode.CreateNonTerminalValue(this.Array(t)).Opt());

            IOpt<ValueNode> InlineTable()
            {
                return this.input
                    .Accept(t => t.Type == TokenType.LCurly)
                    .CreateNode(t => ValueNode.CreateNonTerminalValue(this.InlineTable(t, cctx.Consume())).Opt());
            }
        }

        private Comment AppComment()
        {
            if (this.input.Peek(t => t.Type == TokenType.Comment))
            {
                return new Comment(this.input.Advance().Value);
            }

            return null;
        }

        private IReq<ArrayNode> Array(Token lbrac)
        {
            var value = this.ArrayValue(out var emptyArrayComments);

            return this.input.Expect(t => t.Type == TokenType.RBrac)
                .CreateNode(
                    onSuccess: rbrac => ArrayNode.Create(lbrac, value, rbrac, emptyArrayComments, this.AppComment()).Req(),
                    onError: ue => SyntaxErrorNode.Unexpected("Expected array close tag ']'", ue));
        }

        private IOpt<ArrayItemNode> ArrayValue(out IEnumerable<Comment> emptyArrayComments)
        {
            using (var cctx = this.input.CreateConsumeCommentContext())
            {
                if (Epsilon())
                {
                    emptyArrayComments = cctx.Consume();
                    return Opt<ArrayItemNode>.None;
                }

                var value = this.Value(cctx);

                this.input.AcceptNewLines();
                var sep = this.ArraySeparator();

                emptyArrayComments = Comment.NoComments;
                return new ArrayItemNode(value, sep, cctx.Consume()).Opt();

                bool Epsilon()
                    => this.input.Peek(t => t.Type == TokenType.RBrac);
            }
        }

        private IOpt<ArraySeparatorNode> ArraySeparator()
        {
            if (Epsilon()) { return Opt<ArraySeparatorNode>.None; }

            return this.input.Expect(t => t.Type == TokenType.Comma)
                .CreateNode(
                    onSuccess: t => new ArraySeparatorNode(t, this.ArrayValue(out var _), this.AppComment()).Opt(),
                    onError: ut => SyntaxErrorNode.Unexpected("Array value is missing", ut));

            bool Epsilon()
                => this.input.Peek(t => t.Type == TokenType.RBrac);
        }

        private IReq<InlineTableNode> InlineTable(Token lcurly, IEnumerable<Comment> preComments)
        {
            var item = this.InlineTableItem();

            return this.input.Expect(t => t.Type == TokenType.RCurly)
                .CreateNode(t => new InlineTableNode(lcurly, item, t, preComments, this.AppComment()).Req());
        }

        private IOpt<InlineTableItemNode> InlineTableItem()
        {
            if (Epsilon()) { return Opt<InlineTableItemNode>.None; }

            var key = this.Key();
            var kve = this.input
                .Expect(t => t.Type == TokenType.Assign)
                .CreateNode(a => new KeyValueExpressionNode(
                    key,
                    a,
                    this.Value(NoCommentsHere.Instance),
                    Comment.NoComments,
                    Comment.NoComment).Req());

            return new InlineTableItemNode(kve, this.NextInlineTableItem()).Opt();

            bool Epsilon()
                => this.input.Peek(t => t.Type == TokenType.RCurly);
        }

        private IOpt<InlineTableNextItemNode> NextInlineTableItem()
        {
            if (Epsilon()) { return Opt<InlineTableNextItemNode>.None; }

            return this.input.Expect(t => t.Type == TokenType.Comma)
                .CreateNode(s => new InlineTableNextItemNode(s, this.InlineTableItem()).Opt());

            bool Epsilon()
                => this.input.Peek(t => t.Type == TokenType.RCurly);
        }

        private void Epsilon()
        {
            // Readability method
        }
    }
}
