using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Nett.Parser.Productions
{
    internal static class ValueProduction
    {
        private static readonly char[] WhitspaceCharSet =
        {
            '\u0009', '\u000A', '\u000B', '\u000D', '\u0020', '\u0085', '\u00A0',
            '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006',
            '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F',
            '\u3000',
        };


        public static TomlObject Apply(IMetaDataStore metaData, TokenBuffer tokens)
        {
            var value = ParseTomlValue(metaData, tokens);

            if (value == null)
            {
                throw new Exception($"Expected a value while parsing key value pair but value incompatible token '{tokens.Peek().value}' of type '{tokens.Peek().type}' was found.");
            }

            return value;
        }

        private static TomlValue ParseTomlValue(IMetaDataStore metaData, TokenBuffer tokens)
        {
            if (tokens.TryExpect(TokenType.Integer)) { return ParseTomlInt(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.Float)) { return ParseTomlFloat(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.DateTime)) { return TomlDateTime.Parse(metaData, tokens.Consume().value); }
            else if (tokens.TryExpect(TokenType.Timespan)) { return new TomlTimeSpan(metaData, TimeSpan.Parse(tokens.Consume().value, CultureInfo.InvariantCulture)); }
            else if (tokens.TryExpect(TokenType.String)) { return ParseStringValue(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.LiteralString)) { return ParseLiteralString(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.MultilineString)) { return ParseMultilineString(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.MultilineLiteralString)) { return ParseMultilineLiteralString(metaData, tokens); }
            else if (tokens.TryExpect(TokenType.Bool)) { return new TomlBool(metaData, bool.Parse(tokens.Consume().value)); }
            else if (tokens.TryExpect(TokenType.LBrac)) { return ParseTomlArray(metaData, tokens); }

            return null;
        }

        private static TomlInt ParseTomlInt(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var token = tokens.Consume();

            if (token.value.Length > 1 && token.value[0] == '0')
            {
                throw new Exception($"Failed to parse TOML int with '{token.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlInt(metaData, long.Parse(token.value.Replace("_", "")));
        }

        private static TomlFloat ParseTomlFloat(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var floatToken = tokens.Consume();

            var check = floatToken.value;
            int startToCheckForZeros = check[0] == '+' || check[0] == '-' ? 1 : 0;

            if (check[startToCheckForZeros] == '0' && check[startToCheckForZeros + 1] != '.')
            {
                throw new Exception($"Failed to parse TOML float with '{floatToken.value}' because it has  a leading '0' which is not allowed by the TOML specification.");
            }

            return new TomlFloat(metaData, double.Parse(floatToken.value.Replace("_", ""), CultureInfo.InvariantCulture));
        }

        private static TomlString ParseStringValue(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();

            Debug.Assert(t.type == TokenType.String);

            if (t.value.Contains("\n"))
            {
                throw new ArgumentException($"String '{t.value}' is invalid because it contains newlines.");
            }

            var s = t.value.TrimNChars(1).Unescape(t);

            return new TomlString(metaData, s, TomlString.TypeOfString.Normal);
        }

        private static TomlString ParseLiteralString(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();

            Debug.Assert(t.type == TokenType.LiteralString);

            var s = t.value.TrimNChars(1);

            return new TomlString(metaData, s, TomlString.TypeOfString.Literal);
        }

        private static TomlString ParseMultilineString(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();
            Debug.Assert(t.type == TokenType.MultilineString);

            var s = t.value.TrimNChars(3);

            // Trim newline following the """ tag immediate
            if (s.Length > 0 && s[0] == '\r') { s = s.Substring(1); }
            if (s.Length > 0 && s[0] == '\n') { s = s.Substring(1); }

            s = ReplaceDelimeterBackslash(s);

            return new TomlString(metaData, s, TomlString.TypeOfString.Multiline);
        }

        private static TomlString ParseMultilineLiteralString(IMetaDataStore metaData, LookaheadBuffer<Token> tokens)
        {
            var t = tokens.Consume();
            Debug.Assert(t.type == TokenType.MultilineLiteralString);

            var s = t.value.TrimNChars(3);

            // Trim newline following the """ tag immediate
            if (s.Length > 0 && s[0] == '\r') { s = s.Substring(1); }
            if (s.Length > 0 && s[0] == '\n') { s = s.Substring(1); }

            return new TomlString(metaData, s, TomlString.TypeOfString.MultilineLiteral);
        }

        private static TomlArray ParseTomlArray(IMetaDataStore metaData, TokenBuffer tokens)
        {
            TomlArray a;

            tokens.ExpectAndConsume(TokenType.LBrac);

            if (tokens.TryExpect(TokenType.RBrac))
            {
                tokens.Consume();
                return new TomlArray(metaData);
            }
            else
            {
                List<TomlValue> values = new List<TomlValue>();
                var v = ParseTomlValue(metaData, tokens);
                values.Add(v);

                while (!tokens.TryExpect(TokenType.RBrac))
                {
                    tokens.ExpectAndConsume(TokenType.Comma);
                    values.Last().Comments.AddRange(CommentProduction.TryParseComments(tokens, CommentLocation.Append));

                    if (!tokens.TryExpect(TokenType.RBrac))
                    {
                        var et = tokens.Peek();
                        v = ParseTomlValue(metaData, tokens);

                        if (v.GetType() != values[0].GetType())
                        {
                            throw new Exception(et.PrefixWithTokenPostion($"Expected array item of type '{values[0].ReadableTypeName}' but item of type '{v.ReadableTypeName}' was found."));
                        }

                        values.Add(v);
                    }
                }

                a = new TomlArray(metaData, values.ToArray());
            }

            a.Last().Comments.AddRange(CommentProduction.TryParseComments(tokens, CommentLocation.Append));
            tokens.ExpectAndConsume(TokenType.RBrac);

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
