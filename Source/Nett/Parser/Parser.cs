using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nett.Parser
{
    internal sealed class Parser
    {
        private readonly Tokenizer tokenizer;
        private LookaheadBuffer<Token> Tokens => this.tokenizer.Tokens;

        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };

        public Parser(Stream s)
        {
            this.tokenizer = new Tokenizer(s);
        }

        public TomlTable Parse()
        {
            return this.Toml();
        }

        private TomlTable Toml()
        {
            var table = new TomlTable();
            while (!this.Tokens.End && this.Tokens.Peek().type != TokenType.Eof)
            {
                var kvp = KeyValuePair();
                table.Add(kvp.Key, kvp.Value);
            }

            return table;

        }

        private bool Expect(TokenType tt) => this.Tokens.Expect(tt);
        private Token Consume() => this.Tokens.Consume();

        private KeyValuePair<string, TomlObject> KeyValuePair()
        {
            var key = this.Key();
            if (!this.Tokens.Expect(TokenType.Assign))
            {
                throw new Exception($"Failed to parse key value pair because token '{this.Tokens.Peek().value}' was found but '=' was expected.");
            }

            this.Tokens.Consume();

            var value = this.Value();

            return new KeyValuePair<string, TomlObject>(key, value);
        }

        private string Key()
        {
            if (this.Tokens.Expect(TokenType.BareKey)) { return this.Tokens.Consume().value; }
            else if (this.Tokens.Expect(TokenType.String)) { return this.Tokens.Consume().value.Replace("\"", ""); }
            else
            {
                throw new System.Exception($"Failed to parse key because unexpected token '{Tokens.Peek().value}' was found.");
            }
        }

        private TomlObject Value()
        {
            if (this.Tokens.Expect(TokenType.Integer)) { return this.ParseTomlInt(); }
            else if (this.Expect(TokenType.Float)) { return ParseTomlFloat(); }
            else if (this.Expect(TokenType.DateTime)) { return new TomlDateTime(DateTimeOffset.Parse(this.Tokens.Consume().value)); }
            else if (this.Expect(TokenType.String)) { return ParseStringValue(); }
            else if (this.Expect(TokenType.LiteralString)) { return ParseLiteralString(); }
            else if (this.Expect(TokenType.MultilineString)) { return ParseMultilineString(); }
            else if (this.Expect(TokenType.Bool)) { return new TomlBool(bool.Parse(this.Consume().value)); }

            throw new Exception($"Failed to parse TOML file as token '{this.Tokens.Peek().value}' cannot be converted to valid TOML value.");
        }

        private TomlInt ParseTomlInt()
        {
            var token = this.Consume();

            if (token.value.Length > 1 && token.value[0] == '0')
            {
                throw new Exception($"Failed to parse TOML int with '{token.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlInt(long.Parse(token.value.Replace("_", "")));
        }

        private TomlFloat ParseTomlFloat()
        {
            var floatToken = this.Tokens.Consume();

            var check = floatToken.value;
            int startToCheckForZeros = check[0] == '+' || check[0] == '-' ? 1 : 0;

            if (check[startToCheckForZeros] == '0' && check[startToCheckForZeros + 1] != '.')
            {
                throw new Exception($"Failed to parse TOML float with '{floatToken.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlFloat(double.Parse(floatToken.value.Replace("_", ""), CultureInfo.InvariantCulture));
        }

        private TomlString ParseStringValue()
        {
            var t = this.Consume();

            Debug.Assert(t.type == TokenType.String);

            var s = t.value.TrimNChars(1).Unescape();

            return new TomlString(s, TomlString.TypeOfString.Normal);
        }

        private TomlString ParseLiteralString()
        {
            var t = this.Consume();

            Debug.Assert(t.type == TokenType.LiteralString);

            var s = t.value.TrimNChars(1);

            return new TomlString(s, TomlString.TypeOfString.Literal);
        }

        private TomlString ParseMultilineString()
        {
            var t = this.Consume();
            Debug.Assert(t.type == TokenType.MultilineString);

            var s = t.value.TrimNChars(3);

            // Trim newline following the """ tag immediate
            if (s.Length > 0 && s[0] == '\r') { s = s.Substring(1); }
            if (s.Length > 0 && s[0] == '\n') { s = s.Substring(1); }

            s = ReplaceDelimeterBackslash(s);

            return new TomlString(s, TomlString.TypeOfString.Multiline);
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
