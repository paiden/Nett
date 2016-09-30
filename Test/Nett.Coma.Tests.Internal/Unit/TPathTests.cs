using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Nett.UnitTests.Util;
using Xunit;

namespace Nett.Coma.Tests.Internal.Unit
{
    [ExcludeFromCodeCoverage]
    public sealed class TPathTests
    {
        public static IEnumerable<object[]> ValidParseData
        {
            get
            {
                yield return new object[] { "", new TPath() };
                yield return new object[] { "/X", new TPath().WithKeyAdded("X") };
                yield return new object[] { "/X/Y", new TPath().WithKeyAdded("X").WithKeyAdded("Y") };
                yield return new object[] { "[1]", new TPath().WithIndexAdded(1) };
                yield return new object[] { "/X[1]", new TPath().WithKeyAdded("X").WithIndexAdded(1) };
                yield return new object[] { "/X[1][2]", new TPath().WithKeyAdded("X").WithIndexAdded(1).WithIndexAdded(2) };
                yield return new object[] { "/X[1][2]/Y/Z[3]", new TPath().WithKeyAdded("X").WithIndexAdded(1).WithIndexAdded(2).WithKeyAdded("Y").WithKeyAdded("Z").WithIndexAdded(3) };

            }
        }

        [MTheory(nameof(TPath), nameof(TPath.Parse), "When input data is a parseable path, produces the TPath equivalent")]
        [MemberData(nameof(ValidParseData))]
        public void Parse_ProducesCorrectTPath(string toParse, object expected)
        {
            // Arrange
            TPath expectedPath = (TPath)expected;

            // Act
            var p = TPath.Parse(toParse);

            foreach (var s in p)
            {
                Debug.WriteLine(s);
            }

            var a = p.ToList();
            var b = expectedPath.ToList();

            // Assert
            p.Should().Equal(expectedPath);
        }

        [MTheory(nameof(TPath), nameof(TPath.ToString), "Returns correct string representation")]
        [MemberData(nameof(ValidParseData))]
        public void ToString_ReturnsCorrectStringRep(string input, object _)
        {
            // Arrange
            var p = TPath.Parse(input);

            // Act
            var s = p.ToString();

            // Assert
            s.Should().Be(input);
        }

        public static TheoryData<string> InvalidParseData = new TheoryData<string>()
        {
            "//",
            "///",
            " ",
            " /",
            "convert(/X)",
            "(",
            "X",
        };

        [MTheory(nameof(TPath), nameof(TPath.Parse), "When input is invalid throws a parse exception")]
        [MemberData(nameof(InvalidParseData))]
        public void Parse_WhenInputIsInvalid_ThorwsException(string src)
        {
            // Arrange

            // Act
            Action a = () => TPath.Parse(src);

            // Assert
            a.ShouldThrow<ArgumentException>().WithMessage($"*{src}**no valid TPath*");
        }

        [MFact(nameof(TPath), nameof(TPath.Parse), "When input is null throws ArgNull")]
        public void Parse_WhenInptuIsNull_ThrowsArgNull()
        {
            // Act
            Action a = () => TPath.Parse(null);

            // Assert
            a.ShouldThrow<ArgumentNullException>().WithMessage("*src*");
        }
    }
}
