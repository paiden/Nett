namespace Nett.Writer
{
    using System.Collections.Generic;
    using Nett.Util;
    using static System.Diagnostics.Debug;

    internal sealed partial class TomlTableWriter : TomlWriter
    {
        public TomlTableWriter(FormattingStreamWriter writer, TomlConfig config)
            : base(writer, config)
        {
            Assert(writer != null);
            Assert(config != null);
        }

        internal void WriteToml(TomlTable table)
        {
            const string rootParentKey = "";
            this.WriteTableRows(rootParentKey, table);
            this.writer.Flush();
        }

        private static string CombineKey(string parent, string key) => parent + key + ".";

        private void WriteKeyedValueWithComments(KeyValuePair<string, TomlObject> row)
        {
            this.WritePrependComments(row.Value);
            this.WriteKeyedValue(row);
            this.WriteAppendComments(row.Value);
        }

        private void WriteNormalTomlTable(string parentKey, string key, TomlTable table)
        {
            this.WritePrependComments(table);
            this.writer.Write('[');
            this.writer.Write(parentKey + key);
            this.writer.Write(']');
            this.writer.WriteLine();
            this.WriteAppendComments(table);

            foreach (var r in table.Rows)
            {
                this.WriteTableRow(parentKey + key + ".", r);
            }
        }

        private void WriteTomlArrayWithComments(string key, TomlArray array)
        {
            this.WritePrependComments(array);
            this.WriteArray(key, array);
            this.WriteAppendComments(array);
        }

        private void WriteTableRow(string parentKey, KeyValuePair<string, TomlObject> r)
        {
            this.WritePrependNewlines(r.Value);

            if (r.Value.TomlType == TomlObject.TomlObjectType.Array) { this.WriteTomlArrayWithComments(r.Key, (TomlArray)r.Value); }
            else if (r.Value.TomlType == TomlObject.TomlObjectType.Table) { this.WriteTomlTable(parentKey, r.Key, (TomlTable)r.Value); }
            else if (r.Value.TomlType == TomlObject.TomlObjectType.ArrayOfTables)
            {
                this.WriteTomlTableArray(parentKey, r.Key, (TomlTableArray)r.Value);
            }
            else { this.WriteKeyedValueWithComments(r); }

            this.WriteAppendNewlines(r.Value);
        }

        private void WriteTableRows(string parentKey, TomlTable table)
        {
            Assert(table != null);

            foreach (var r in table.Rows)
            {
                this.WriteTableRow(parentKey, r);
            }
        }

        private void WriteTomlInlineTable(string parentKey, string key, TomlTable table)
        {
            var inlineWriter = new TomlInlineTableWriter(this.writer, this.config);
            inlineWriter.WriteInlineTable(key, table);
        }

        private void WriteTomlTable(string parentKey, string key, TomlTable table)
        {
            switch (table.TableType)
            {
                case TomlTable.TableTypes.Default: this.WriteNormalTomlTable(parentKey, key, table); break;
                case TomlTable.TableTypes.Inline: this.WriteTomlInlineTable(parentKey, key, table); break;
            }
        }

        private void WriteTomlTableArray(string parentKey, string key, TomlTableArray tableArray)
        {
            this.WritePrependComments(tableArray);

            foreach (var t in tableArray.Items)
            {
                this.writer.Write("[[");
                this.writer.Write(key);
                this.writer.Write("]]");
                this.writer.WriteLine();
                this.WriteAppendComments(tableArray);
                this.WriteTableRows(CombineKey(parentKey, key), t);
            }
        }
    }
}
