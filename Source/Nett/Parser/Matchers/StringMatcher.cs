namespace Nett.Parser.Matchers
{
    internal sealed class StringMatcher : SingleLineStringMatcher
    {
        public StringMatcher()
            : base('\"', TokenType.String)
        {

        }
    }
}
