using System;
using System.Globalization;
using System.Text;
using Nett.Extensions;
using Nett.Parser;
using static System.Math;

namespace Nett
{
    public sealed class TomlDuration : TomlValue<TimeSpan>
    {
        internal TomlDuration(ITomlRoot root, TimeSpan value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "duration";

        public override TomlObjectType TomlType => TomlObjectType.TimeSpan;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.Value < TimeSpan.Zero) { sb.Append('-'); }

            if (this.Value.Days != 0) { sb.Append(Abs(this.Value.Days)).Append("d"); }
            if (this.Value.Hours != 0) { sb.Append(Abs(this.Value.Hours)).Append("h"); }
            if (this.Value.Minutes != 0) { sb.Append(Abs(this.Value.Minutes)).Append("m"); }
            if (this.Value.Seconds != 0) { sb.Append(Abs(this.Value.Seconds)).Append("s"); }
            if (this.Value.Milliseconds != 0) { sb.Append(Abs(this.Value.Milliseconds)).Append("ms"); }

            if (sb.Length <= 0) { sb.Append("0ms"); }

            return sb.ToString();
        }

        // This method is only capable of processing lexer input. Lexer did all validation and error handling already
        // so in this impl. here we know exactly what the input data looks like and do not need to do any checks or specialized
        // handling.
        internal static TomlDuration Parse(ITomlRoot root, string input)
        {
            var value = new StringBuilder(16);
            var unit = new StringBuilder(2);
            TimeSpan result = TimeSpan.Zero;

            int startIndex = input[0] == '+' || input[0] == '-' ? 1 : 0;
            for (int i = startIndex; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '_') { continue; }
                else if (c.IsDigit() || c == '.') { value.Append(input[i]); }
                else { unit.Append(c); }

                if (SegmentDone(i))
                {
                    result += TimeSpanFromTokens(value.ToString(), unit.ToString());
                    value.Clear();
                    unit.Clear();
                }
            }

            bool SegmentDone(int i)
                => unit.Length > 0 && (i == input.Length - 1 || input[i + 1].IsDigit());

            return input[0] == '-' ? new TomlDuration(root, -result) : new TomlDuration(root, result);
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneTimespanFor(root);

        internal TomlDuration CloneTimespanFor(ITomlRoot root) => CopyComments(new TomlDuration(root, this.Value), this);

        private static TimeSpan TimeSpanFromTokens(string value, string unit)
        {
            switch (unit)
            {
                case "d": return ToTimeSpan(TimeSpan.TicksPerDay);
                case "h": return ToTimeSpan(TimeSpan.TicksPerHour);
                case "m": return ToTimeSpan(TimeSpan.TicksPerMinute);
                case "s": return ToTimeSpan(TimeSpan.TicksPerSecond);
                case "ms": return ToTimeSpan(TimeSpan.TicksPerMillisecond);
                case "us": return ToTimeSpan(TimeSpan.TicksPerMillisecond / 1000);
                default: throw new ArgumentException($"Unknown timespan unit '{unit}'.");
            }

            // Work-around for .Net design issue:
            // https://stackoverflow.com/questions/5450439/timespan-frommilliseconds-strange-implementation
            TimeSpan ToTimeSpan(long ticksPer)
                => TimeSpan.FromTicks((long)(double.Parse(value, CultureInfo.InvariantCulture) * ticksPer));
        }
    }
}
