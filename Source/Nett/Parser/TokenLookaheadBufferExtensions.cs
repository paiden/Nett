namespace Nett.Parser
{
    internal static class TokenLookaheadBufferExtensions
    {
        public static bool Expect(this LookaheadBuffer<Token> tokens, string tokenValue)
        {
            return tokens.Peek().value == tokenValue;
        }

        public static bool Expect(this LookaheadBuffer<Token> tokens, TokenType tt)
        {
            return tokens.Peek().type == tt;
        }

        public static bool ExpectAt(this LookaheadBuffer<Token> tokens, TokenType tt)
        {
            return tokens.Peek().type == tt;
        }
    }
}
