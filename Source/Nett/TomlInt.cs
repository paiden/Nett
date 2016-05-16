namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        public override string ReadableTypeName => "int";

        internal TomlInt(long value)
            : base(value)
        {

        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
