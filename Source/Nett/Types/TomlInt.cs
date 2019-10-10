using System;
using Nett.Extensions;

namespace Nett
{
    public sealed class TomlInt : TomlValue<long>
    {
        internal TomlInt(ITomlRoot root, long value, IntTypes intType = IntTypes.Decimal)
            : base(root, value)
        {
            this.IntType = intType;
        }

        public enum IntTypes
        {
            Decimal,
            Binary,
            Octal,
            Hex,
        }

        public IntTypes IntType { get; }

        public override string ReadableTypeName => "int";

        public override TomlObjectType TomlType => TomlObjectType.Int;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            switch (this.IntType)
            {
                case IntTypes.Binary: return $"0b{Convert.ToString(this.Value, 2)}";
                case IntTypes.Hex: return $"0x{Convert.ToString(this.Value, 16)}";
                case IntTypes.Octal: return $"0o{Convert.ToString(this.Value, 8)}";
                default: return this.Value.ToString();
            }
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneIntFor(root);

        internal TomlInt CloneIntFor(ITomlRoot root) => CopyComments(new TomlInt(root, this.Value), this);

        internal TomlInt IntWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlInt(root, this.Value);
        }
    }
}
