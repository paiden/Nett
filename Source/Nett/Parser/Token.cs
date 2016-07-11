namespace Nett.Parser
{
    using System.Diagnostics;

    internal enum TokenType
    {
        Unknown,
        NewLine,
        Eof,

        BareKey,
        Comment,
        Dot,
        Key,
        Assign,
        Comma,

        LBrac,
        RBrac,
        LCurly,
        RCurly,

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

    [DebuggerDisplay("{value}")]
    internal struct Token
    {
        public static Token Eof = new Token(TokenType.Eof, "<End of input>");

#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
        public int col;
        public int line;
        public TokenType type;
        public string value;
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter

        public Token(TokenType type, string value)
        {
            this.type = type;
            this.value = value;
            this.line = 0;
            this.col = 0;
        }

        public string PrefixWithTokenPostion(string message) => $"Line {this.line} at column {this.col}: {message}";
    }
}
