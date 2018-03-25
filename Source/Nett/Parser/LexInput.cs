using System;
using System.Collections.Generic;
using System.Text;

namespace Nett.Parser
{
    internal static class LexInputExtensions
    {
        public static string Consume(this LexInput input, int len)
        {
            var sb = new StringBuilder(len);
            for (int i = 0; i < len; i++)
            {
                sb.Append(input.Consume());
            }

            return sb.ToString();
        }
    }

    internal sealed class LexInput
    {
        public const char Unused = char.MinValue;
        public const char EofChar = char.MaxValue;

        private const int MaxLa = 256;

        private readonly StringBuilder emitBuffer = new StringBuilder(1024);
        private readonly string input;
        private readonly int originalLength;
        private readonly ValueScopeTracker scopeTracker;

        private int tokenStart = 0;
        private int index = 0;

        private int line = 1;
        private int column = 1;

        private int tokenLine = 1;
        private int tokenCol = 1;

        public LexInput(string input, Action<char> lvalueAction, Action<char> rValueAction)
        {
            this.originalLength = input.Length;
            this.input = input + new string(EofChar, MaxLa);
            this.scopeTracker = new ValueScopeTracker(lvalueAction, rValueAction);
        }

        public int Position => this.index;

        public Action<char> LexValueState
            => this.scopeTracker.ScopeAction;

        public bool Eof
            => this.index >= this.originalLength;

        public char Back(int n)
            => this.input[this.index - n];

        public char Peek()
            => this.input[this.index];

        public char Peek(int n)
            => this.input[this.index + n];

        public string PeekString(int n)
        {
            int len = this.index + n < this.input.Length
                ? n
                : this.input.Length - this.index;

            return this.input.Substring(this.index, len);
        }

        public string PeekEmit()
            => this.emitBuffer.ToString();

        public IEnumerable<Token> Emit(TokenType type)
        {
            return this.EmitInternal(new Token(type, this.PeekEmit())
            {
                line = this.tokenLine,
                col = this.tokenCol,
            });
        }

        public char Consume()
        {
            char c = this.Advance();

            if (c != EofChar)
            {
                this.emitBuffer.Append(c);
            }

            return c;
        }

        public void Skip(int n = 1)
        {
            for (int i = 0; i < n; i++)
            {
                this.Advance();
            }
        }

        internal IEnumerable<Token> EmitUnknown(string errorHint)
        {
            for (char c = this.Consume(); !c.IsTokenSepChar(); c = this.Consume()) { /* consume loop */ }

            return this.EmitInternal(Token.Unknown(this.PeekEmit(), errorHint, this.tokenLine, this.tokenCol));
        }

        private char Advance()
        {
            this.column++;

            var c = this.input[this.index++];
            if (c == '\n')
            {
                this.AdvanceLine();
            }

            return c;
        }

        private IEnumerable<Token> EmitInternal(Token mainToken)
        {
            yield return mainToken;

            foreach (var t in this.ConsumeWhitepsaces())
            {
                yield return t;
            }

            this.emitBuffer.Clear();
            this.scopeTracker.Emit(mainToken.type);
            this.SetTokenStartLocation();
        }

        private void SetTokenStartLocation()
        {
            this.tokenLine = this.line;
            this.tokenCol = this.column;
            this.tokenStart = this.index;
        }

        private IEnumerable<Token> ConsumeWhitepsaces()
        {
            char c;
            while ((c = this.Peek()).IsWhitespaceChar())
            {
                if (c == '\n')
                {
                    int offset = this.index > 0 && this.input[this.index - 1] == '\r' ? -1 : 0;
                    yield return Token.NewLine(this.line, this.column + offset);
                }

                this.Consume();
            }
        }

        private void AdvanceLine()
        {
            this.line++;
            this.column = 1;
        }
    }
}
