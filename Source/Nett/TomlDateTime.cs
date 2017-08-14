namespace Nett
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Extensions;

    public sealed class TomlDateTime : TomlValue<DateTimeOffset>
    {
        private static readonly string[] ParseFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.FFFFFFK", "yyyy-MM-ddTHH:mm:ss.FFFFFFZ", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFF", "yyyy-MM-dd",
        };

        internal TomlDateTime(ITomlRoot root, DateTimeOffset value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "date time";

        public override TomlObjectType TomlType => TomlObjectType.DateTime;

        public override string ToString() => this.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFK");

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlDateTime Parse(ITomlRoot root, string s)
        {
            Debug.Assert(s != null);
            var value = DateTimeOffset.ParseExact(s, ParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new TomlDateTime(root, value);
        }

        internal TomlDateTime CloneDateTimeFor(ITomlRoot root) => CopyComments(new TomlDateTime(root, this.Value), this);

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneDateTimeFor(root);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.DateTimeWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.DateTimeWithRoot(root);

        internal TomlDateTime DateTimeWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlDateTime(root, this.Value);
        }
    }
}
