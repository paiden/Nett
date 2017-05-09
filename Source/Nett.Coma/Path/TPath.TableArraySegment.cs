using System;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class TableArraySegment : KeySegment
        {
            public TableArraySegment(string key)
                : base(TomlObjectType.ArrayOfTables, key)
            {
            }

            public override TomlObject Apply(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override TomlObject ApplyOrCreate(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(TomlObject value)
            {
                throw new NotImplementedException();
            }

            public override TomlObject TryApply(TomlObject obj)
            {
                throw new NotImplementedException();
            }

            public override string ToString() => $"/[{{{this.key}}}]";
        }
    }
}
