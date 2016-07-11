namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        internal TomlBool(IMetaDataStore metaData, bool value)
            : base(metaData, value)
        {
        }

        public override string ReadableTypeName => "bool";

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
