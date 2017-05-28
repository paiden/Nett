using System;
using System.Diagnostics;
using Nett.Extensions;

namespace Nett.Coma.Path
{
    internal sealed partial class TPath
    {
        private sealed class TableSegment : KeySegment
        {
            private readonly Type clrType;

            public TableSegment(string key, Type clrType)
                : base(TomlObjectType.Table, key)
            {
                this.clrType = clrType.CheckNotNull(nameof(clrType));
            }

            public override TomlObject Apply(TomlObject obj, PathSettings settings = PathSettings.None)
            {
                return base.TryApply(obj, settings)
                    ?? this.TryCreateTable(obj, settings)
                    ?? throw new InvalidOperationException(this.KeyNotFoundMessage);
            }

            public override TomlObject TryApply(TomlObject obj, PathSettings settings = PathSettings.None)
            {
                try
                {
                    return base.TryApply(obj, settings) ?? this.TryCreateTable(obj, settings);
                }
                catch
                {
                    Debug.Assert(false, "This should never happen if the stuff in the try block is implemented correctly.");
                    return null;
                }
            }

            public override string ToString() => $"/{{{this.key}}}";

            private TomlTable TryCreateTable(TomlObject obj, PathSettings settings)
            {
                if (settings.HasFlag(PathSettings.CreateTables) && obj is TomlTable tbl)
                {
                    var table = tbl.AddTable(this.key, tbl.CreateAttachedTable());
                    return table;
                }

                return null;
            }
        }
    }
}
