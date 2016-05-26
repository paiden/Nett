using System.Diagnostics;

namespace Nett
{
    [DebuggerDisplay("{Value}")]
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

        internal TomlString(IMetaDataStore metaData, string value, TypeOfString type = TypeOfString.Default)
            : base(metaData, value)
        {
            this.type = type;
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
