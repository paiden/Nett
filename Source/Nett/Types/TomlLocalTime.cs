using System;
using System.Globalization;

namespace Nett
{
    public sealed class TomlLocalTime : TomlValue<TimeSpan>
    {
        internal TomlLocalTime(ITomlRoot root, TimeSpan value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName
            => "Local Time";

        public override TomlObjectType TomlType
            => TomlObjectType.LocalTime;

        public override void Visit(ITomlObjectVisitor visitor)
            => visitor.Visit(this);

        public override string ToString()
            => this.Value.Milliseconds != 0
                ? this.Value.ToString(@"hh\:mm\:ss\.FFFFFFF", CultureInfo.InvariantCulture)
                : this.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);

        internal static TomlValue Parse(ITomlRoot root, string value)
        {
            return new TomlLocalTime(root, TimeSpan.Parse(value));
        }

        internal override TomlObject CloneFor(ITomlRoot root)
            => new TomlLocalTime(root, this.Value);
    }
}
