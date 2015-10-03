namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        public override string ReadableTypeName => "float";

        public TomlFloat(double value)
            : base(value)
        {

        }

        public override void Visit(TomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
