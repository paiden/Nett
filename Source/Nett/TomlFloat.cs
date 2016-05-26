namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        public override string ReadableTypeName => "float";

        internal TomlFloat(IMetaDataStore metaData, double value)
            : base(metaData, value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
