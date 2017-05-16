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

            public override string ToString() => $"/[{{{this.key}}}]";
        }
    }
}
