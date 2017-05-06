using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Newtonsoft.Json;

namespace Nett.UnitTests.Util
{
    [ExcludeFromCodeCoverage]
    public static class AssertionExtensions
    {
        /// <summary>
        /// Checks that the readable contents of two string is equivalent but ignores all whitespaces so that text formatting is not checked.
        /// </summary>
        /// <remarks>
        /// Should only be used for complex tests where it is very hard to produce test data and expected data with correct white spaces.
        /// There should be dedicated tests to check white space formatting, for the other test, only the unformatted content of the
        /// serialized stuff should be of relevance. Also it would be nice if this method would at least consider the line breaks.
        /// </remarks>
        public static void ShouldBeSemanticallyEquivalentTo(this string sx, string sy)
        {
            sx.StripWhitespace().Should().Be(sy.StripWhitespace());
        }

        public static void SouldBeEqualByJsonCompare(this object x, object y)
        {
            var xj = JsonConvert.SerializeObject(x);
            var yj = JsonConvert.SerializeObject(y);

            xj.Should().Be(yj);
        }
    }
}
