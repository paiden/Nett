using System.Collections.Generic;
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
            var tkn = t.Tokens.La(0);

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
            var tkn = t.Tokens.La(0);

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
            var tkn = t.Tokens.La(0);

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
            var tkn = t.Tokens.La(0);

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
            var tkn = t.Tokens.La(0);

            // Assert
            tkn.type.Should().Be(TokenType.NormalString);
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
            var tkn = t.Tokens.La(0);

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
            var tkn = t.Tokens.La(0);

            // Assert
            tkn.type.Should().Be(TokenType.Timespan);
            tkn.value.Should().Be(token);
        }

        internal static IEnumerable<Token> Tokens
        {
            get
            {
                yield return new Token(TokenType.Integer, "0");
                yield return new Token(TokenType.NormalString, "\"X\"");
                yield return new Token(TokenType.Float, "2.0");
                yield return new Token(TokenType.Bool, "true");
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
