namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        public override string ReadableTypeName => "int";

        public TomlInt(long value)
            : base(value)
        {

        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
