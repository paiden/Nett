namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using Nett.Extensions;

    internal sealed class TomlSource : TomlObject
    {
        public TomlSource(ITomlRoot root, IConfigSource source)
            : base(root)
        {
            this.Value = source;
        }

        public IConfigSource Value { get; }

        public override string ReadableTypeName => "Value source";

        // Workaround for the 'internal TOML type' TomlSource as the type doesn't matter for this internal type
        // How can we provide a better value for this enum inside the COMA lib?
        public override TomlObjectType TomlType => TomlObjectType.Int;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            throw new NotImplementedException();
        }

        internal override object GetInternal(Type t, Func<IEnumerable<string>> _)
            => this.Value;

        internal override TomlObject CloneFor(ITomlRoot root) => CopyComments(new TomlSource(root, this.Value), this);
    }
}
