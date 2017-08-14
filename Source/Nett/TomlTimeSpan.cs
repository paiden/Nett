namespace Nett
{
    using System;
    using Extensions;

    public sealed class TomlTimeSpan : TomlValue<TimeSpan>
    {
        internal TomlTimeSpan(ITomlRoot root, TimeSpan value)
            : base(root, value)
        {
        }

        public override string ReadableTypeName => "timespan";

        public override TomlObjectType TomlType => TomlObjectType.TimeSpan;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneTimespanFor(root);

        internal TomlTimeSpan CloneTimespanFor(ITomlRoot root) => CopyComments(new TomlTimeSpan(root, this.Value), this);

        internal override TomlValue ValueWithRoot(ITomlRoot root) => this.TimeSpanWithRoot(root);

        internal override TomlObject WithRoot(ITomlRoot root) => this.TimeSpanWithRoot(root);

        internal TomlTimeSpan TimeSpanWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            return new TomlTimeSpan(root, this.Value);
        }
    }
}
