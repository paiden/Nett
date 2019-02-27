using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nett.Parser;
using Xunit;

#pragma warning disable S4144 // Methods should not have identical implementations

namespace Nett.Tests.Internal.Parser
{
    public sealed class LexerTests
    {
        [Fact]
        public void Lex_WithSimpleKeyInput_ProducesCorrectTokens()
        {
            // Arrange
            Lexer l = CreateLexer("key");

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.BareKey, "key", 1, 1)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("123")]
        [InlineData("+123")]
        [InlineData("-123")]
        [InlineData("+1_2_3")]
        [InlineData("-1_23")]
        public void Lex_WithValidIntegers_ProducesIntegerToken(string input)
        {
            // Arrange
            Lexer l = CreateValueLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Integer, input)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("+1.0")]
        [InlineData("3.145")]
        [InlineData("-0.01")]
        [InlineData("5e+22")]
        [InlineData("1e6")]
        [InlineData("-2E-2")]
        [InlineData("6.26E-34")]
        [InlineData("9_224_617.445_991_228_313")]
        [InlineData("1e1_000")]
        public void Lex_WithValidFloatInput_ProducesFloatToken(string input)
        {
            // Arrange
            Lexer l = CreateValueLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Float, input)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void Lex_WithBoolInput_ProducesBoolTokens(string token)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Bool, token)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\"X\"", "X")]
        [InlineData("\"X\\\"\"", "X\\\"")]
        public void Lex_WithStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.String, expected)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("''", "")]
        [InlineData("'X'", "X")]
        [InlineData(@"'X\\'", @"X\\")]
        public void Lex_WithLiteralStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.LiteralString, expected)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("''''''", "")]
        [InlineData("'''X\\'''", @"X\")]
        [InlineData(@"'''
2nd line'''", @"
2nd line")]
        public void Lex_WithMultilineLiteralStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.MultilineLiteralString, expected)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"", "")]
        [InlineData("\"\"\"X\\\"\"\"\"", "X\\\"")]
        [InlineData(@"""""""
2nd line""""""", @"
2nd line")] // Lexer should not remove any data (so leave the first newline, TomlObject will be responsible for removing it)
        public void Lex_WithMultilineStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.MultilineString, expected)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("1979-05-27T07:32:00Z")]
        [InlineData("1979-05-27T00:32:00-07:00")]
        [InlineData("1979-05-27T00:32:00.999999-07:00")]
        public void Lex_WithDateTimeOffsetInput_ProducesCorrectTokens(string token)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.OffsetDateTime, token)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("07:32:00")]
        [InlineData("00:32:00.999999")]
        [InlineData("11:01:59")]
        public void Lex_LocalTimeTime_ProducesCorrectTokens(string token)
        {
            // Arrange
            Lexer l = CreateValueLexer(token);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.LocalTime, token)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("'X'", TokenType.LiteralString, "X")]
        [InlineData("'''X'''", TokenType.MultilineLiteralString, "X")]
        [InlineData("\"X\"", TokenType.String, "X")]
        [InlineData("\"\"\"X\"\"\"", TokenType.MultilineString, "X")]
        public void Lex_WhenStringTokenGiven_RemovesTheStringCharsFromTheTokenValue(
            string input, object expectedType, string expectedValue)
        {
            // Arrange
            TokenType expectedTokenType = (TokenType)expectedType;
            Lexer l = CreateValueLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(expectedTokenType, expectedValue)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("0m")]
        [InlineData("1us")]
        [InlineData("1ms")]
        [InlineData("1s")]
        [InlineData("1m")]
        [InlineData("1h")]
        [InlineData("1d")]
        [InlineData("0.1us")]
        [InlineData("0.1ms")]
        [InlineData("0.1s")]
        [InlineData("0.1m")]
        [InlineData("0.1h")]
        [InlineData("0.1d")]
        [InlineData("1d2h3m4s5ms6us")]
        [InlineData("-2.0d3.0h4.0m5.0s6.0ms7.0us")]
        [InlineData("1d2.0h3m4.0s5ms6.0us")]
        [InlineData("1_0h2_0.5m6_0s")]
        public void Lex_ValidDateTimeValue_ProducesCorrectTokens(string input)
        {
            // Arrange
            TokenType expectedTokenType = TokenType.Duration;
            Lexer l = CreateValueLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(expectedTokenType, input)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("# Comment")]
        [InlineData("#Comment")]
        public void Lex_WithComment_ProducesCommentTokensWithoutCommentChar(string input)
        {
            // Arrange
            Lexer l = CreateLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.Comment, input.Substring(1));
        }

        [Fact]
        public void Lex_TableKey_ProducesCorrectTokens()
        {
            // Arrange
            Lexer lexer = CreateLexer("[key]");

            // Act
            List<Token> tokens = lexer.Lex();

            // Assert
            tokens
                .AssertNextTokenIs(TokenType.LBrac, "[", 1, 1)
                .AssertNextTokenIs(TokenType.BareKey, "key", 1, 2)
                .AssertNextTokenIs(TokenType.RBrac, "]", 1, 5)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("\"a.value\"", "a.value", TokenType.DoubleQuotedKey)]
        public void Lex_WithQuotedKeys_ProducesCorrectTokens(string key, string expectedKey, object expectedKeyTokenTypeUntyped)
        {
            // Arrange
            TokenType expectedKeyTokenType = (TokenType)expectedKeyTokenTypeUntyped;
            Lexer lexer = CreateLexer($"{key}=1");

            // Act
            List<Token> tokens = lexer.Lex();

            // Assert
            tokens
                .AssertNextTokenIs(expectedKeyTokenType, expectedKey, 1, 1)
                .AssertNextTokenIs(TokenType.Assign, "=")
                .AssertNextTokenIs(TokenType.Integer, "1")
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_KeyValuePairWithQuotedContainingSpace_ProducesCorrectTokens()
        {
            // Arrange
            Lexer lexer = CreateLexer("\"x a\" = 1");

            // Act
            List<Token> tokens = lexer.Lex();

            // Assert
            tokens.AssertNextTokenIs(TokenType.DoubleQuotedKey, "x a", 1, 1)
                .AssertNextTokenIs(TokenType.Assign, "=", 1, 7)
                .AssertNextTokenIs(TokenType.Integer, "1", 1, 9);
        }

        [Fact]
        public void Lex_KeyValuePair_ProducesCorrectTokens()
        {
            // Arrange
            Lexer lexer = CreateLexer("x = 1");

            // Act
            List<Token> tokens = lexer.Lex();

            // Assert
            tokens.AssertNextTokenIs(TokenType.BareKey, "x", 1, 1)
                .AssertNextTokenIs(TokenType.Assign, "=", 1, 3)
                .AssertNextTokenIs(TokenType.Integer, "1", 1, 5);
        }

        [Fact]
        public void Lex_IntArray_ProducesCorrectTokens()
        {
            // Arrange
            Lexer lexer = CreateLexer("x = [1, 2, 3]");

            // Act
            List<Token> tokens = lexer.Lex();

            // Assert
            tokens.AssertNextTokenIs(TokenType.BareKey, "x")
                .AssertNextTokenIs(TokenType.Assign, "=")
                .AssertNextTokenIs(TokenType.LBrac, "[")
                .AssertNextTokenIs(TokenType.Integer, "1")
                .AssertNextTokenIs(TokenType.Comma, ",")
                .AssertNextTokenIs(TokenType.Integer, "2")
                .AssertNextTokenIs(TokenType.Comma, ",")
                .AssertNextTokenIs(TokenType.Integer, "3")
                .AssertNextTokenIs(TokenType.RBrac, "]")
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_InlineTableWithArray_ProducesCorrectTokens()
        {
            // Arrange
            Lexer l = CreateLexer("p = { x = [1], y = 2, z = 3 },");

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertTokensAre(
                TokenType.BareKey,
                TokenType.Assign,
                TokenType.LCurly,
                TokenType.BareKey,
                TokenType.Assign,
                TokenType.LBrac,
                TokenType.Integer,
                TokenType.RBrac,
                TokenType.Comma,
                TokenType.BareKey,
                TokenType.Assign,
                TokenType.Integer,
                TokenType.Comma,
                TokenType.BareKey,
                TokenType.Assign,
                TokenType.Integer,
                TokenType.RCurly,
                TokenType.Comma,
                TokenType.Eof);

        }

        [Fact]
        public void Lex_NewlinesInput_ProducesCorrectTokens()
        {
            // Arrange
            Lexer l = CreateLexer("\r\n   \r\n   \n");

            // Act
            List<Token> t = l.Lex();

            // Assert
            t.AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_WithTableKeyInput_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "[A]";
            Lexer l = CreateLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.LBrac)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.RBrac);
        }

        [Fact]
        public void Lex_WithNewLineInInput_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "X = \r\n";
            var l = CreateLexer(input);

            // Act
            var t = l.Lex();

            // Assert
            t.AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.Assign)
                .AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_WithNestedTableKeyInput_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "[A.B]";
            Lexer l = CreateLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.LBrac)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.Dot)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.RBrac)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_WithLiteralStringsInsideRValue_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "A = { 'X' = false, 'Y' = true }";
            Lexer l = CreateLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.Assign)
                .AssertNextTokenIs(TokenType.LCurly)
                .AssertNextTokenIs(TokenType.SingleQuotedKey)
                .AssertNextTokenIs(TokenType.Assign)
                .AssertNextTokenIs(TokenType.Bool)
                .AssertNextTokenIs(TokenType.Comma)
                .AssertNextTokenIs(TokenType.SingleQuotedKey)
                .AssertNextTokenIs(TokenType.Assign)
                .AssertNextTokenIs(TokenType.Bool)
                .AssertNextTokenIs(TokenType.RCurly)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("--0")]
        [InlineData("00")]
        //[InlineData("1X")]
        public void Lex_InvalidValueTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("0x")]
        [InlineData("-0x0")]
        [InlineData("+0x0")]
        [InlineData("0xG")]
        [InlineData("0x0X")]
        public void Lex_InvalidHexIntTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("0b")]
        [InlineData("0b2")]
        [InlineData("0b ")]
        [InlineData("-0b0")]
        [InlineData("+0b0")]
        public void Lex_InvalidBinIntTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("0o")]
        [InlineData("0o8")]
        [InlineData("0o ")]
        [InlineData("-0o0")]
        [InlineData("+0o0")]
        public void Lex_InvalidOctalIntTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData(".1")]
        [InlineData("0.")]
        [InlineData("--0.0")]
        [InlineData("++0.0")]
        [InlineData("0. ")]
        [InlineData("1e++6")]
        [InlineData("1e--6")]
        [InlineData("1.e2")]
        [InlineData("1.2__3e5")]

        public void Lex_InvalidFloatTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("'''A \r\n\r\n")]
        [InlineData("\"\"\"A\r\n\n\n")]
        public void Lex_UnclosedMLineStrings_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            Lexer l = CreateValueLexer(tkn);

            // Act
            List<Token> r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("m")]
        [InlineData("0dm")]
        [InlineData("1m1d")]
        [InlineData("1_m")]
        [InlineData("1__1m")]
        [InlineData("1m1")]
        public void Lex_InvalidDateTimeValue_ProducesUnknownToken(string input)
        {
            // Arrange
            Lexer l = CreateValueLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown, input)
                .AssertNextTokenIs(TokenType.Eof)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("x = 100#C")]
        [InlineData("x = 100 #C")]
        public void Lex_WhenCommentApppendedWithoutSpace_ProducesFinalCommentToken(string input)
        {
            // Arrange
            Lexer l = CreateLexer(input);

            // Act
            List<Token> r = l.Lex();

            // Asset
            r.Skip(3).First().Type.Should().Be(TokenType.Comment);
        }

        [Fact]
        public void Lex_WithAnyCharFrag_LexesCorrecTokens()
        {
            // Arrange
            Lexer l = CreateLexer("x=1 ($)");

            // Act
            List<Token> r = l.Lex();

            // Asset
            r.Skip(3).First().Type.Should().Be(TokenType.Unit);
        }

        private static Lexer CreateValueLexer(string valueToken)
            => new Lexer($"X={valueToken}");

        private static Lexer CreateLexer(string input)
            => new Lexer(input);
    }

    internal static class TokenListAssertions
    {
        public static void AssertTokensAre(this IEnumerable<Token> tokens, params TokenType[] types)
        {
            foreach (TokenType t in types)
            {
                tokens = tokens.AssertNextTokenIs(t);
            }

            tokens.AssertNoMoreTokens();

        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' is expected");

            tokens.First().Type.Should().Be(type);

            return tokens.Skip(1);
        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type, string value)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' and value '{value}' is expected");

            tokens.First().Type.Should().Be(type);
            tokens.First().Value.Should().Be(value);

            return tokens.Skip(1);
        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type, string value, int line, int col)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' and value '{value}' is expected");

            tokens.First().Type.Should().Be(type);
            tokens.First().Value.Should().Be(value);
            tokens.First().Location.Equals(new SourceLocation(line, col));

            return tokens.Skip(1);
        }

        public static void AssertNoMoreTokens(this IEnumerable<Token> tokens)
        {
            tokens.Should().BeEmpty("no more tokens are expected");
        }
    }
}
