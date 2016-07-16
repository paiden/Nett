namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        internal TomlInt(IMetaDataStore metaData, long value)
            : base(metaData, value)
        {
        }

        public override string ReadableTypeName => "int";

        public override TomlObjectType TomlType => TomlObjectType.Int;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
