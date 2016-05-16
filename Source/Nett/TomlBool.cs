namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        public override string ReadableTypeName => "bool";

        internal TomlBool(bool value)
            : base(value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

