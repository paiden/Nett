using System;
using System.Globalization;

namespace Nett
{
    public sealed class TomlLocalDate : TomlValue<DateTime>
    {
        internal TomlLocalDate(ITomlRoot root, DateTime dt)
            : base(root, dt)
        {
        }

        public override string ReadableTypeName
            => "Local Date";

        public override TomlObjectType TomlType
            => TomlObjectType.LocalDate;

        public override void Visit(ITomlObjectVisitor visitor)
            => visitor.Visit(this);

        public override string ToString()
            => this.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        internal static TomlLocalDate Parse(ITomlRoot root, string s)
        {
            return new TomlLocalDate(root, DateTime.Parse(s));
        }

        internal override TomlObject CloneFor(ITomlRoot root)
        {
            throw new NotImplementedException();
        }
    }
}
