using Nett.Parser;
using Xunit;
using Nett.UnitTests.TestUtil;
using FluentAssertions;

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
            tkn.type.Should().Be(TokenType.Key);
        }
    }
}
