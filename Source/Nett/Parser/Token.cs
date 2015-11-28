using System.Diagnostics;

namespace Nett.Parser
{
    enum TokenType
    {
        Unknown,
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

        public string PrefixWithTokenPostion(string message) => $"Line {this.line} at column {this.col}: {message}";
    }
}
