namespace Nett
{
    using System.Diagnostics;
    using Extensions;
    using Nett.Parser;

    [DebuggerDisplay("{Value}")]
    public sealed class TomlString : TomlValue<string>
    {
        private static readonly char[] RT = { '\r' };
        private static readonly char[] NT = { '\n' };

        private readonly TypeOfString type;

        internal TomlString(ITomlRoot root, string value, TypeOfString type = TypeOfString.Default)
            : base(root, MultilineTrim(type, value))
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

        public override string ToString()
            => this.Value;

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneStringFor(root);

        internal TomlString CloneStringFor(ITomlRoot root) => CopyComments(new TomlString(root, this.Value), this);

        private static string MultilineTrim(TypeOfString type, string input)
        {
            switch (type)
            {
                case TypeOfString.MultilineLiteral: return TrimFirstNewline(input);
                case TypeOfString.Multiline: return ReplaceDelimeterBackslash(TrimFirstNewline(input));
                default: return input;
            }

            string TrimFirstNewline(string i)
                => i.TrimStart(RT).TrimStart(NT);
        }

        private static string ReplaceDelimeterBackslash(string source)
        {
            int candiate = -1;

            for (int i = 0; i < source.Length; i++)
            {
                if (!IsCandiate() && source[i] == '\\')
                {
                    candiate = i;
                }
                else if (IsCandiate() && source[i].Is('\r', '\n'))
                {
                    source = source.Remove(candiate, 1);
                    source = source.TrimStartFrom(candiate);
                    i = candiate - 1; // -1 to counter for loop increment before next char check
                    candiate = -1;
                }
                else if (IsCandiate() && !source[i].IsWhitespaceChar())
                {
                    candiate = -1;
                }
            }

            return source;

            bool IsCandiate()
                => candiate >= 0;
        }
    }
}
