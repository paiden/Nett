using System;

namespace Nett
{
    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        public override string ReadableTypeName => "timespan";

        internal TomlTimeSpan(IMetaDataStore metaData, TimeSpan value)
            : base(metaData, value)
        {
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
