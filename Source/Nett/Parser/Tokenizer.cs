using System.IO;
using System.Text;
using Nett.Parser.Matchers;

namespace Nett.Parser
{
    internal sealed class Tokenizer
    {
        private const int SBS = 256;

        private static readonly Token Eof = new Token(TokenType.Eof, null);
        private static readonly MatcherBase[] Matchers = new MatcherBase[]
        {
            new IntMatcher(),
            new BoolMatcher(),
            new StringMatcher(),
        };

        private readonly StreamReader reader;
        private readonly LookaheadBuffer<char> characters;
        private readonly LookaheadBuffer<Token> tokens;

        public LookaheadBuffer<Token> Tokens => this.tokens;

        public Tokenizer(Stream sr)
        {
            this.reader = new StreamReader(sr);
            this.characters = new LookaheadBuffer<char>(this.ReadChar, 64);
            this.tokens = new LookaheadBuffer<Token>(this.NextToken, 5);
        }

        private char? ReadChar()
        {
            var r = this.reader.Read();
            return r != -1 ? new char?((char)r) : new char?();
        }

        private Token? NextToken()
        {
            if (this.characters.End)
            {
                return Eof;
            }

            while (!this.characters.End && this.characters.ExpectWhitespace())
            {
                this.characters.Consume();
            }

            if (this.characters.End)
            {
                return Eof;
            }

            foreach (var m in Matchers)
            {
                var tkn = m.Match(this.characters);

                if (tkn.HasValue)
                {
                    return tkn;
                }
            }

            return new Token(TokenType.Unknown, this.characters.ConsumeTillWhitespaceOrEnd());
        }



        private Token ConsumeStringToken(StringBuilder sb)
        {
            sb.Append(Consume());

            while (!this.characters.ExpectAt(0, '\"'))
            {
                sb.Append(Consume());
                if (this.characters.ExpectAt(0, '\\'))
                {
                    sb.Append(Consume());
                    sb.Append(Consume());
                }
            }

            sb.Append(this.characters.Consume());

            return new Token(TokenType.NormalString, sb.ToString());
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
            return this.characters.ExpectAt(0, expected);
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
