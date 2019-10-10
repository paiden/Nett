namespace Nett
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Extensions;
    using Nett.Parser;

    public sealed class TomlOffsetDateTime : TomlValue<DateTimeOffset>
    {
        private const string SpaceSep = " ";
        private const string TSep = "T";

        private static readonly string[] ParseFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.FFFFFFK", "yyyy-MM-ddTHH:mm:ss.FFFFFFZ", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFF", "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ssK", "yyyy-MM-dd HH:mm:ssZ", "yyyy-MM-dd HH:mm:ss.FFFFFFK", "yyyy-MM-dd HH:mm:ss.FFFFFFZ", "yyyy-MM-dd HH:mm:ssK", "yyyy-MM-dd HH:mm:ss.FFFFFF",
        };

        private static readonly string[] LocalParseFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.FFFFFF",
        };

        private readonly string sepFmt;
        private readonly string offsetFmt;

        internal TomlOffsetDateTime(ITomlRoot root, DateTimeOffset value)
            : this(root, value, " ", string.Empty)
        {
        }

        private TomlOffsetDateTime(ITomlRoot root, DateTimeOffset value, string sepFmt, string offsetFmt)
            : base(root, value)
        {
            this.sepFmt = sepFmt;
            this.offsetFmt = offsetFmt;
        }

        public override string ReadableTypeName => "date time";

        public override TomlObjectType TomlType => TomlObjectType.DateTime;

        public override string ToString()
        {
            var o = this.Value.Offset.Ticks != 0 ? "K" : this.offsetFmt;
            return this.Value.ToString($"yyyy-MM-dd{this.sepFmt}HH:mm:ss.FFFFFF{o}");
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlOffsetDateTime Parse(ITomlRoot root, string s)
        {
            Debug.Assert(s != null);

            const int offsetCharPos = 19;
            string sep = s.Contains(TSep) ? TSep : SpaceSep;
            bool hasOffset = s.LastIndexOf("+") == offsetCharPos || s.LastIndexOf("-") == offsetCharPos;
            var off = hasOffset ? "K" : "Z";
            var value = DateTimeOffset.ParseExact(s, ParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);

            return new TomlOffsetDateTime(root, value, sep, off);
        }

        internal static TomlOffsetDateTime FromLocal(ITomlRoot root, Token tkn)
        {
            Debug.Assert(tkn.Type == TokenType.LocalTime);

            var i = $"0001-01-01T{tkn.Value}";
            var value = DateTimeOffset.ParseExact(i, LocalParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            return new TomlOffsetDateTime(root, value);
        }

        internal TomlOffsetDateTime CloneDateTimeFor(ITomlRoot root) => CopyComments(new TomlOffsetDateTime(root, this.Value), this);

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneDateTimeFor(root);
    }
}
