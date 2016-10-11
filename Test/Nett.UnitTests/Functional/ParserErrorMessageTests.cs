using System;
using FluentAssertions;
using Xunit;

namespace Nett.UnitTests.Functional
{
    public sealed class ParserErrorMessageTests
    {
        private const string NoSpecificErrorMessage = "";
        private const string KeyMissingError = "*Key is missing.";
        private const string ValueMissingError = "*Value is missing.";
        private const string StringNotClosedError = "*String not closed.";
        private const string ArrayValueIsMissing = "Array value is missing.";
        private const string ArrayNotClosed = "Array not closed.";
        private const string FloatFractionMissign = "*Fraction of float is missing.";
        private const string FloatMissingExponent = "*Exponent of float is missing.";
        private const string FailedToReadInteger = "*Failed to read integer.*";

        public static TheoryData<string, string, int, int> InputData =
            new TheoryData<string, string, int, int>
            {
                { "1.0", "*Failed to parse key because unexpected token '1.0' was found.", 1, 1 },
                { "X = 2.", FloatFractionMissign, 1, 7 },
                { "X = 2.X", FloatFractionMissign, 1, 7 },
                { "X = 2.1X", "*Failed to read float because fraction '1X' is invalid.", 1, 7 },
                { "X = 2e", FloatMissingExponent, 1, 7 },
                { "X = 2e-", FailedToReadInteger, 1, 8 },
                { "X = 2e-1X", "*Failed to read float because exponent '-1X' is invalid.", 1, 7 },
                //{ "X = 2e01", "Integer is invalid because of leading '0'.", 1, 7 }, // Leading zero not allowed in E-part (currently not working because of bug, will be fixed separately)
                { "X = 2eX", FloatMissingExponent, 1, 7 },
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
                { "X = \"\r\n\"", "*is invalid because it contains newlines.", 1, 5 },
                { "X = [", ArrayValueIsMissing, 1, 6 },
                { "X = [,]", ArrayValueIsMissing, 1, 6 },
                { "X = [1, ,]", ArrayValueIsMissing, 1, 9 }, // Space is no token, so the ',' is the error position => 9 instead of 8
                { "X = [1, 'X']", "*Expected value of type 'int' but value of type 'string' was found.", 1, 9 },
                { "X = [1, 2, []", "*Expected value of type 'int' but value of type 'array' was found.", 1, 12 },
                { "X = [[1, 2], []", ArrayNotClosed, 1, 16 },
                { "X = [[1, 2], ['a', 'b']", ArrayNotClosed, 1, 24 },
                { "X = [[1, 2]['a', 'b']]", ArrayNotClosed, 1, 12 },
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
