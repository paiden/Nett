namespace Nett.Parser.Matchers
{
    using System.Text;

    /// <summary>
    /// Note this doesn't match the all bare keys e.g. '1234'. So all keys are only recognized at the parser level.
    /// </summary>
    internal static class BareKeyMatcher
    {
        public static Token? TryContinueMatch(StringBuilder alreadyMatched, CharBuffer cs)
        {
            return TryMatchInternal(alreadyMatched, cs);
        }

        public static Token? TryMatch(CharBuffer cs)
        {
            return TryMatchInternal(null, cs);
        }

        private static Token? TryMatchInternal(StringBuilder alreadyMatched, CharBuffer cs)
        {
            var sb = alreadyMatched ?? new StringBuilder(Constants.MatcherBufferSize);

            while (!cs.End && cs.Peek().IsBareKeyChar())
            {
                sb.Append(cs.Consume());
            }

            if (sb.Length > 0 && !TokenDone(cs))
            {
                return Token.CreateUnknownTokenFromFragment(cs, sb);
            }

            return sb.Length > 0 ? new Token(TokenType.BareKey, sb.ToString()) : default(Token?);
        }

        private static bool TokenDone(CharBuffer cs) => cs.TokenDone() || NextTokenIsValidBareKeySuccessor(cs);

        private static bool NextTokenIsValidBareKeySuccessor(CharBuffer cs) => cs.Peek() == '=' || cs.Peek() == '.';
    }
}
