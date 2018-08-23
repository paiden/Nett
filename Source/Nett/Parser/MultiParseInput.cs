using System;
using Nett.Parser.Nodes;

namespace Nett.Parser
{
    internal sealed class MultiParseInput : IParseInput
    {
        private readonly IParseInput standard;
        private readonly IParseInput ignoreNewlines;

        private IParseInput active;

        public MultiParseInput(IParseInput standard, IParseInput ignorewNewlines)
        {
            this.standard
                = this.active = standard;
            this.ignoreNewlines = ignorewNewlines;
        }

        public bool IsFinished
            => this.active.IsFinished;

        public Token Current
            => this.active.Current;

        public Token Advance()
            => this.active.Advance();

        public ICommentsContext CreateConsumeCommentContext()
            => this.active.CreateConsumeCommentContext();

        public bool Peek(Func<Token, bool> predicate)
            => this.active.Peek(predicate);

        public IDisposable UseIgnorewNewlinesInput()
            => new Context(this, this.ignoreNewlines);

        private class Context : IDisposable
        {
            private readonly MultiParseInput input;
            private readonly IParseInput toRestore;

            public Context(MultiParseInput input, IParseInput contextual)
            {
                this.input = input;
                this.toRestore = input.active;
                this.input.active = contextual;
            }

            public void Dispose()
            {
                this.input.active = this.toRestore;
            }
        }
    }
}
