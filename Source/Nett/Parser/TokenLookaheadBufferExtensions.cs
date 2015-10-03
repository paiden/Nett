using System;

namespace Nett.Parser
{
    internal static class TokenLookaheadBufferExtensions
    {
        public static bool TryExpect(this LookaheadBuffer<Token> tokens, string tokenValue)
        {
            return tokens.Peek().value == tokenValue;
        }

        public static bool TryExpect(this LookaheadBuffer<Token> tokens, TokenType tt)
        {
            return tokens.Peek().type == tt;
        }

        public static bool TryExpectAt(this LookaheadBuffer<Token> tokens, int index, TokenType tt)
        {
            return tokens.PeekAt(index).type == tt;
        }

        public static bool TryExpectAt(this LookaheadBuffer<Token> tokens, TokenType tt)
        {
            return tokens.Peek().type == tt;
        }

        public static Token ExpectAndConsume(this LookaheadBuffer<Token> tokens, TokenType tt)
        {
            var t = tokens.Peek();
            if (t.type != tt)
            {
                throw new Exception($"Expected token of '{tt}' but token with value of '{t.value}' and the type '{t.type}' was found.");
            }
            else
            {
                return tokens.Consume();
            }
        }
    }
}
