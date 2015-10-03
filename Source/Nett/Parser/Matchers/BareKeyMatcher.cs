using System.Text;

namespace Nett.Parser.Matchers
{
    /// <summary>
    /// Note this doesn't match the all bare keys e.g. '1234'. So all keys are only recognized at the parser level.
    /// </summary>
    internal class BareKeyMatcher : MatcherBase
    {
        private readonly StringBuilder alreadyMatched;
        public BareKeyMatcher(StringBuilder alreadyMatched)
        {
            this.alreadyMatched = alreadyMatched;
        }

        public BareKeyMatcher()
        {
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            var sb = alreadyMatched ?? new StringBuilder(64);

            while (!cs.End && cs.Peek().IsBareKeyChar())
            {
                sb.Append(cs.Consume());
            }

            return sb.Length > 0 ? new Token(TokenType.BareKey, sb.ToString()) : new Token?();
        }
    }
}
