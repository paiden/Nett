using System;
using FluentAssertions;
using FluentAssertions.Primitives;
using Nett.Tests.Util;

namespace Nett.Tests
{
    public sealed class NormalizedStringAssertions : ReferenceTypeAssertions<string, NormalizedStringAssertions>
    {
        protected override string Context => throw new NotImplementedException();

        public NormalizedStringAssertions(string subject)
        {
            this.Subject = subject;
        }


        public AndConstraint<NormalizedStringAssertions> Be(
            string expected, string because = "", params object[] becauseArgs)
        {
            expected = expected?.NormalizeLineEndings()?.Trim();

            this.Subject.Should().Be(expected, because, becauseArgs);

            return new AndConstraint<NormalizedStringAssertions>(this);
        }
    }
}
