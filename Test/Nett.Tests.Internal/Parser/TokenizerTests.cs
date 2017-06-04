using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using FluentAssertions;
using Nett.Parser;
using Nett.Tests.Util;
using Nett.Tests.Util.TestData;
using Xunit;

namespace Nett.Tests.Internal.Parser
{
    [ExcludeFromCodeCoverage]
    public class TokenizerTests
    {
        [Fact]
        public void TokenizeSimpleKey()
        {
            // Arrange
            var t = new Tokenizer("key".ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.BareKey);
            tkn.value.Should().Be("key");
        }


        [Theory]
        [InlineData("123")]
        [InlineData("+123")]
        [InlineData("-123")]
        [InlineData("+1_2_3")]
        [InlineData("-1_23")]
        public void TokenizeInteger(string intToken)
        {
            // Arrange
            var t = new Tokenizer(intToken.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.Integer);
            tkn.value.Should().Be(intToken);
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
        public void TokenizeFloat(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.Float);
            tkn.value.Should().Be(token);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public void TokenizeBool(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.Bool);
            tkn.value.Should().Be(token);
        }

        [Theory]
        [InlineData(@"""""", "")]
        [InlineData(@"""X""", "X")]
        [InlineData("\"X\\\"\"", "X\\\"")]
        public void TokenizeString(string token, string expected)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.String);
            tkn.value.Should().Be(expected);
        }

        [Fact]
        public void TokenizeLiteralString_WhenClosingTagMissing_ThrowsException()
        {
            Action a = () => new Tokenizer("'not closed".ToStream());

            a.ShouldThrow<Exception>();
        }

        [Theory]
        [InlineData("''", "")]
        [InlineData("'X'", "X")]
        [InlineData(@"'X\\'", @"X\\")]
        public void TokenizeLiteralString(string token, string expected)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.LiteralString);
            tkn.value.Should().Be(expected);
        }

        [Theory]
        [InlineData("''''''", "")]
        [InlineData("'''X\\'''", @"X\")]
        [InlineData(@"'''
2nd line'''", @"2nd line")]
        public void TokenizeMultilineLiteralString(string token, string expected)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.MultilineLiteralString);
            tkn.value.Should().Be(expected);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"", "")]
        [InlineData("\"\"\"X\\\"\"\"\"", "X\\\"")]
        [InlineData(@"""""""
2nd line""""""", @"
2nd line")]
        public void TokenizeMultilineString(string token, string expected)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.MultilineString);
            tkn.value.Should().Be(expected);
        }

        [Theory]
        [InlineData("1979-05-27T07:32:00Z")]
        [InlineData("1979-05-27T00:32:00-07:00")]
        [InlineData("1979-05-27T00:32:00.999999-07:00")]
        public void TokenizeDateTime(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.DateTime);
            tkn.value.Should().Be(token);
        }

        [Theory]
        [InlineData("0.01:02:03.4")]
        [InlineData("-0.01:02:03.4")]
        public void TokenizeTimespan(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.Timespan);
            tkn.value.Should().Be(token);
        }

        [Fact]
        public void Tokenizer_CanTokenizeCurlyBraces()
        {
            // Arrange + Act
            var t = new Tokenizer("{   }".ToStream());

            // Assert
            t.Tokens.PeekAt(0).type.Should().Be(TokenType.LCurly);
            t.Tokens.PeekAt(0).value.Should().Be("{");

            t.Tokens.PeekAt(1).type.Should().Be(TokenType.RCurly);
            t.Tokens.PeekAt(1).value.Should().Be("}");
        }

        [Theory]
        [InlineData("'X'", TokenType.LiteralString, "X")]
        [InlineData("'''X'''", TokenType.MultilineLiteralString, "X")]
        [InlineData("\"X\"", TokenType.String, "X")]
        [InlineData("\"\"\"X\"\"\"", TokenType.MultilineString, "X")]
        public void Tokenize_WhenStringTokenGiven_RemovesTheStringCharsFromTheTokenValue(
            string input, object expectedType, string expectedValue)
        {
            // Arrange
            TokenType expectedTokenType = (TokenType)expectedType;
            var tknizer = new Tokenizer(input.ToStream());

            // Act
            var tkn = tknizer.Tokens.PeekAt(0);

            // Asset
            tkn.type.Should().Be(expectedTokenType);
            tkn.value.Should().Be(expectedValue);

        }

        [Fact]
        public void TokizingAssignsLineAndColumnNumbersOneBased()
        {
            string input =
@"a
 b
  c";

            var t = new Tokenizer(input.ToStream());

            //a
            t.Tokens.PeekAt(0).line.Should().Be(1);
            t.Tokens.PeekAt(0).col.Should().Be(1);
            // newline
            t.Tokens.PeekAt(1).line.Should().Be(1);
            t.Tokens.PeekAt(1).col.Should().Be(2);

            //b
            t.Tokens.PeekAt(2).line.Should().Be(2);
            t.Tokens.PeekAt(2).col.Should().Be(2);
            //newline
            t.Tokens.PeekAt(3).line.Should().Be(2);
            t.Tokens.PeekAt(3).col.Should().Be(3);

            //c
            t.Tokens.PeekAt(4).line.Should().Be(3);
            t.Tokens.PeekAt(4).col.Should().Be(3);
        }

        internal static IEnumerable<Token> Tokens
        {
            get
            {
                yield return new Token(TokenType.Integer, "0");
                yield return new Token(TokenType.String, "X");
                yield return new Token(TokenType.Float, "2.0");
                yield return new Token(TokenType.Bool, "true");
                yield return new Token(TokenType.LiteralString, "LS");
            }
        }


        public static IEnumerable<object[]> TknData
        {
            get
            {
                var tokens = new List<Token>(Tokens);
                for (int i = 0; i < tokens.Count(); i++)
                {

                    var append = tokens[0];
                    tokens.RemoveAt(0);
                    tokens.Add(append);
                    var tknString = TokensToString(tokens);
                    yield return new object[] { tknString, tokens.ToArray() };

                }
            }
        }

        [Theory]
        [MemberData(nameof(TknData))]
        internal void TokenizeMultipleToken(string tokenString, Token[] tokens)
        {
            // Act
            var t = new Tokenizer(tokenString.ToStream());

            // Assert
            foreach (var tkn in tokens)
            {
                t.Tokens.Peek().type.Should().Be(tkn.type);
                t.Tokens.Peek().value.Should().Be(tkn.value);
                t.Tokens.Consume();
            }
        }

        [Fact]
        public void TokenizeTomlTableName()
        {
            // Arrange + Act
            var t = new Tokenizer(" [   TableName  ]  ".ToStream());

            // Assert.
            t.Tokens.PeekAt(0).type.Should().Be(TokenType.LBrac);
            t.Tokens.PeekAt(0).value.Should().Be("[");

            t.Tokens.PeekAt(1).type.Should().Be(TokenType.BareKey);
            t.Tokens.PeekAt(1).value.Should().Be("TableName");

            t.Tokens.PeekAt(2).type.Should().Be(TokenType.RBrac);
            t.Tokens.PeekAt(2).value.Should().Be("]");
        }

        [Fact]
        public void TokenizeKeyValuePair()
        {
            var t = new Tokenizer("   key =   1929  ".ToStream());

            t.Tokens.PeekAt(0).type.Should().Be(TokenType.BareKey);
            t.Tokens.PeekAt(0).value.Should().Be("key");

            t.Tokens.PeekAt(1).type.Should().Be(TokenType.Assign);
            t.Tokens.PeekAt(1).value.Should().Be("=");

            t.Tokens.PeekAt(2).type.Should().Be(TokenType.Integer);
            t.Tokens.PeekAt(2).value.Should().Be("1929");
        }

        [Fact]
        public void TokenizeIntEmbeddedInArray_ShouldGiveInToken()
        {
            var t = new Tokenizer("[0]".ToStream());

            t.Tokens.PeekAt(1).type.Should().Be(TokenType.Integer);
        }

        [Fact]
        public void TokenizeArraySequence()
        {
            var t = new Tokenizer("a = [0.0]".ToStream());

            t.Tokens.PeekAt(0).type.Should().Be(TokenType.BareKey);
            t.Tokens.PeekAt(1).type.Should().Be(TokenType.Assign);
            t.Tokens.PeekAt(2).type.Should().Be(TokenType.LBrac);
            t.Tokens.PeekAt(3).type.Should().Be(TokenType.Float);
            t.Tokens.PeekAt(4).type.Should().Be(TokenType.RBrac);
        }

        [Theory]
        [InlineData("d = 1e6")]
        [Description("An error in the tokenizer caused this test to produce 5 instead of 3 tokens")]
        public void TokenizeDoubleKeyValuePair(string input)
        {
            var t = new Tokenizer(input.ToStream());

            t.Tokens.Consume();
            t.Tokens.Consume();
            t.Tokens.Consume();
            t.Tokens.End.Should().Be(true);
        }

        [Fact]
        [Description("An error caused tokenizer to produce mixed up tokens")]
        public void TokenizeLongFloats()
        {
            var t = new Tokenizer(TomlStrings.Valid.LongFloats.ToStream());

            t.Tokens.Consume().value.Should().Be("longpi");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("3.141592653589793");
            t.Tokens.Consume().type.Should().Be(TokenType.NewLine);
            t.Tokens.Consume().value.Should().Be("neglongpi");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("-3.141592653589793");
        }

        [Theory]
        [InlineData("1}", "1")]
        [InlineData("1.0}", "1.0")]
        [InlineData("1]", "1")]
        [InlineData("1.0]", "1.0")]
        public void Tokenize_ValuesWithoutSpaceToBraces_ProducesValueTokensCorrectly(string input, string expectedValueToken)
        {
            var t = new Tokenizer(input.ToStream());

            t.Tokens.Consume().value.Should().Be(expectedValueToken);
        }

        [Fact]
        public void Tokenize_InlineTableWithoutSpaces_ProducesCorrectTokens()
        {
            // Act
            var t = new Tokenizer(TomlStrings.Valid.InlineTableNoSpaces.ToStream());

            // Assert
            t.Tokens.Consume().value.Should().Be("<NewLine>");
            t.Tokens.Consume().value.Should().Be("[");
            t.Tokens.Consume().value.Should().Be("Test");
            t.Tokens.Consume().value.Should().Be("]");
            t.Tokens.Consume().value.Should().Be("<NewLine>");
            t.Tokens.Consume().value.Should().Be("InlineTable");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("{");
            t.Tokens.Consume().value.Should().Be("test");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("1");
            t.Tokens.Consume().value.Should().Be("}");
        }

        private static string TokensToString(IEnumerable<Token> tokens)
        {
            var sb = new StringBuilder();
            foreach (var tk in tokens)
            {
                sb.Append(GetTokenString(tk)).Append(" ");
            }

            return sb.ToString();

            string GetTokenString(Token t)
            {
                switch (t.type)
                {
                    case TokenType.String: return $"\"{t.value}\"";
                    case TokenType.LiteralString: return $"'{t.value}'";
                    case TokenType.MultilineString: return $"\"\"\"{t.value}\"\"\"";
                    case TokenType.MultilineLiteralString: return $"'''{t.value}'''";
                    default: return t.value;
                }
            }
        }
    }
}
