using Nett.Extensions;

namespace Nett
{
    public sealed class TomlFloat : TomlValue<double>
    {
        internal TomlFloat(ITomlRoot root, double value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "float";

        public override TomlObjectType TomlType => TomlObjectType.Float;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneFloatFor(root);

        internal TomlFloat CloneFloatFor(ITomlRoot root) => CopyComments(new TomlFloat(root, this.Value), this);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.FloatWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.FloatWithRoot(root);

        internal TomlFloat FloatWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlFloat(root, this.Value);
        }
    }
}
