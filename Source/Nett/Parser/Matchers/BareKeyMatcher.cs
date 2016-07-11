namespace Nett.Parser.Matchers
{
    using System.Text;

    /// <summary>
    /// Note this doesn't match the all bare keys e.g. '1234'. So all keys are only recognized at the parser level.
    /// </summary>
    internal static class BareKeyMatcher
    {
        public static Token? TryContinueMatch(StringBuilder alreadyMatched, LookaheadBuffer<char> cs)
        {
            return TryMatchInternal(alreadyMatched, cs);
        }

        public static Token? TryMatch(LookaheadBuffer<char> cs)
        {
            return TryMatchInternal(null, cs);
        }

        private static Token? TryMatchInternal(StringBuilder alreadyMatched, LookaheadBuffer<char> cs)
        {
            var sb = alreadyMatched ?? new StringBuilder(64);

            while (!cs.End && cs.Peek().IsBareKeyChar())
            {
                sb.Append(cs.Consume());
            }

            return sb.Length > 0 ? new Token(TokenType.BareKey, sb.ToString()) : default(Token?);
        }
    }
}
