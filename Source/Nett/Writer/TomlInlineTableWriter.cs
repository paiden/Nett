namespace Nett.Writer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Util;

    internal sealed partial class TomlTableWriter
    {
        private sealed class TomlInlineTableWriter : TomlWriter
        {
            public TomlInlineTableWriter(FormattingStreamWriter writer, TomlConfig config)
                : base(writer, config)
            {
            }

            public void WriteInlineTable(string key, TomlTable table)
            {
                this.WritePrependComments(table);

                this.writer.Write(key);
                this.writer.Write(" = {");

                var rows = table.Rows.ToArray();

                for (int i = 0; i < rows.Length - 1; i++)
                {
                    this.WriteTableRow(rows[i]);
                    this.writer.Write(", ");
                }

                if (rows.Length > 0)
                {
                    this.WriteTableRow(rows[rows.Length - 1]);
                }

                this.writer.Write('}');
                this.WriteAppendComments(table);
            }

            private void WriteTableRow(KeyValuePair<string, TomlObject> r)
            {
                if (r.Value.TomlType == TomlObject.TomlObjectType.Array)
                {
                    this.WriteArray(r.Key, (TomlArray)r.Value);
                }
                else if (r.Value.TomlType == TomlObject.TomlObjectType.Table)
                {
                    var tbl = (TomlTable)r.Value;
                    if (tbl.TableType != TomlTable.TableTypes.Inline)
                    {
                        throw new InvalidOperationException($"Putting normal table '{r.Key}' inside inline table is not allowed.");
                    }

                    this.WriteInlineTable(r.Key, tbl);
                }
                else if (r.Value.TomlType == TomlObject.TomlObjectType.ArrayOfTables)
                {
                    throw new InvalidOperationException($"Putting Array of table '{r.Key}' inside inline table is not allowed.");
                }
                else
                {
                    this.WriteKeyedValue(r);
                }
            }
        }
    }
}
