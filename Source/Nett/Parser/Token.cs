namespace Nett.Parser
{
    using System.Diagnostics;
    using Nett.Extensions;
    using Nett.Parser.Nodes;

    internal enum TokenType
    {
        Unknown,
        NewLine,
        Eof,

        BareKey,
        SingleQuotedKey,
        DoubleQuotedKey,
        DottedKey,
        Comment,
        Dot,
        Assign,
        Comma,

        LBrac,
        RBrac,
        LCurly,
        RCurly,

        Integer,
        HexInteger,
        OctalInteger,
        BinaryInteger,
        Float,
        Bool,
        String,
        LiteralString,
        MultilineString,
        MultilineLiteralString,
        LocalDate,
        LocalTime,
        LocalDateTime,
        OffsetDateTime,
        Duration,

        Unit,
    }

    [DebuggerDisplay("{Value}:{Type}")]
    internal struct Token
    {
        public readonly SourceLocation Location;
        public readonly TokenType Type;
        public readonly string Value;

        private string errorMessage;

        public Token(TokenType type, string value, SourceLocation location)
        {
            this.Type = type;
            this.Value = value;
            this.Location = location;
            this.errorMessage = null;
        }

        public bool IsEmpty => this.Value == null || this.Value.Trim().Length <= 0;

        public bool IsEof => this.Type == TokenType.Eof;

        public bool IsNewLine => this.Type == TokenType.NewLine;

        public static Token Unknown(string value, string errorMessage, SourceLocation location)
        {
            errorMessage.CheckNotNull(nameof(errorMessage));

            return new Token(TokenType.Unknown, value, location)
            {
                errorMessage = errorMessage
            };
        }

        public static Token NewLine(SourceLocation location)
            => new Token(TokenType.NewLine, "<NewLine>", location);

        public static Token EndOfFile(SourceLocation location)
            => new Token(TokenType.Eof, "<EndOfFile>", location);

        public override string ToString()
            => $"{this.Value}:{this.Type} at {this.Location}";

        public Token Trimmed()
            => new Token(this.Type, this.Value.Trim(), this.Location);

        public SyntaxErrorNode TokenError()
            => this.Type == TokenType.Unknown ? new SyntaxErrorNode(this.errorMessage, this.Location) : null;
    }
}
