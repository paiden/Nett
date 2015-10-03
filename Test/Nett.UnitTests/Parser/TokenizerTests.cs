using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using FluentAssertions;
using Nett.Parser;
using Nett.UnitTests.TestUtil;
using Xunit;

namespace Nett.UnitTests.Parser
{
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
        [InlineData(@"""""")]
        [InlineData(@"""X""")]
        [InlineData("\"X\\\"\"")]
        public void TokenizeString(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.String);
            tkn.value.Should().Be(token);
        }

        [Fact]
        public void TokenizeLiteralString_WhenClosingTagMissing_ThrowsException()
        {
            Action a = () => new Tokenizer("'not closed".ToStream());

            a.ShouldThrow<Exception>();
        }

        [Theory]
        [InlineData("''")]
        [InlineData("'X'")]
        [InlineData(@"'X\\'")]
        public void TokenizeLiteralString(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.LiteralString);
            tkn.value.Should().Be(token);
        }

        [Theory]
        [InlineData("''''''")]
        [InlineData("'''X\\'''")]
        [InlineData(@"'''
2nd line'''")]
        public void TokenizeMultilineLiteralString(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.MultilineLiteralString);
            tkn.value.Should().Be(token);
        }

        [Theory]
        [InlineData("\"\"\"\"\"\"")]
        [InlineData("\"\"\"X\\\"\"\"\"")]
        [InlineData(@"""""""
2nd line""""""")]
        public void TokenizeMultilineString(string token)
        {
            // Arrange
            var t = new Tokenizer(token.ToStream());

            // Act
            var tkn = t.Tokens.PeekAt(0);

            // Assert
            tkn.type.Should().Be(TokenType.MultilineString);
            tkn.value.Should().Be(token);
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
        public void TokizingAssignsLineAndColumnNumbersOneBased()
        {
            string input =
@"x
 x
  x";

            var t = new Tokenizer(input.ToStream());

            t.Tokens.PeekAt(0).line.Should().Be(1);
            t.Tokens.PeekAt(0).col.Should().Be(1);

            t.Tokens.PeekAt(1).line.Should().Be(2);
            t.Tokens.PeekAt(1).col.Should().Be(2);

            t.Tokens.PeekAt(2).line.Should().Be(3);
            t.Tokens.PeekAt(2).col.Should().Be(3);
        }

        internal static IEnumerable<Token> Tokens
        {
            get
            {
                yield return new Token(TokenType.Integer, "0");
                yield return new Token(TokenType.String, "\"X\"");
                yield return new Token(TokenType.Float, "2.0");
                yield return new Token(TokenType.Bool, "true");
                yield return new Token(TokenType.LiteralString, "\'LS\'");
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

            t.Tokens.ItemsAvailable.Should().Be(3);
        }

        [Fact]
        [Description("An error caused tokenizer to produce mixed up tokens")]
        public void TokenizeLongFloats()
        {
            var t = new Tokenizer(TomlStrings.Valid.LongFloats.ToStream());

            t.Tokens.Consume().value.Should().Be("longpi");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("3.141592653589793");
            t.Tokens.Consume().value.Should().Be("neglongpi");
            t.Tokens.Consume().value.Should().Be("=");
            t.Tokens.Consume().value.Should().Be("-3.141592653589793");
        }

        private static string TokensToString(IEnumerable<Token> tokens)
        {
            var sb = new StringBuilder();
            foreach (var tk in tokens)
            {
                sb.Append(tk.value).Append(" ");
            }

            return sb.ToString();
        }
    }
}
