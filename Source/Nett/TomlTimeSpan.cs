using System;

namespace Nett
{
    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        public override string ReadableTypeName => "timespan";

        internal TomlTimeSpan(TimeSpan value)
            : base(value)
        {
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
