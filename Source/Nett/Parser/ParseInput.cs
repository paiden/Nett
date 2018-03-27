using System;
using System.Collections.Generic;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal sealed partial class ParseInput : IParseInput
    {
        private const int EofTokenCount = 1;

        private readonly List<Token> tokens;

        private int index = 0;

        public ParseInput(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public bool IsFinished
            => this.index > this.tokens.Count - 1 - EofTokenCount;

        public Token Current
            => this.CurrentToken;

        private Token CurrentToken =>
            this.tokens[this.index];

        public Token Advance()
           => this.tokens[this.index++];

        public ICommentsContext CreateConsumeCommentContext()
            => CommentsContext.Create(this);

        public SyntaxErrorNode CreateErrorNode()
            => this.Current.TokenError() ?? SyntaxErrorNode.Unexpected(this.CurrentToken);

        public SyntaxErrorNode CreateErrorNode(string message)
            => new SyntaxErrorNode(message, this.CurrentToken.Location);

        public bool Peek(Func<Token, bool> predicate)
            => predicate(this.CurrentToken);

        internal sealed class CommentsContext : ICommentsContext
        {
            private const int NoReset = -1;

            private readonly ParseInput input;
            private readonly List<Comment> comments = new List<Comment>();

            private int resetIndex;

            private CommentsContext(ParseInput input)
            {
                this.resetIndex = input.index;
                this.input = input;
            }

            public static CommentsContext Create(ParseInput input)
            {
                var instance = new CommentsContext(input);
                instance.Init();
                return instance;
            }

            public IEnumerable<Comment> Consume()
            {
                this.resetIndex = NoReset;
                return this.comments;
            }

            public void Dispose()
            {
                if (this.resetIndex != NoReset)
                {
                    this.input.index = this.resetIndex;
                }
            }

            private void Init()
            {
                while (this.input.Peek(t => t.Type == TokenType.Comment || t.Type == TokenType.NewLine))
                {
                    this.input.AcceptNewLines();

                    if (this.input.Peek(t => t.Type == TokenType.Comment))
                    {
                        var tkn = this.input.Advance();
                        this.comments.Add(new Comment(tkn.Value));
                    }
                }
            }
        }
    }
}
