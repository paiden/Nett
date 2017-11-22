using System;
using System.Diagnostics;
using Nett.Extensions;

namespace Nett
{
    [Flags]
    public enum TomlStringType
    {
        Auto = 0x0,
        Basic = 0x01 << 0,
        Literal = 0x01 << 1,
        Multiline = 0x01 << 2,
        MultilineLiteral = Literal | Multiline,
    }

    [DebuggerDisplay("{Value}")]
    public sealed class TomlString : TomlValue<string>
    {
        private readonly TomlStringType type = Nett.TomlStringType.Auto;

        internal TomlString(ITomlRoot root, string value)
            : this(root, value, root.Settings.DefaultStringType)
        {
        }

        internal TomlString(ITomlRoot root, string value, TomlStringType type)
            : base(root, value)
        {
            this.type = type == Nett.TomlStringType.Auto ? ChooseBestStringType(value) : type;

            if (type == Nett.TomlStringType.Literal && !IsLiteralStringTypeValid(value))
            {
                this.type = Nett.TomlStringType.MultilineLiteral;
            }
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
                case Nett.TomlStringType.Basic: return $"\"{val}\"";
                case Nett.TomlStringType.Multiline: return $"\"\"\"{val}\"\"\"";
                case Nett.TomlStringType.Literal: return $"'{val}'";
                case Nett.TomlStringType.MultilineLiteral: return $"'''{val}'''";
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

        private static TomlStringType ChooseBestStringType(string s)
        {
            bool foundBackslash = false;
            bool foundNewline = false;

            for (int i = 0; i < s.Length; i++)
            {
                foundBackslash |= s[i] == '\\';
                foundNewline |= s[i] == '\n';
            }

            if (foundBackslash && foundNewline) { return Nett.TomlStringType.MultilineLiteral; }
            else if (foundBackslash) { return Nett.TomlStringType.Literal; }
            else if (foundNewline) { return Nett.TomlStringType.Multiline; }
            else { return Nett.TomlStringType.Basic; }
        }
    }
}
