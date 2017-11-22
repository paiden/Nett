using System;

namespace Nett
{
    internal struct TomlKey : IEquatable<TomlKey>
    {
        public readonly string Value;
        public KeyType Type;

        public TomlKey(string s)
        {
            this.Type = AutoClassify(s);
            this.Value = s;
        }

        public TomlKey(string key, KeyType type)
        {
            this.Value = key;
            this.Type = type;
        }

        public enum KeyType
        {
            Undefined,
            Bare,
            Basic,
            Literal,
        }

        public static bool operator ==(TomlKey x, TomlKey y) => x.Value == y.Value;

        public static bool operator !=(TomlKey x, TomlKey y) => !(x == y);

        public bool Equals(TomlKey other) => this == other;

        public override bool Equals(object obj) => obj is TomlKey && this == (TomlKey)obj;

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString()
        {
            var type = this.Type == KeyType.Undefined
                ? AutoClassify(this.Value)
                : this.Type;

            switch (type)
            {
                case KeyType.Bare: return this.Value;
                case KeyType.Basic: return "\"" + this.Value.Escape(TomlStringType.Basic) + "\"";
                case KeyType.Literal: return "'" + this.Value + "'";
                default: return this.Value;
            }
        }

        private static KeyType AutoClassify(string input)
        {
            if (input.HasBareKeyCharsOnly()) { return KeyType.Bare; }
            else if (input.IndexOf("'") >= 0) { return KeyType.Basic; }
            else { return KeyType.Literal; }
        }
    }
}
