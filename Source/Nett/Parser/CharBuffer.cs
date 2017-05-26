namespace Nett.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal sealed class CharBuffer : LookaheadBuffer<char>
    {
        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public CharBuffer(Func<char?> read, int lookAhead)
            : base(read, lookAhead)
        {
            this.Line = 1;
            this.Column = 1;
        }

        public int Column { get; private set; }

        public int Line { get; private set; }

        public FilePosition FilePosition => new FilePosition() { Line = this.Line, Column = this.Column };

        public override char Consume()
        {
            this.Column++; // TODO adapt counting for non visible characters, ignore for now not important enough

            if (this.Peek() == '\n')
            {
                this.Line++;
                this.Column = 1;
            }

            return base.Consume();
        }

        public IList<char> Consume(int count)
        {
            List<char> c = new List<char>();
            for (int i = 0; i < count; i++)
            {
                c.Add(this.Consume());
            }

            return c;
        }

        public string ConsumeTillWhitespaceOrEnd()
        {
            StringBuilder sb = new StringBuilder();

            while (!this.End && !this.TryExpectWhitespace())
            {
                sb.Append(this.Consume());
            }

            return sb.ToString();
        }

        public char ExpectAndConsume(char c)
        {
            if (this.TryExpect(c))
            {
                return this.Consume();
            }
            else
            {
                throw new Exception($"Expected character '{c}' but '{this.Peek()}' was found.");
            }
        }

        public char ExpectAndConsumeDigit()
        {
            var c = this.ExpectInRange('0', '9');
            this.Consume();
            return c;
        }

        public char ExpectInRange(char min, char max)
        {
            char pv = this.Peek();
            if (pv >= min && pv <= max)
            {
                return pv;
            }
            else
            {
                throw new Exception($"Expected character in range '{min} to {max}' but character '{pv}' was found.");
            }
        }

        public bool TokenDone()
        {
            return this.End || this.TryExpectWhitespace() || this.TryExpect(']') || this.TryExpect(',') || this.TryExpect('}');
        }

        public int TryTrimNewline()
        {
            if (this.PeekAt(0) == '\n')
            {
                this.Consume();
                return 1;
            }
            else if (this.PeekAt(0) == '\r' && this.PeekAt(1) == '\n')
            {
                this.Consume(2);
                return 2;
            }

            return 0;
        }

        public bool TryExpect(string seq)
        {
            if (this.ItemsAvailable < seq.Length)
            {
                return false;
            }

            for (int i = 0; i < seq.Length; i++)
            {
                if (this.PeekAt(i) != seq[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryExpectDigit()
        {
            return this.TryExpectInRange('0', '9');
        }

        public bool TryExpectInRange(char min, char max)
        {
            char pv = this.Peek();
            return pv >= min && pv <= max;
        }

        public bool TryExpectWhitespace()
        {
            if (this.End)
            {
                return false;
            }

            char pv = this.Peek();
            return WhitspaceCharSet.Contains(pv);
        }
    }
}
