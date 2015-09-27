using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Nett.Parser.Productions
{
    internal sealed class ValueProduction : Production<TomlObject>
    {
        private static readonly char[] WhitspaceCharSet =
{
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public override TomlObject Apply(LookaheadBuffer<Token> tokens)
        {
            return this.ParseTomlValue(tokens);
        }

        private TomlObject ParseTomlValue(LookaheadBuffer<Token> tokens)
        {
            if (tokens.Expect(TokenType.Integer)) { return this.ParseTomlInt(tokens); }
            else if (tokens.Expect(TokenType.Float)) { return ParseTomlFloat(tokens); }
            else if (tokens.Expect(TokenType.DateTime)) { return new TomlDateTime(DateTimeOffset.Parse(tokens.Consume().value)); }
            else if (tokens.Expect(TokenType.String)) { return ParseStringValue(tokens); }
            else if (tokens.Expect(TokenType.LiteralString)) { return ParseLiteralString(tokens); }
            else if (tokens.Expect(TokenType.MultilineString)) { return ParseMultilineString(tokens); }
            else if (tokens.Expect(TokenType.MultilineLiteralString)) { return this.ParseMultilineLiteralString(tokens); }
            else if (tokens.Expect(TokenType.Bool)) { return new TomlBool(bool.Parse(tokens.Consume().value)); }
            else if (tokens.Expect(TokenType.LBrac)) { return this.ParseTomlArray(tokens); }

            return null;
        }

        private TomlInt ParseTomlInt(LookaheadBuffer<Token> tokens)
        {
            var token = tokens.Consume();

            if (token.value.Length > 1 && token.value[0] == '0')
            {
                throw new Exception($"Failed to parse TOML int with '{token.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlInt(long.Parse(token.value.Replace("_", "")));
        }

        private TomlFloat ParseTomlFloat(LookaheadBuffer<Token> tokens)
        {
            var floatToken = tokens.Consume();

            var check = floatToken.value;
            int startToCheckForZeros = check[0] == '+' || check[0] == '-' ? 1 : 0;

            if (check[startToCheckForZeros] == '0' && check[startToCheckForZeros + 1] != '.')
            {
                throw new Exception($"Failed to parse TOML float with '{floatToken.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlFloat(double.Parse(floatToken.value.Replace("_", ""), CultureInfo.InvariantCulture));
        }

        private TomlString ParseStringValue(LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();

            Debug.Assert(t.type == TokenType.String);

            var s = t.value.TrimNChars(1).Unescape();

            return new TomlString(s, TomlString.TypeOfString.Normal);
        }

        private TomlString ParseLiteralString(LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();

            Debug.Assert(t.type == TokenType.LiteralString);

            var s = t.value.TrimNChars(1);

            return new TomlString(s, TomlString.TypeOfString.Literal);
        }

        private TomlString ParseMultilineString(LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();
            Debug.Assert(t.type == TokenType.MultilineString);

            var s = t.value.TrimNChars(3);

            // Trim newline following the """ tag immediate
            if (s.Length > 0 && s[0] == '\r') { s = s.Substring(1); }
            if (s.Length > 0 && s[0] == '\n') { s = s.Substring(1); }

            s = ReplaceDelimeterBackslash(s);

            return new TomlString(s, TomlString.TypeOfString.Multiline);
        }

        private TomlString ParseMultilineLiteralString(LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();
            Debug.Assert(t.type == TokenType.MultilineLiteralString);

            var s = t.value.TrimNChars(3);

            // Trim newline following the """ tag immediate
            if (s.Length > 0 && s[0] == '\r') { s = s.Substring(1); }
            if (s.Length > 0 && s[0] == '\n') { s = s.Substring(1); }

            return new TomlString(s, TomlString.TypeOfString.MultilineLiteral);
        }

        private TomlArray ParseTomlArray(LookaheadBuffer<Token> tokens)
        {
            TomlArray a = new TomlArray();

            var t = tokens.Consume();
            Debug.Assert(t.type == TokenType.LBrac);

            var v = this.ParseTomlValue(tokens);
            if (v != null)
            {
                a.Add(v);

                while (tokens.Expect(TokenType.Comma) && !tokens.ExpectAt(1, TokenType.RBrac))
                {
                    tokens.Consume();

                    v = this.ParseTomlValue(tokens);
                    if (v != null)
                    {
                        a.Add(v);
                    }
                    else
                    {
                        if (tokens.Expect(TokenType.RBrac))
                        {
                            tokens.Consume();
                            return a;
                        }
                        else
                        {
                            throw new Exception($"Failed to parse array. Expected ']' but found '{tokens.Peek().value}'.");
                        }
                    }
                }
            }

            if (tokens.Expect(TokenType.Comma)) { tokens.Consume(); }

            if (tokens.Expect(TokenType.RBrac))
            {
                tokens.Consume();
            }
            else
            {
                throw new Exception($"Failed to parse array. Expected ']' but found '{tokens.Peek().value}'.");
            }

            return a;
        }

        private static string ReplaceDelimeterBackslash(string source)
        {
            for (int d = DelimeterBackslashPos(source, 0); d >= 0; d = DelimeterBackslashPos(source, d))
            {
                var nnw = NextNonWhitespaceCharacer(source, d + 1);
                source = source.Remove(d, nnw - d);
            }

            return source;
        }

        private static int DelimeterBackslashPos(string source, int startIndex)
        {
            int i1 = source.IndexOf("\\\r", startIndex);
            int i2 = source.IndexOf("\\\n", startIndex);

            var minIndex = MinGreaterThan(0, -1, i1, i2);
            return minIndex;
        }

        private static int NextNonWhitespaceCharacer(string source, int startIndex)
        {
            for (int i = startIndex; i < source.Length; i++)
            {
                if (!WhitspaceCharSet.Contains(source[i]))
                {
                    return i;
                }
            }

            return source.Length;
        }

        private static int MinGreaterThan(int absoluteMin, int defaultValue, params int[] values)
        {
            int currenMin = int.MaxValue;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] >= absoluteMin && values[i] < currenMin)
                {
                    currenMin = values[i];
                }
            }

            return currenMin == int.MaxValue ? defaultValue : currenMin;
        }
    }
}
