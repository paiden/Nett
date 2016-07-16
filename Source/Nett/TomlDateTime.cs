namespace Nett
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    public sealed class TomlDateTime : TomlValue<DateTimeOffset>
    {
        private static readonly string[] ParseFormats = new string[]
        {
            "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.FFFFFFK", "yyyy-MM-ddTHH:mm:ss.FFFFFFZ", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFF", "yyyy-MM-dd",
        };

        internal TomlDateTime(IMetaDataStore metaData, DateTimeOffset value)
            : base(metaData, value)
        {
        }

        public override string ReadableTypeName => "date time";

        public override TomlObjectType TomlType => TomlObjectType.DateTime;

        public override string ToString() => this.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFK");

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlDateTime Parse(IMetaDataStore metaData, string s)
        {
            Debug.Assert(s != null);
            var value = DateTimeOffset.ParseExact(s, ParseFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            return new TomlDateTime(metaData, value);
        }
    }
}
