namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        public override string ReadableTypeName => "bool";

        internal TomlBool(IMetaDataStore metaData, bool value)
            : base(metaData, value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

