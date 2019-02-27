using FluentAssertions;
using Nett.Parser;
using Xunit;

namespace Nett.Tests.Internal.Parser
{
    public sealed class CreateTableTests
    {
        [Fact]
        public void CreateTable_WhenTableHasSingleIntValue_CreatesTableWithSingleIntValue()
        {
            // Arrange
            var input = @"
x = 1
";
            // Act
            var result = CreatTable(input);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Get<int>("x").Should().Be(1);
        }

        [Fact]
        public void CreateTable_WhenTableHasSingleArrayValue_CreatesTableWithSingleArrayValue()
        {
            // Arrange
            var input = @"
x = [1]
";
            // Act
            var result = CreatTable(input);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Get<int[]>("x")[0].Should().Be(1);
        }

        [Fact]
        public void CreateTable_WhenTableHasNestedArrayValue_CreatesTableWithSuchArray()
        {
            // Arrange
            var input = @"
x = [[1], []]
";
            // Act
            var result = CreatTable(input);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Get<TomlArray>("x").Items.Should().HaveCount(2);
            result.Get<TomlArray>("x")[1].Get<TomlArray>().Items.Should().BeEmpty();
        }

        [Fact]
        public void CreateTable_WithSubTable_CreatesTableWithSubTable()
        {
            // Arrange
            var input = @"
[s]
x = 1
";
            // Act
            var result = CreatTable(input);

            // Assert
            result.Rows.Should().HaveCount(1);
            result.Get<TomlTable>("s").Rows.Should().HaveCount(1);
            result.Get<TomlTable>("s").Get<int>("x").Should().Be(1);
        }

        private static TomlTable CreatTable(string input)
        {
            var lexer = new Lexer(input);
            var tokens = lexer.Lex();
            var parser = new Nett.Parser.Parser(new ParseInput(tokens), TomlSettings.DefaultInstance);
            return parser.Parse().SyntaxNodeOrDefault().CreateTable();
        }
    }
}
