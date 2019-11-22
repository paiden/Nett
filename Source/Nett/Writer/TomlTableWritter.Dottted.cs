using System.Linq;
using Nett.Extensions;
using Nett.Util;

namespace Nett.Writer
{
    internal sealed partial class TomlTableWriter
    {
#pragma warning disable S1144 // Unused private types or members should be removed
        private class DottedTableWriter : TomlInlineTableWriter
        {
            public DottedTableWriter(FormattingStreamWriter writer, TomlSettings settings)
                : base(writer, settings)
            {
            }

            public void WriteDottedTable(TomlTable table, params TomlKey[] keys)
            {
                foreach (var r in table.NonScopingRows)
                {
                    // Inline Tables
                    if (r.Value is TomlTable it)
                    {
                        this.WriteDotteKey(ConcatWithRowKey(r.Key));
                        this.WriteInlineTableBody(it);
                    }
                    else
                    {
                        this.WriteDottedKeyValue(r.Value, ConcatWithRowKey(r.Key));
                    }
                }

                foreach (var r in table.ScopingRows)
                {
                    if (r.Value is TomlTable tbl)
                    {
                        this.WriteDottedTable(tbl, ConcatWithRowKey(r.Key));
                    }
                }

                TomlKey[] ConcatWithRowKey(TomlKey rk)
                {
                    return keys.Concat(rk.ToEnumerable()).ToArray();
                }
            }

            public void WriteDottedKeyValue(TomlObject value, params TomlKey[] keySegments)
            {
                this.WriteDotteKey(keySegments);
                this.WriteValue(value);
                this.WriteAppendNewlines(value);
            }

            private void WriteDotteKey(params TomlKey[] keySegments)
            {
                for (int i = 0; i < keySegments.Length - 1; i++)
                {
                    this.writer.Write(keySegments[i].Value);
                    this.writer.Write(".");
                }

                this.writer.Write(keySegments[keySegments.Length - 1].Value);

                this.writer.Write(" = ");
            }
        }
#pragma warning restore S1144 // Unused private types or members should be removed
    }
}
