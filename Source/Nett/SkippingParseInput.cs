using System;
using Nett.Parser;
using Nett.Parser.Nodes;

namespace Nett
{
    internal sealed class SkippingParseInput : IParseInput
    {
        private readonly IParseInput baseInput;
        private readonly TokenType toSkip;

        public SkippingParseInput(IParseInput baseInput, TokenType toSkip)
        {
            this.baseInput = baseInput;
            this.toSkip = toSkip;
        }

        public bool IsFinished
            => this.Skip().IsFinished;

        public Token Current =>
            this.Skip().Current;

        public Token Advance()
            => this.Skip().Advance();

        public ICommentsContext CreateConsumeCommentContext()
            => this.baseInput.CreateConsumeCommentContext();

        public bool Peek(Func<Token, bool> predicate)
            => this.Skip().Peek(predicate);

        private IParseInput Skip()
        {
            while (this.baseInput.Peek(t => t.Type == this.toSkip))
            {
                this.baseInput.Advance();
            }

            return this.baseInput;
        }
    }
}
