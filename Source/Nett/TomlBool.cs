namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        internal TomlBool(ITomlRoot root, bool value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "bool";

        public override TomlObjectType TomlType => TomlObjectType.Bool;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
