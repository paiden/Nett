namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        internal TomlInt(ITomlRoot root, long value)
            : base(root, value)
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
