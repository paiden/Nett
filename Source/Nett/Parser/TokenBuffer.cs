namespace Nett.Parser
{
    using System;

    internal sealed class TokenBuffer : LookaheadBuffer<Token>
    {
        private bool autoThrowAwayNewlines = false;
        private int position = 0;

        public TokenBuffer(Func<Token?> read, int lookAhead)
            : base(read, lookAhead)
        {
        }

        public override bool End => this.Peek().type == TokenType.Eof || base.End;

        public void ConsumeAllNewlines()
        {
            while (this.Peek().type == TokenType.NewLine) { this.Consume(); }
        }

        public Token Expect(TokenType tt)
        {
            var t = this.Peek();
            if (t.type != tt)
            {
                throw Parser.CreateParseError(t, $"Expected token of type '{tt}' but token with value of '{t.value}' and the type '{t.type}' was found.");
            }

            return t;
        }

        public Token ExpectAndConsume(TokenType tt)
        {
            this.Expect(tt);
            return this.Consume();
        }

        public bool TryExpect(string tokenValue)
        {
            return this.Peek().value == tokenValue;
        }

        public bool TryExpectAndConsume(TokenType tt)
        {
            var r = this.TryExpect(tt);
            if (r)
            {
                this.Consume();
            }

            return r;
        }

        public bool TryExpect(TokenType tt)
        {
            return !this.End && this.Peek().type == tt;
        }

        public bool TryExpectAt(int index, TokenType tt)
        {
            return !this.End && this.PeekAt(index).type == tt;
        }

        public bool TryExpectAt(TokenType tt)
        {
            return this.Peek().type == tt;
        }

        public IDisposable UseIgnoreNewlinesContext()
        {
            this.ConsumeAllNewlines();
            return new AutoThrowAwayNewLinesContext(this);
        }

        public override Token Consume()
        {
            var t = base.Consume();
            this.position++;

            if (this.autoThrowAwayNewlines)
            {
                this.ConsumeAllNewlines();
            }

            return t;
        }

        public ImaginaryContext GetImaginaryContext()
        {
            return new ImaginaryContext(this);
        }

        public struct ImaginaryContext
        {
            private readonly TokenBuffer buffer;
            private int bufferPosition;
            private int position;

            public ImaginaryContext(TokenBuffer buffer)
            {
                this.buffer = buffer;
                this.bufferPosition = buffer.position;
                this.position = 0;
            }

            public Token Peek()
            {
                return this.PeekAt(0);
            }

            public Token PeekAt(int la)
            {
                this.CheckBufferPosition();
                return this.buffer.PeekAt(this.position + la);
            }

            public bool TryExpect(TokenType tt)
            {
                return this.TryExpectAt(0, tt);
            }

            public bool TryExpectAt(int la, TokenType tt)
            {
                this.CheckBufferPosition();
                return this.buffer.TryExpectAt(this.position + la, tt);
            }

            public Token Consume()
            {
                this.CheckBufferPosition();
                return this.buffer.PeekAt(this.position++);
            }

            public bool TryExpectAndConsume(TokenType tt)
            {
                var r = this.TryExpect(tt);
                if (r)
                {
                    this.position++;
                }

                return r;
            }

            public void ConsumeAllNewlines()
            {
                while (this.TryExpectAndConsume(TokenType.NewLine)) { }
            }

            public void MakeItReal()
            {
                this.CheckBufferPosition();
                for (; this.position > 0; this.position--)
                {
                    this.buffer.Consume();
                }

                this.bufferPosition = this.buffer.position;
            }

            [System.Diagnostics.Conditional("DEBUG")]
            private void CheckBufferPosition()
            {
                if (this.buffer.position != this.bufferPosition)
                {
                    throw new InvalidOperationException("Position of TokenBuffer have been moved.");
                }
            }
        }

        private class AutoThrowAwayNewLinesContext : IDisposable
        {
            private readonly TokenBuffer buffer;

            public AutoThrowAwayNewLinesContext(TokenBuffer buffer)
            {
                this.buffer = buffer;
                buffer.autoThrowAwayNewlines = true;
            }

            public void Dispose() => this.buffer.autoThrowAwayNewlines = false;
        }
    }
}
