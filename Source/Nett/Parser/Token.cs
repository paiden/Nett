namespace Nett.Parser
{
    enum TokenType
    {
        Unknown,
        Eof,
        BareKey,
        Integer,
        Float,
        Bool,
        String,
        LiteralString,
        MultilineString,
        MultilineLiteralString,
        DateTime,
        Timespan,
    }

    internal struct Token
    {
        public TokenType type;
        public string value;
        public int line;
        public int col;

        public Token(TokenType type, string value)
        {
            this.type = type;
            this.value = value;
            this.line = 0;
            this.col = 0;
        }
    }
}
