using System.Text;

namespace Nett.Parser.Matchers
{
    /// <summary>
    /// Note this doesn't match the all bare keys e.g. '1234'. So all keys are only recognized at the parser level.
    /// </summary>
    internal class BareKeyMatcher : MatcherBase
    {
        private StringBuilder sb;
        public BareKeyMatcher(StringBuilder alreadyMatched)
        {
            this.sb = alreadyMatched;
        }

        public BareKeyMatcher()
        {
            this.sb = new StringBuilder();
        }

        internal override Token? Match(LookaheadBuffer<char> cs)
        {
            while (!cs.End && cs.Peek().IsBareKeyChar())
            {
                this.sb.Append(cs.Consume());
            }

            return this.sb.Length > 0 ? new Token(TokenType.BareKey, sb.ToString()) : new Token?();
        }
    }
}
