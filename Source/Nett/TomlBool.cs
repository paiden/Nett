namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        public override string ReadableTypeName => "bool";

        public TomlBool(bool value)
            : base(value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
