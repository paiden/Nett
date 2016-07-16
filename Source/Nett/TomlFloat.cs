namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        internal TomlFloat(IMetaDataStore metaData, double value)
            : base(metaData, value)
        {
        }

        public override string ReadableTypeName => "float";

        public override TomlObjectType TomlType => TomlObjectType.Float;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
