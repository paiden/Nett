using System;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed class ParserErrorMessageTests
    {
        private const string KeyMissingError = "*Key is missing.";
        private const string ValueMissingError = "*Value is missing.";
        private const string StringNotClosedError = "*String not closed.";

        public static TheoryData<string, string, int, int> InputData =
            new TheoryData<string, string, int, int>
            {
                { "1.0", "*Failed to parse key because unexpected token '1.0' was found.", 1, 1 },
                { "X = ", ValueMissingError, 1, 5 },
                { "X = \r\n", ValueMissingError, 1, 5 },
                { "X = \r", ValueMissingError, 1, 6 }, // \r is a omitted char, and after that EOF will cause error => 5 + 1
                { "X = \n", ValueMissingError, 1, 5 },
                { "X = \r\n 2.0", ValueMissingError, 1, 5 },
                { "=1", KeyMissingError, 1, 1 },
                { "=", KeyMissingError, 1, 1 },
                { "X = \"Hello", StringNotClosedError, 1, 5 }, // string errors use string start pos as error indicator position
                { "X = 'Hello", StringNotClosedError, 1, 5 },
                { "X = '''Hello \r\n\r\n", StringNotClosedError, 1, 5 },
                { "X = \"\"\"Hello \r\n\r\n", StringNotClosedError, 1, 5 },
            };

        [Theory(DisplayName = "ErrMsg has correct pos")]
        [MemberData(nameof(InputData))]
        public static void Parser_WhenInputIsInvalid_GeneratesErrorMessageWithLineAndColumn(string toml, string _, int line, int column)
        {
            // Act
            Action a = () => Toml.ReadString(toml);

            // Assert
            a.ShouldThrow<Exception>().WithMessage($"Line {line}, Column {column}:*");
        }

        [Theory(DisplayName = "Useful ErrMsg")]
        [MemberData(nameof(InputData))]
        public static void Parser_WhenInputIsInvalid_GeneratesSomewhatUsefulErrorMessage(string toml, string error, int _, int __)
        {
            // Act
            Action a = () => Toml.ReadString(toml);

            // Assert
            a.ShouldThrow<Exception>().WithMessage($"*{error}*");
        }
    }
}
