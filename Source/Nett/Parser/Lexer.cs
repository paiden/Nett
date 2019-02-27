using System;
using System.Collections.Generic;
using System.Linq;

namespace Nett.Parser
{
    internal sealed class Lexer
    {
        private const int StartLexDuration = -1;
        private const int InvalidDurationUnit = 100;
        private const string ErrStringNotClosed = "String not closed.";
        private const string ErrNewlineInString = "Single line string contains newlines.";
        private const string ErrFractionMissing = "Fraction of float is missing.";

        private static readonly List<Func<char, char, bool>> DurationUnitsOrdered
            = new List<Func<char, char, bool>>()
            {
                (x, y) => x == 'd',
                (x, y) => x == 'h',
                (x, y) => x == 'm' && y != 's',
                (x, y) => x == 's',
                (x, y) => x == 'm' && y == 's',
                (x, y) => x == 'u' && y == 's',
            };

        private readonly LexInput input;
        private readonly List<Token> lexed = new List<Token>();

        private Action<char> lexerState;
        private char la;

        public Lexer(string input)
        {
            this.input = new LexInput(input, this.LexLValue, this.LexRValue);
            this.lexerState = this.input.LexValueState;
        }

        public List<Token> Lex()
        {
            this.LexAllInput();
            return this.lexed;
        }

        private static TokenType GetMultilineStringType(char t)
            => t == '\'' ? TokenType.MultilineLiteralString : TokenType.MultilineString;

        private static TokenType GetStringType(char t)
            => t == '\'' ? TokenType.LiteralString : TokenType.String;

        private static string ErrLexNum(char c)
            => $"Encountered unexpected character '{c}' while reading a number.";

        private void LexAllInput()
        {
            this.la = this.Peek(1);

            for (char c = this.Peek(); c != LexInput.EofChar; c = this.Peek())
            {
                var preStatePos = this.input.Position;
                var preStatetEmit = this.lexed.Count;

                this.lexerState(c);

                bool stateConsumed = preStatePos != this.input.Position;
                bool stateEmited = preStatetEmit != this.lexed.Count;

                if (!stateConsumed && !stateEmited)
                {
                    this.Consume();
                }
            }

            this.lexerState(LexInput.EofChar);

            if (this.lexed.Last().Type != TokenType.Eof)
            {
                this.lexerState(LexInput.EofChar);
            }
        }

        private void LexLValue(char c)
        {
            if (c == '[') { this.Accept(TokenType.LBrac, this.Consume); }
            else if (c == ']') { this.Accept(TokenType.RBrac, this.Consume); }
            else if (c == '}') { this.Accept(TokenType.RCurly, this.Consume); }
            else if (c == ',') { this.Accept(TokenType.Comma, this.Consume); }
            else if (c == '=') { this.Accept(TokenType.Assign, this.Consume); }
            else if (c == '.') { this.Accept(TokenType.Dot, this.Consume); }
            else if (c == '\r') { this.SkipChar(); }
            else if (c == '\n') { this.Accept(TokenType.NewLine, this.Consume); }
            else if (c == '#') { this.EnterState(this.LexComment, this.SkipChar); }
            else if (c == '\"') { this.EnterState(this.LexDoubleQuotedKey, label: this.SkipChar); }
            else if (c == '\'') { this.EnterState(this.LexSingleQuotedKey, label: this.SkipChar); }
            else if (c == LexInput.EofChar) { this.Accept(TokenType.Eof); }
            else if (c.IsBareKeyChar()) { this.EnterState(this.LexBareKey); }
            else if (c == '(') { this.EnterState(this.LexAnyCharFrag, label: this.SkipChar); }
            else { this.Fail($"Encountered unexpected char '{c}' while lexing RValue"); }
        }

        private void LexBareKey(char c)
        {
            if (c.IsBareKeyChar()) { this.Continue(); }
            else if (c == '.' || c.IsTokenSepChar()) { this.Accept(TokenType.BareKey); }
            else { this.Fail($"Encountered unexpected char '{c}' while lexing key"); }
        }

        private void LexDoubleQuotedKey(char c)
        {
            if (c == '"') { this.Accept(TokenType.DoubleQuotedKey, this.SkipChar); }
            else { this.Continue(); }
        }

        private void LexSingleQuotedKey(char c)
        {
            if (c == '\'') { this.Accept(TokenType.SingleQuotedKey, this.SkipChar); }
            else { this.Continue(); }
        }

        private void LexAnyCharFrag(char c)
        {
            if (c == ')') { this.Accept(TokenType.Unit, this.SkipChar); }
            else { this.Continue(); }
        }

        private void LexRValue(char c)
        {
            if (this.TryLexStringRValue("true")) { this.Accept(TokenType.Bool); }
            else if (this.TryLexStringRValue("false")) { this.Accept(TokenType.Bool); }
            else if (this.TryLexStringRValue("+inf")) { this.Accept(TokenType.Float); }
            else if (this.TryLexStringRValue("-inf")) { this.Accept(TokenType.Float); }
            else if (this.TryLexStringRValue("inf")) { this.Accept(TokenType.Float); }
            else if (this.TryLexStringRValue("+nan")) { this.Accept(TokenType.Float); }
            else if (this.TryLexStringRValue("-nan")) { this.Accept(TokenType.Float); }
            else if (this.TryLexStringRValue("nan")) { this.Accept(TokenType.Float); }
            else if (c == '=') { this.Accept(TokenType.Assign, this.Consume); }
            else if (c == '+' || c == '-') { this.EnterState(this.LexIntNumberFirstDigit); }
            else if (c == '0') { this.EnterState(this.LexLeadingZeroRemainder); }
            else if (c.InRange('1', '9')) { this.EnterState(this.LexIntAll); }
            else if (c == '\"') { this.EnterState(this.LexBasicString, label: this.SkipChar); }
            else if (c == '\'') { this.EnterState(this.LexLiteralString, label: this.SkipChar); }
            else if (c == '\r') { this.Continue(); }
            else if (c == '\n') { this.Accept(TokenType.NewLine, this.Consume); }
            else if (c == '[') { this.Accept(TokenType.LBrac, this.Consume); }
            else if (c == ']') { this.Accept(TokenType.RBrac, this.Consume); }
            else if (c == '{') { this.Accept(TokenType.LCurly, this.Consume); }
            else if (c == '}') { this.Accept(TokenType.RCurly, this.Consume); }
            else if (c == ',') { this.Accept(TokenType.Comma, this.Consume); }
            else if (c == '#') { this.EnterState(this.LexComment, label: this.SkipChar); }
            else if (c == '(') { this.EnterState(this.LexAnyCharFrag, label: this.SkipChar); }
            else if (c == LexInput.EofChar) { this.Accept(TokenType.Eof); }
            else { this.Fail($"Encountered unexpected char '{c}' while lexing RValue"); }
        }

        private bool TryLexStringRValue(string value)
        {
            if (this.PeekSring(value.Length) == value)
            {
                this.Consume(value.Length);
                return true;
            }

            return false;
        }

        private void LexLeadingZeroRemainder(char c)
        {
            if (c == 'x' && this.la.IsHexChar()) { this.EnterState(this.LexHex); }
            else if (c == 'o' && this.la.IsOctalChar()) { this.EnterState(this.LexOctal); }
            else if (c == 'b' && this.la.IsBinChar()) { this.EnterState(this.LexBinaryNumber); }
            else if (c == '.') { this.EnterState(this.LexFloatFractionFirstDigit); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Integer); }
            else if (this.IsDurationUnit(c, this.la, StartLexDuration, out int next)) { this.EnterNextDurationState(next); }
            else if (c.IsDigit() && this.la == ':')
            {
                this.Consume();
                this.LexLocalTime(TokenType.Unknown);
            }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexIntAll(char c)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c == '_' && this.la.IsDigit()) { this.EnterState(this.LexIntNumber); }
            else if (c == '.') { this.EnterState(this.LexFloatFractionFirstDigit); }
            else if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (c == '-' && this.PeekEmit().Length == "XXXX".Length)
            {
                this.Consume();
                this.LexLocalDate();
            }
            else if (c == ':') { this.LexLocalTime(TokenType.LocalTime); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Integer); }
            else if (this.IsDurationUnit(c, this.la, StartLexDuration, out int next)) { this.EnterNextDurationState(next); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexIntNumberFirstDigit(char c)
        {
            if (c == '0' && this.la.IsDigit()) { this.Fail("Leading zeros are not allowed."); }
            else if (c.IsDigit()) { this.EnterState(this.LexIntNumber); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexIntNumber(char c)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Integer); }
            else if (c == '.' && this.la.IsDigit()) { this.EnterState(this.LexFloatFraction); }
            else if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (c == '_' && this.la.IsDigit()) { this.Continue(); }
            else if (this.IsDurationUnit(c, this.la, StartLexDuration, out int next)) { this.EnterNextDurationState(next); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexLocalDate()
        {
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());
            this.Expect(c => c == '-');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());

            bool isDateTimeSep = this.Peek() == 'T' || this.Peek() == ' ';

            if (isDateTimeSep && this.Peek(1).IsDigit() && this.Peek(2).IsDigit())
            {
                this.Consume(3);
                this.LexLocalTime(TokenType.LocalDateTime);
            }
            else
            {
                this.Accept(TokenType.LocalDate);
            }
        }

        private void LexLocalTime(TokenType first)
        {
            this.Expect(c => c == ':');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());
            this.Expect(c => c == ':');
            this.Expect(c => c.IsDigit());
            this.Expect(c => c.IsDigit());

            if (this.Peek() == '.')
            {
                this.Consume();
                while (this.Peek().IsDigit())
                {
                    this.Consume();
                }
            }

            if (this.Peek() == 'Z')
            {
                this.Consume();
                this.Accept(TokenType.OffsetDateTime);
            }
            else if (this.Peek().Is('-', '+'))
            {
                this.Consume();
                this.Expect(c => c.IsDigit());
                this.Expect(c => c.IsDigit());
                this.Expect(c => c == ':');
                this.Expect(c => c.IsDigit());
                this.Expect(c => c.IsDigit());
                this.Accept(TokenType.OffsetDateTime);
            }
            else
            {
                this.Accept(first != TokenType.Unknown ? first : TokenType.LocalTime);
            }
        }

        private void LexHex(char c)
            => this.LexIntAnySys(c, sc => sc.IsHexChar(), TokenType.HexInteger);

        private void LexOctal(char c)
            => this.LexIntAnySys(c, sc => sc.IsOctalChar(), TokenType.OctalInteger);

        private void LexBinaryNumber(char c)
            => this.LexIntAnySys(c, sc => sc.IsBinChar(), TokenType.BinaryInteger);

        private void LexIntAnySys(char c, Func<char, bool> isSysDigit, TokenType sysType)
        {
            if (isSysDigit(c)) { this.Continue(); }
            else if (c.IsTokenSepChar()) { this.Accept(sysType); }
            else if (c == '_' && isSysDigit(this.la)) { this.Continue(); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexFloatFractionFirstDigit(char c)
        {
            if (c.IsDigit()) { this.EnterState(this.LexFloatFraction); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexFloatFraction(char c)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Float); }
            else if (c == '_' && this.la.IsDigit()) { this.Continue(); }
            else if (c.IsExponent()) { this.EnterState(this.LexFloatExponentFirstDigitOrSign); }
            else if (this.IsDurationUnit(c, this.la, StartLexDuration, out int next)) { this.EnterNextDurationState(next); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexFloatExponentFirstDigitOrSign(char c)
        {
            if (c.Is('+', '-') && this.la.IsDigit()) { this.EnterState(this.LexFloatExponent); }
            else if (c == '0' && this.la.IsDigit()) { this.Fail("Exponent is invalid because of leading '0'."); }
            else if (c.IsDigit()) { this.EnterState(this.LexFloatExponent); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexFloatExponent(char c)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c == '_' && this.la.IsDigit()) { this.Continue(); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Float); }
            else { this.Fail(ErrLexNum(c)); }
        }

        private void LexBasicString(char c)
            => this.LexString(c, '\"', '\\');

        private void LexLiteralString(char c)
            => this.LexString(c, '\'', LexInput.Unused);

        private void LexString(char c, char t, char escape)
        {
            if (this.PeekSequence(t, t))
            {
                this.SkipChar();
                this.EnterState(fc => this.LexMultilineString(fc, t, escape), label: this.SkipChar);
            }
            else if (c == escape)
            {
                this.EnterState(fc => this.LexSingleLineString(fc, t, escape));
                this.Consume();
            }
            else if (c == t) { this.Accept(GetStringType(t), this.SkipChar); }
            else if (c == '\r' || c == '\n') { this.Fail(ErrNewlineInString); }
            else if (c == LexInput.EofChar) { this.Fail(ErrStringNotClosed); }
            else { this.EnterState(fc => this.LexSingleLineString(fc, t, escape)); }
        }

        private void LexSingleLineString(char c, char t, char escape)
        {
            if (c == escape) { this.Consume(2); }
            else if (c == t)
            {
                this.SkipChar();
                this.Accept(GetStringType(t));
            }
            else if (c == '\r' || c == '\n') { this.Fail(ErrNewlineInString); }
            else if (c == LexInput.EofChar) { this.Fail(ErrStringNotClosed); }
            else { this.Continue(); }
        }

        private void LexMultilineString(char c, char t, char escape)
        {
            if (c == escape && this.la != '\r' && this.la != '\n') { this.Consume(2); }
            else if (this.PeekSequence(t, t, t) && this.Peek(3) != t)
            {
                this.SkipChar(3);
                this.Accept(GetMultilineStringType(t));
            }
            else if (c == LexInput.EofChar) { this.Fail(ErrStringNotClosed); }
            else { this.Continue(); }
        }

        private bool IsDurationUnit(char x, char y, int preIndex, out int nextIndex)
        {
            nextIndex = InvalidDurationUnit;

            for (int i = preIndex + 1; i < DurationUnitsOrdered.Count; i++)
            {
                if (DurationUnitsOrdered[i](x, y))
                {
                    nextIndex = i;
                    return true;
                }
            }

            return false;
        }

        private void LexComment(char c)
        {
            if (c.Is('\r', '\n', LexInput.EofChar))
            {
                this.Accept(TokenType.Comment);
            }
        }

        private void LexDurationIntNumberFirstDigit(char c, int prevUnit)
        {
            if (c.IsDigit()) { this.EnterState(sc => this.LexDurationIntNumber(sc, prevUnit)); }
            else if (c.IsTokenSepChar()) { this.Accept(TokenType.Duration); }
            else { this.Fail($"Duration segment needs to start with a number '0' - '9' but '{c}' was found instead."); }
        }

        private void LexDurationIntNumber(char c, int prevUnit)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c == '_' && this.la.IsDigit()) { this.Continue(); }
            else if (c == '.' && this.la.IsDigit()) { this.EnterState(sc => this.LexDurationFloatNumber(sc, prevUnit)); }
            else if (this.IsDurationUnit(c, this.la, prevUnit, out int curUnit)) { this.EnterNextDurationState(curUnit); }
            else { this.Fail($"Duration segment encountered unexpected '{c}'."); }
        }

        private void LexDurationFloatNumber(char c, int prevUnit)
        {
            if (c.IsDigit()) { this.Continue(); }
            else if (c == '_' && this.la.IsDigit()) { this.Continue(); }
            else if (c == '.' && this.la.IsDigit()) { this.EnterState(sc => this.LexDurationFloatNumber(sc, prevUnit)); }
            else if (this.IsDurationUnit(c, this.la, prevUnit, out int curUnit)) { this.EnterNextDurationState(curUnit); }
            else { this.Fail($"Duration segment encountered unexpected '{c}'."); }
        }

        private void EnterNextDurationState(int curUnit)
        {
            if (Is2CharUnit()) { this.Consume(); }

            this.EnterState(c => this.LexDurationIntNumberFirstDigit(c, curUnit));

            bool Is2CharUnit()
                => curUnit == 4 || curUnit == 5;
        }

        private void EnterState(Action<char> state)
            => this.EnterState(state, () => this.Consume());

        private void EnterState(Action<char> state, Action label)
        {
            label();
            this.lexerState = state;
        }

        private void Continue()
            => this.Consume();

        private void Accept(TokenType type, Action label)
        {
            label();
            this.Accept(type);
        }

        private void Accept(TokenType type)
        {
            this.lexed.AddRange(this.input.Emit(type));
            this.lexerState = this.input.LexValueState;
        }

        private void Fail(string errorHint = null)
        {
            this.lexed.AddRange(this.input.EmitUnknown(errorHint));
            this.lexerState = this.input.LexValueState;
        }

        private void Consume()
        {
            this.input.Consume();
            this.la = this.Peek(1);
        }

        private void Consume(int n)
        {
            this.input.Consume(n);
            this.la = this.Peek(1);
        }

        private char Peek()
            => this.input.Peek();

        private char Peek(int n)
            => this.input.Peek(n);

        private string PeekEmit()
            => this.input.PeekEmit();

        private void Expect(Func<char, bool> expectation)
        {
            if (!expectation(this.Peek())) { this.Fail(); }
            else { this.Consume(); }
        }

        private bool PeekSequence(params char[] c)
        {
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != this.input.Peek(i))
                {
                    return false;
                }
            }

            return true;
        }

        private void SkipChar()
        {
            this.SkipChar(1);
            this.la = this.Peek(1);
        }

        private void SkipChar(int n)
        {
            this.input.Skip(n);
            this.la = this.Peek(1);
        }

        private string PeekSring(int len)
             => this.input.PeekString(len);
    }
}
