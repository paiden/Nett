using System.IO;
using System.Text;

namespace Nett.Parser
{
    class Tokenizer
    {
        private const int SBS = 256;

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
            StringBuilder sb = new StringBuilder(SBS);

            switch (this.characters.La(0))
            {
                case '\"': sb.Append(this.characters.Consume()); return StringToken(sb);
            }

            return new Token?();
        }

        private Token StringToken(StringBuilder sb)
        {
            while (!this.characters.LaIs(0, '\"'))
            {
                sb.Append(this.characters.Consume());
                if (this.characters.LaIs(0, '\\'))
                {
                    sb.Append(this.characters.Consume());
                    sb.Append(this.characters.Consume());
                }
            }

            sb.Append(this.characters.Consume());

            return new Token(TokenType.NormalString, sb.ToString());
        }
    }
}
