using System;
using FluentAssertions;
using Xunit;

namespace Nett.Tests.Functional
{
    public sealed class ParserErrorMessageTests
    {
        private const string FailedToReadNumber = "*Encountered unexpected character '*' while reading a number";
        private const string NoSpecificErrorMessage = "*";
        private const string KeyMissingError = "*Expected TOML table or row key but encountered unexpected token of type 'Assign' with value '='";
        private const string ValueMissingError = "*Expected TOML value*";
        private const string StringNotClosedError = "*String not closed.";
        private const string ArrayValueIsMissing = "Array value is missing.";
        private const string ArrayNotClosed = "Expected array close tag ']' but encountered unexpected token of type '*' with value '*'.";
        private const string FloatFractionMissign = FailedToReadNumber;
        private const string FloatMissingExponent = FailedToReadNumber;
        private const string FailedToReadInteger = "*Failed to read integer.*";
        private const string ExpectedValue = "Expected TOML value but encountered unexpected token of type '*' with value '*'.";



        public static TheoryData<string, string, int, int> InputData { get; } =
            new TheoryData<string, string, int, int>
            {
                { "1.0", NoSpecificErrorMessage, 1, 4 },
                { "X = 2.", FloatFractionMissign, 1, 5 },
                { "X = 2.X", FloatFractionMissign, 1, 5 },
                { "X = 2.1X", FailedToReadNumber, 1, 5 },
                { "X = 2e", FloatMissingExponent, 1, 5 },
                { "X = 2e-", FloatMissingExponent, 1, 5 },
                { "X = 2e-1X", FailedToReadNumber, 1, 5 },
                { "X = 2e01", "*Exponent is invalid because of leading '0'." , 1, 5 },
                { "X = 2eX", FloatMissingExponent, 1, 5 },
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
                { "X = \"\r\n\"", "Single line string contains newlines.", 1, 5 },
                { "X = [", ArrayNotClosed, 1, 6 },
                { "X = [,]", ExpectedValue, 1, 6 },
                { "X = [1, ,]", ExpectedValue, 1, 9 }, // Space is no token, so the ',' is the error position => 9 instead of 8
                { "X = [1, 'X']", "*Expected array value of type 'int' but value of type 'string' was found.", 1, 9 },
                { "X = [1, 2, []", ArrayNotClosed, 1, 14 },
                { "X = [[1, 2], []", ArrayNotClosed, 1, 16 },
                { "X = [[1, 2], ['a', 'b']", ArrayNotClosed, 1, 24 },
                { "X = [[1, 2]['a', 'b']]", ArrayNotClosed, 1, 12 },
                { "X = 1;", "*Encountered unexpected character ';' while reading a number.", 1, 5 },
                { "[X]A", "Expected newline after expression  but encountered unexpected token of type 'BareKey' with value 'A'.", 1, 4 },
            };

        [Theory]
        [MemberData(nameof(InputData))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public static void Parser_WhenInputIsInvalid_GeneratesErrorMessageWithLineAndColumn(string toml, string _, int line, int column)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            // Act
            Action a = () => Toml.ReadString(toml);

            // Assert
            a.ShouldThrow<Exception>().WithMessage($"Line {line}, column {column}:*");
        }

        [Theory]
        [MemberData(nameof(InputData))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public static void Parser_WhenInputIsInvalid_GeneratesSomewhatUsefulErrorMessage(string toml, string error, int _, int __)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            // Act
            Action a = () => Toml.ReadString(toml);

            // Assert
            a.ShouldThrow<Exception>().WithMessage($"*{error}*");
        }
    }
}
