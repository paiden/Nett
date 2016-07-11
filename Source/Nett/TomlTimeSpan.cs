namespace Nett
{
    using System;

    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        internal TomlTimeSpan(IMetaDataStore metaData, TimeSpan value)
            : base(metaData, value)
        {
        }

        public override string ReadableTypeName => "timespan";

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
