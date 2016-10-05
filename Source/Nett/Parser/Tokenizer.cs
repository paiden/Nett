namespace Nett.Parser
{
    using System.IO;
    using System.Text;
    using Nett.Parser.Matchers;

    internal sealed class Tokenizer
    {
        private const int SBS = 256;

        private readonly CharBuffer characters;
        private readonly StreamReader reader;
        private readonly TokenBuffer tokens;

        public Tokenizer(Stream sr)
        {
            this.reader = new StreamReader(sr);
            this.characters = new CharBuffer(this.ReadChar, 64);
            this.tokens = new TokenBuffer(this.NextToken, 5);
        }

        public TokenBuffer Tokens => this.tokens;

        private char Consume()
        {
            return this.characters.Consume();
        }

        private Token ConsumeBareKeyToken(StringBuilder sb)
        {
            while (this.PeekIsBareKeyChar())
            {
                sb.Append(this.Consume());
            }

            return new Token(TokenType.BareKey, sb.ToString());
        }

        private Token ConsumeStringToken(StringBuilder sb)
        {
            sb.Append(this.Consume());

            while (!this.characters.TryExpectAt(0, '\"'))
            {
                sb.Append(this.Consume());
                if (this.characters.TryExpectAt(0, '\\'))
                {
                    sb.Append(this.Consume());
                    sb.Append(this.Consume());
                }
            }

            sb.Append(this.characters.Consume());

            return new Token(TokenType.String, sb.ToString());
        }

        private Token CreateEof() => Token.EndOfFile(this.characters.Line, this.characters.Column);

        private Token? NextToken()
        {
            if (this.characters.End) { return this.CreateEof(); }

            while (!this.characters.End && this.characters.TryExpectWhitespace())
            {
                int lineBeforeConsume = this.characters.Line;
                int colBeforeConsume = this.characters.Column;

                var c = this.characters.Consume();

                if (c == '\r' && this.characters.TryExpect('\n'))
                {
                    this.characters.Consume();
                    return Token.NewLine(lineBeforeConsume, colBeforeConsume);
                }

                if (c == '\n')
                {
                    return Token.NewLine(lineBeforeConsume, colBeforeConsume);
                }
            }

            if (this.characters.End) { return this.CreateEof(); }

            int lineAtFirstTokenChar = this.characters.Line;
            int colAtFirstTokenChar = this.characters.Column;

            var token = SymbolsMatcher.TryMatch(this.characters)
                ?? BoolMatcher.TryMatch(this.characters)
                ?? StringMatcher.TryMatch(this.characters)
                ?? LiteralStringMatcher.TryMatch(this.characters)
                ?? IntMatcher.TryMatch(this.characters)
                ?? BareKeyMatcher.TryMatch(this.characters)
                ?? CommentMatcher.TryMatch(this.characters);

            if (token != null)
            {
                return new Token(token.Value.type, token.Value.value) { line = lineAtFirstTokenChar, col = colAtFirstTokenChar };
            }
            else
            {
                return new Token(TokenType.Unknown, this.characters.ConsumeTillWhitespaceOrEnd())
                {
                    line = lineAtFirstTokenChar,
                    col = colAtFirstTokenChar
                };
            }
        }

        private char Peek()
        {
            return this.characters.PeekAt(0);
        }

        private bool PeekInRange(char min, char max)
        {
            var p = this.characters.PeekAt(0);
            return p >= min && p <= max;
        }

        private bool PeekIs(char expected)
        {
            return this.characters.TryExpectAt(0, expected);
        }

        private bool PeekIsBareKeyChar()
        {
            return this.PeekInRange('a', 'z') || this.PeekInRange('A', 'Z') || this.PeekInRange('0', '9') || this.PeekIs('_');
        }

        private char? ReadChar()
        {
            var r = this.reader.Read();
            return r != '\uffff' && r != -1 ? new char?((char)r) : default(char?);
        }
    }
}
