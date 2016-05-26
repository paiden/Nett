namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        public override string ReadableTypeName => "int";

        internal TomlInt(IMetaDataStore metaData, long value)
            : base(metaData, value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
