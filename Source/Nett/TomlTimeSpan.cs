namespace Nett
{
    using System;

    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        internal TomlTimeSpan(ITomlRoot root, TimeSpan value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "timespan";

        public override TomlObjectType TomlType => TomlObjectType.TimeSpan;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
