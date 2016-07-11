namespace Nett.Parser.Matchers
{
    internal static class SymbolsMatcher
    {
        internal static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            if (cs.TryExpect('[')) { return new Token(TokenType.LBrac, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect(']')) { return new Token(TokenType.RBrac, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect('=')) { return new Token(TokenType.Assign, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect(',')) { return new Token(TokenType.Comma, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect('.')) { return new Token(TokenType.Dot, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect('{')) { return new Token(TokenType.LCurly, new string(cs.Consume(), 1)); }
            else if (cs.TryExpect('}')) { return new Token(TokenType.RCurly, new string(cs.Consume(), 1)); }

            return null;
        }
    }
}
