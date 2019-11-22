using System;
using FluentAssertions.Primitives;

namespace FluentAssertions
{
    public class ST
    {
        private readonly Func<string, string>[] transforms;

        public static readonly ST None = Do();

        public static Func<string, string> Trim = s => s.Trim();
        public static Func<string, string> Norm = s => s.Replace("\r\n", "\n");
        public static Func<string, string> NoSpc = s => s.Replace(" ", "");

        public ST(Func<string, string>[] transforms)
        {
            this.transforms = transforms;
        }

        public static ST Do(params Func<string, string>[] transforms)
        {
            return new ST(transforms ?? new Func<string, string>[0]);
        }

        public string Transform(string x)
        {
            foreach (var t in this.transforms)
            {
                x = t(x);
            }

            return x;
        }
    }

    public static class CustomStringAssertions
    {
        public static AndConstraint<StringAssertions> BeAfterTransforms(
            this StringAssertions a,
            ST transforms,
            string expected,
            string because = "",
            params object[] becauseArgs)
        {
            var t = transforms ?? ST.None;
            var x = t.Transform(a.Subject);
            var y = t.Transform(expected);

            x.Should().Be(y, because, becauseArgs);

            return new AndConstraint<StringAssertions>(a);
        }
    }
}
