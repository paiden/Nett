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
    }
}
