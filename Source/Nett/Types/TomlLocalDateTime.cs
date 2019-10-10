using System;
using System.Globalization;
using Nett.Extensions;

namespace Nett
{
    public sealed class TomlLocalDateTime : TomlValue<DateTime>
    {
        internal TomlLocalDateTime(ITomlRoot root, DateTime dt)
            : base(root, dt)
        {
        }

        public override string ReadableTypeName
            => "Local date time";

        public override TomlObjectType TomlType
            => TomlObjectType.LocalDateTime;

        public override void Visit(ITomlObjectVisitor visitor)
            => visitor.Visit(this);

        public override string ToString()
            => this.Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFF");

        internal static TomlLocalDateTime Parse(ITomlRoot root, string value)
        {
            return new TomlLocalDateTime(root, DateTime.Parse(value, CultureInfo.InvariantCulture));
        }

        internal override TomlObject CloneFor(ITomlRoot root)
            => this.CloneLocalDateTimeFor(root);

        internal TomlLocalDateTime CloneLocalDateTimeFor(ITomlRoot root)
            => CopyComments(this.NewLocalDateTimeWithRoot(root), this);

        private TomlLocalDateTime NewLocalDateTimeWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));
            return new TomlLocalDateTime(root, this.Value);
        }
    }
}
