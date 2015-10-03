namespace Nett.Parser.Matchers
{
    internal sealed class SymbolsMatcher : MatcherBase
    {
        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            if (cs.TryExpect('[')) return new Token(TokenType.LBrac, new string(cs.Consume(), 1));
            else if (cs.TryExpect(']')) return new Token(TokenType.RBrac, new string(cs.Consume(), 1));
            else if (cs.TryExpect('=')) return new Token(TokenType.Assign, new string(cs.Consume(), 1));
            else if (cs.TryExpect(',')) return new Token(TokenType.Comma, new string(cs.Consume(), 1));
            else if (cs.TryExpect('.')) return new Token(TokenType.Dot, new string(cs.Consume(), 1));

            return null;
        }
    }
}
