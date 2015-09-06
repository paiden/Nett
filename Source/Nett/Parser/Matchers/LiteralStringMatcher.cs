namespace Nett.Parser.Matchers
{
    internal sealed class LiteralStringMatcher : SingleLineStringMatcher
    {
        public LiteralStringMatcher()
            : base('\'', TokenType.LiteralString)
        {

        }
    }
}
