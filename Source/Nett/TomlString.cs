namespace Nett
{
    using System;
    using System.Diagnostics;
    using Extensions;

    [DebuggerDisplay("{Value}")]
    public sealed class TomlString : TomlValue<string>
    {
        private readonly TypeOfString type = TypeOfString.Auto;

        internal TomlString(ITomlRoot root, string value)
            : this(root, value, root.Settings.DefaultStringType)
        {
        }

        internal TomlString(ITomlRoot root, string value, TypeOfString type)
            : base(root, value)
        {
            this.type = type == TypeOfString.Auto ? ChooseBestStringType(value) : type;

            if (type == TypeOfString.Literal && !IsLiteralStringTypeValid(value))
            {
                this.type = TypeOfString.MultilineLiteral;
            }
        }

        [Flags]
        public enum TypeOfString
        {
            Auto = 0x0,
            Normal = 0x01 << 0,
            Literal = 0x01 << 1,
            Multiline = 0x01 << 2,
            MultilineLiteral = Literal | Multiline,
        }

        public override string ReadableTypeName => "string";

        public override TomlObjectType TomlType => TomlObjectType.String;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal string QuotedAndEscapedValue()
        {
            string val = this.Value.Escape(this.type);

            switch (this.type)
            {
                case TypeOfString.Normal: return $"\"{val}\"";
                case TypeOfString.Multiline: return $"\"\"\"{val}\"\"\"";
                case TypeOfString.Literal: return $"'{val}'";
                case TypeOfString.MultilineLiteral: return $"'''{val}'''";
            }

            return $"\"{val}\"";
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneStringFor(root);

        internal TomlString CloneStringFor(ITomlRoot root) => CopyComments(new TomlString(root, this.Value), this);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.StringWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.StringWithRoot(root);

        internal TomlString StringWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlString(root, this.Value, this.type);
        }

        private static bool IsLiteralStringTypeValid(string s) => s.IndexOf('\'') < 0;

        private static bool IsMultilineLiteralStringTypeValid(string s) => s.Length <= 0 || s[s.Length - 1] != '\'';

        private static TypeOfString ChooseBestStringType(string s)
        {
            bool foundBackslash = false;
            bool foundNewline = false;

            for (int i = 0; i < s.Length; i++)
            {
                foundBackslash |= s[i] == '\\';
                foundNewline |= s[i] == '\n';
            }

            if (foundBackslash && foundNewline) { return TypeOfString.MultilineLiteral; }
            else if (foundBackslash) { return TypeOfString.Literal; }
            else if (foundNewline) { return TypeOfString.Multiline; }
            else { return TypeOfString.Normal; }
        }
    }
}
