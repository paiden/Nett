namespace Nett
{
    public sealed class TomlString : TomlValue<string>
    {
        public override string ReadableTypeName => "string";

        public enum TypeOfString
        {
            Default,
            Normal,
            Literal,
            Multiline,
            MultilineLiteral,
        }

        private TypeOfString type = TypeOfString.Default;

        public TomlString(string value, TypeOfString type = TypeOfString.Default)
            : base(value)
        {
            this.type = type;
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
