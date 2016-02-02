using FluentAssertions;
using Nett.UnitTests.TestUtil;

namespace Nett.UnitTests
{
    public static class AssertionExtensions
    {
        /// <summary>
        /// Checks that the readable contents of two string is equivalent but ignores all whitespaces so that text formatting is not checked.
        /// </summary>
        /// <remarks>
        /// Should only be used for complex tests where it is very hard to produce test data and expected data with correct white spaces.
        /// There should be dedicated tests to check white space formatting, for the other test, only the unformatted content of the
        /// serialized stuff should be of relevance
        /// </remarks>
        public static void ShouldBeSemanticallyEquivalentTo(this string sx, string sy)
        {
            sx.StripWhitespace().Should().Be(sy.StripWhitespace());
        }
    }
}
