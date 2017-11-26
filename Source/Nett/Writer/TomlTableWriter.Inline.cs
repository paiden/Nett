namespace Nett.Writer
{
    using System.Collections.Generic;
    using System.Linq;
    using Util;

    internal sealed partial class TomlTableWriter
    {
        private sealed class TomlInlineTableWriter : TomlWriter
        {
            public TomlInlineTableWriter(FormattingStreamWriter writer, TomlSettings settings)
                : base(writer, settings)
            {
            }

            public void WriteInlineTable(TomlKey key, TomlTable table)
            {
                this.WritePrependComments(table);

                this.writer.Write(key.ToString());
                this.writer.Write(" = ");
                this.WriteInlineTableBody(table);

                this.WriteAppendComments(table);
            }

            public void WriteTomlTableArray(TomlKey key, TomlTableArray tableArray)
            {
                this.WritePrependComments(tableArray);

                const string assignment = " = [ ";
                this.writer.Write(key.ToString());
                this.writer.Write(assignment);

                int indentLen = key.ToString().Length + assignment.Length;
                string indent = new string(' ', indentLen);

                for (int i = 0; i < tableArray.Items.Count; i++)
                {
                    this.WriteInlineTableBody(tableArray.Items[i]);

                    if (i < tableArray.Items.Count - 1)
                    {
                        this.writer.Write(",");
                        this.writer.WriteLine();
                        this.writer.Write(indent);
                    }
                }

                this.writer.WriteLine(" ]");
            }

            private void WriteInlineTableBody(TomlTable table)
            {
                this.writer.Write("{ ");
                var rows = table.InternalRows.ToArray();

                for (int i = 0; i < rows.Length - 1; i++)
                {
                    this.WriteTableRow(rows[i]);
                    this.writer.Write(", ");
                }

                if (rows.Length > 0)
                {
                    this.WriteTableRow(rows[rows.Length - 1]);
                }

                this.writer.Write(" }");
            }

            private void WriteTableRow(KeyValuePair<TomlKey, TomlObject> r)
            {
                if (r.Value.TomlType == TomlObjectType.Array)
                {
                    this.WriteArray(r.Key, (TomlArray)r.Value);
                }
                else if (r.Value.TomlType == TomlObjectType.Table)
                {
                    this.WriteInlineTable(r.Key, (TomlTable)r.Value);
                }
                else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
                {
                    this.WriteTomlTableArray(r.Key, (TomlTableArray)r.Value);
                }
                else
                {
                    this.WriteKeyedValue(r, alignColumn: 0);
                }
            }
        }
    }
}
