using System;

namespace Nett
{
    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        public override string ReadableTypeName => "timespan";

        public TomlTimeSpan(TimeSpan value)
            : base(value)
        {
        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
