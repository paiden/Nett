namespace Nett.Parser
{
    using System;

    internal sealed class TokenBuffer : LookaheadBuffer<Token>
    {
        public TokenBuffer(Func<Token?> read, int lookAhead)
            : base(read, lookAhead)
        {
        }

        public Token Expect(TokenType tt)
        {
            var t = this.Peek();
            if (t.type != tt)
            {
                throw new Exception($"Expected token of type '{tt}' but token with value of '{t.value}' and the type '{t.type}' was found.");
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
    }
}
