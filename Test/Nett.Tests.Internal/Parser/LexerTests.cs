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
            var l = CreateLexer("key");

            // Act
            var r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.BareKey, "key", 1, 1)
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
            var l = CreateValueLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Integer, input)
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
            var l = CreateValueLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Float, input)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void Lex_WithBoolInput_ProducesBoolTokens(string token)
        {
            // Arrange
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Bool, token)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\"X\"", "X")]
        [InlineData("\"X\\\"\"", "X\\\"")]
        public void Lex_WithStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.String, expected)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("''", "")]
        [InlineData("'X'", "X")]
        [InlineData(@"'X\\'", @"X\\")]
        public void Lex_WithLiteralStringInput_ProducesCorrectTokens(string token, string expected)
        {
            // Arrange
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.LiteralString, expected)
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
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.MultilineLiteralString, expected)
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
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.MultilineString, expected)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("1979-05-27T07:32:00Z")]
        [InlineData("1979-05-27T00:32:00-07:00")]
        [InlineData("1979-05-27T00:32:00.999999-07:00")]
        public void Lex_WithDateTimeOffsetInput_ProducesCorrectTokens(string token)
        {
            // Arrange
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.DateTime, token)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("07:32:00")]
        [InlineData("00:32:00.999999")]
        [InlineData("11:01:59")]
        public void Lex_LocalTimeTime_ProducesCorrectTokens(string token)
        {
            // Arrange
            var l = CreateValueLexer(token);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.LocalTime, token)
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
            var l = CreateValueLexer(input);

            // Act
            var r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(expectedTokenType, expectedValue)
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
            var l = CreateValueLexer(input);

            // Act
            var r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(expectedTokenType, input)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("# Comment")]
        [InlineData("#Comment")]
        public void Lex_WithComment_ProducesCommentTokensWithoutCommentChar(string input)
        {
            // Arrange
            var l = CreateLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.Comment, input.Substring(1));
        }

        [Fact]
        public void Lex_TableKey_ProducesCorrectTokens()
        {
            // Arrange
            var lexer = CreateLexer("[key]");

            // Act
            var tokens = lexer.Lex();

            // Assert
            tokens
                .AssertNextTokenIs(TokenType.LBrac, "[", 1, 1)
                .AssertNextTokenIs(TokenType.BareKey, "key", 1, 2)
                .AssertNextTokenIs(TokenType.RBrac, "]", 1, 5)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_KeyValuePair_ProducesCorrectTokens()
        {
            // Arrange
            var lexer = CreateLexer("x = 1");

            // Act
            var tokens = lexer.Lex();

            // Assert
            tokens.AssertNextTokenIs(TokenType.BareKey, "x", 1, 1)
                .AssertNextTokenIs(TokenType.Assign, "=", 1, 3)
                .AssertNextTokenIs(TokenType.Integer, "1", 1, 5);
        }

        [Fact]
        public void Lex_IntArray_ProducesCorrectTokens()
        {
            // Arrange
            var lexer = CreateLexer("x = [1, 2, 3]");

            // Act
            var tokens = lexer.Lex();

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
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_InlineTableWithArray_ProducesCorrectTokens()
        {
            // Arrange
            var l = CreateLexer("p = { x = [1], y = 2, z = 3 },");

            // Act
            var r = l.Lex();

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
                TokenType.Comma);

        }

        [Fact]
        public void Lex_NewlinesInput_ProducesCorrectTokens()
        {
            // Arrange
            var l = CreateLexer("\r\n   \r\n   \n");

            // Act
            var t = l.Lex();

            // Assert
            t.AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.NewLine)
                .AssertNextTokenIs(TokenType.NewLine)
                .AssertNoMoreTokens();
        }

        [Fact]
        public void Lex_WithTableKeyInput_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "[A]";
            var l = CreateLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.LBrac)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.RBrac);
        }

        [Fact]
        public void Lex_WithNestedTableKeyInput_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "[A.B]";
            var l = CreateLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.AssertNextTokenIs(TokenType.LBrac)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.Dot)
                .AssertNextTokenIs(TokenType.BareKey)
                .AssertNextTokenIs(TokenType.RBrac);
        }

        [Fact]
        public void Lex_WithLiteralStringsInsideRValue_ProducesCorrectTokens()
        {
            // Arrange
            const string input = "A = { 'X' = false, 'Y' = true }";
            var l = CreateLexer(input);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(3)
                .AssertNextTokenIs(TokenType.LiteralString)
                .AssertNextTokenIs(TokenType.Assign)
                .AssertNextTokenIs(TokenType.Bool)
                .AssertNextTokenIs(TokenType.Comma)
                .AssertNextTokenIs(TokenType.LiteralString)
                .Skip(3)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("--0")]
        [InlineData("00")]
        //[InlineData("1X")]
        public void Lex_InvalidValueTokens_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
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
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
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
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
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
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
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
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
                .AssertNoMoreTokens();
        }

        [Theory]
        [InlineData("'''A \r\n\r\n")]
        [InlineData("\"\"\"A\r\n\n\n")]
        public void Lex_UnclosedMLineStrings_ProducesUnknownTokensAsResult(string tkn)
        {
            // Arrange
            var l = CreateValueLexer(tkn);

            // Act
            var r = l.Lex();

            // Assert
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown)
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
            var l = CreateValueLexer(input);

            // Act
            var r = l.Lex();

            // Asset
            r.Skip(2)
                .AssertNextTokenIs(TokenType.Unknown, input)
                .AssertNoMoreTokens();
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
            foreach (var t in types)
            {
                tokens = tokens.AssertNextTokenIs(t);
            }

            tokens.AssertNoMoreTokens();

        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' is expected");

            tokens.First().type.Should().Be(type);

            return tokens.Skip(1);
        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type, string value)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' and value '{value}' is expected");

            tokens.First().type.Should().Be(type);
            tokens.First().value.Should().Be(value);

            return tokens.Skip(1);
        }

        public static IEnumerable<Token> AssertNextTokenIs(this IEnumerable<Token> tokens, TokenType type, string value, int line, int col)
        {
            tokens.Should().NotBeEmpty($"a token with type '{type}' and value '{value}' is expected");

            tokens.First().type.Should().Be(type);
            tokens.First().value.Should().Be(value);
            tokens.First().line.Should().Be(line);
            tokens.First().col.Should().Be(col);

            return tokens.Skip(1);
        }

        public static void AssertNoMoreTokens(this IEnumerable<Token> tokens)
        {
            tokens.Should().BeEmpty("no more tokens are expected");
        }
    }
}
