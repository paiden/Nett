namespace Nett.Coma
{
    using System;

    internal sealed class TomlSource : TomlObject
    {
        public TomlSource(IMetaDataStore metaData, IConfigSource source)
            : base(metaData)
        {
            this.Value = source;
        }

        public IConfigSource Value { get; }

        public override string ReadableTypeName => "Value source";

        public override TomlObjectType TomlType
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override object Get(Type t) => this.Value;

        public override void Visit(ITomlObjectVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
