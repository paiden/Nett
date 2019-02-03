using System;
using Nett.Parser;

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

        public static bool operator ==(TomlKey x, TomlKey y)
            => x.Equals(y);

        public static bool operator !=(TomlKey x, TomlKey y)
            => !x.Equals(y);

        public bool Equals(TomlKey other)
            => this.Value == other.Value;

        public override bool Equals(object obj)
            => obj is TomlKey k && this.Equals(k);

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString()
        {
            KeyType type = this.Type == KeyType.Undefined
                ? AutoClassify(this.Value)
                : this.Type;

            switch (type)
            {
                case KeyType.Bare: return this.Value;
                case KeyType.Basic: return "\"" + this.Value.Escape() + "\"";
                case KeyType.Literal: return "'" + this.Value + "'";
                default: return this.Value;
            }
        }

        internal static TomlKey FromToken(Token tkn)
        {
            switch (tkn.Type)
            {
                case TokenType.BareKey: return new TomlKey(tkn.Value, KeyType.Bare);
                case TokenType.SingleQuotedKey: return new TomlKey(tkn.Value, KeyType.Literal);
                case TokenType.DoubleQuotedKey: return new TomlKey(tkn.Value, KeyType.Basic);
                default: throw new InvalidOperationException($"Cannot create Key from token '{tkn}'.");
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
