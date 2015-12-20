using System;
using System.Diagnostics;
using System.Globalization;

namespace Nett
{
    public sealed class TomlDateTime : TomlValue<DateTimeOffset>
    {
        private static readonly string[] ParseFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.FFFFFFK", "yyyy-MM-ddTHH:mm:ss.FFFFFFZ", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFF", "yyyy-MM-dd",
        };
        public override string ReadableTypeName => "date time";

        public TomlDateTime(DateTimeOffset value)
            : base(value)
        {
        }

        internal static TomlDateTime Parse(string s)
        {
            Debug.Assert(s != null);
            var value = DateTimeOffset.ParseExact(s, ParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new TomlDateTime(value);
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString() => this.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFK");
    }
}
