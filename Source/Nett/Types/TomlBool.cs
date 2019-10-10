using System;
using Nett.Extensions;

namespace Nett
{
    public sealed class TomlBool : TomlValue<bool>
    {
        internal TomlBool(ITomlRoot root, bool value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "bool";

        public override TomlObjectType TomlType => TomlObjectType.Bool;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
            => this.Value ? "true" : "false";

        internal TomlBool CloneBoolFor(ITomlRoot root) => CopyComments(new TomlBool(root, this.Value), this);

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneBoolFor(root);

        internal TomlBool BoolWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlBool(root, this.Value);
        }
    }
}
