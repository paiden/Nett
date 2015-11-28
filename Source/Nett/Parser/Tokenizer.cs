using System.IO;
using System.Text;
using Nett.Parser.Matchers;

namespace Nett.Parser
{
    internal sealed class Tokenizer
    {
        private const int SBS = 256;

        private readonly StreamReader reader;
        private readonly CharBuffer characters;
        private readonly TokenBuffer tokens;

        public TokenBuffer Tokens => this.tokens;

        public Tokenizer(Stream sr)
        {
            this.reader = new StreamReader(sr);
            this.characters = new CharBuffer(this.ReadChar, 64);
            this.tokens = new TokenBuffer(this.NextToken, 5);
        }

        private char? ReadChar()
        {
            var r = this.reader.Read();
            return r != '\uffff' && r != -1 ? new char?((char)r) : new char?();
        }

        private Token? NextToken()
        {
            if (this.characters.End)
            {
                return null;
            }

            bool hadNewLine = false;
            while (!this.characters.End && this.characters.TryExpectWhitespace())
            {
                var c = this.characters.Consume();

                if (c == '\n')
                {
                    hadNewLine = true;
                }
            }

            if (this.characters.End)
            {
                return null;
            }

            int line = this.characters.Line;
            int column = this.characters.Column;

            var token = SymbolsMatcher.TryMatch(this.characters)
                ?? BoolMatcher.TryMatch(this.characters)
                ?? StringMatcher.TryMatch(this.characters)
                ?? LiteralStringMatcher.TryMatch(this.characters)
                ?? IntMatcher.TryMatch(this.characters)
                ?? BareKeyMatcher.TryMatch(this.characters)
                ?? CommentMatcher.TryMatch(this.characters);

            if (token != null)
            {
                // TOML defines that no newline is allowed after key. As this parser generally doesn't care about newlines and throws them away
                // I have to do it in a some kind wacky way, by replacing the assignment token by a specialized newline token, that later will
                // cause the key value production to fail, because it doesn't encounter the expected '=' token.
                if (hadNewLine && token.Value.type == TokenType.Assign)
                {
                    return new Token(TokenType.NewLine, "<NewLine>");
                }

                return new Token(token.Value.type, token.Value.value) { line = line, col = column };
            }
            else
            {
                return new Token(TokenType.Unknown, this.characters.ConsumeTillWhitespaceOrEnd()) { line = line, col = column };
            }
        }

        private Token ConsumeStringToken(StringBuilder sb)
        {
            sb.Append(Consume());

            while (!this.characters.TryExpectAt(0, '\"'))
            {
                sb.Append(Consume());
                if (this.characters.TryExpectAt(0, '\\'))
                {
                    sb.Append(Consume());
                    sb.Append(Consume());
                }
            }

            sb.Append(this.characters.Consume());

            return new Token(TokenType.String, sb.ToString());
        }

        private Token ConsumeBareKeyToken(StringBuilder sb)
        {
            while (PeekIsBareKeyChar())
            {
                sb.Append(Consume());
            }

            return new Token(TokenType.BareKey, sb.ToString());
        }

        private bool PeekIsBareKeyChar()
        {
            return PeekInRange('a', 'z') || PeekInRange('A', 'Z') || PeekInRange('0', '9') || PeekIs('_');
        }

        private char Peek()
        {
            return this.characters.PeekAt(0);
        }

        private bool PeekIs(char expected)
        {
            return this.characters.TryExpectAt(0, expected);
        }

        private bool PeekInRange(char min, char max)
        {
            var p = this.characters.PeekAt(0);
            return p >= min && p <= max;
        }

        private char Consume()
        {
            return this.characters.Consume();
        }
    }
}
