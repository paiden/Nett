namespace Nett
{
    using System.Diagnostics;

    [DebuggerDisplay("{Value}")]
    public sealed class TomlString : TomlValue<string>
    {
        private TypeOfString type = TypeOfString.Default;

        internal TomlString(IMetaDataStore metaData, string value, TypeOfString type = TypeOfString.Default)
            : base(metaData, value)
        {
            this.type = type;
        }

        public enum TypeOfString
        {
            Default,
            Normal,
            Literal,
            Multiline,
            MultilineLiteral,
        }

        public override string ReadableTypeName => "string";

        public override TomlObjectType TomlType => TomlObjectType.String;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
