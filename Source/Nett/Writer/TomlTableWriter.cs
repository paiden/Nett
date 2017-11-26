using System.Collections.Generic;
using System.Linq;
using Nett.Util;
using static System.Diagnostics.Debug;

namespace Nett.Writer
{
    internal sealed partial class TomlTableWriter : TomlWriter
    {
        public TomlTableWriter(FormattingStreamWriter writer, TomlSettings settings)
            : base(writer, settings)
        {
            Assert(writer != null);
            Assert(settings != null);
        }

        internal void WriteToml(TomlTable table)
        {
            const string rootParentKey = "";
            this.WriteTableRows(rootParentKey, table);
            this.writer.Flush();
        }

        private static string CombineKey(string parent, TomlKey key) => parent + key.ToString() + ".";

        private static bool IsInlineTomlTableArray(TomlTableArray a)
            => a.Items.Any(t => t.TableType == TomlTable.TableTypes.Inline);

        private void WriteKeyedValueWithComments(KeyValuePair<TomlKey, TomlObject> row, int alignColumn)
        {
            this.WritePrependComments(row.Value);
            this.WriteKeyedValue(row, alignColumn);
            this.WriteAppendComments(row.Value);
        }

        private void WriteNormalTomlTable(string parentKey, TomlKey key, TomlTable table)
        {
            this.WritePrependComments(table);
            this.writer.Write('[');
            this.writer.Write(parentKey + key);
            this.writer.Write(']');
            this.writer.WriteLine();
            this.WriteAppendComments(table);

            int alignColumn = this.settings.GetKeyValueAlignColumn(table);
            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(CombineKey(parentKey, key), r, alignColumn);
            }
        }

        private void WriteTomlArrayWithComments(TomlKey key, TomlArray array)
        {
            this.WritePrependComments(array);
            this.WriteArray(key, array);
            this.WriteAppendComments(array);
        }

        private void WriteTableRow(string parentKey, KeyValuePair<TomlKey, TomlObject> r, int alignColumn)
        {
            this.WritePrependNewlines(r.Value);

            if (r.Value.TomlType == TomlObjectType.Array) { this.WriteTomlArrayWithComments(r.Key, (TomlArray)r.Value); }
            else if (r.Value.TomlType == TomlObjectType.Table) { this.WriteTomlTable(parentKey, r.Key, (TomlTable)r.Value); }
            else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
            {
                this.WriteTomlTableArray(parentKey, r.Key, (TomlTableArray)r.Value);
            }
            else { this.WriteKeyedValueWithComments(r, alignColumn); }

            this.WriteAppendNewlines(r.Value);
        }

        private void WriteTableRows(string parentKey, TomlTable table)
        {
            Assert(table != null);
            int alignColumn = this.settings.GetKeyValueAlignColumn(table);

            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(parentKey, r, alignColumn);
            }
        }

        private void WriteTomlInlineTable(string parentKey, TomlKey key, TomlTable table)
        {
            var inlineWriter = new TomlInlineTableWriter(this.writer, this.settings);
            inlineWriter.WriteInlineTable(key, table);
        }

        private void WriteTomlTable(string parentKey, TomlKey key, TomlTable table)
        {
            switch (table.TableType)
            {
                case TomlTable.TableTypes.Default: this.WriteNormalTomlTable(parentKey, key, table); break;
                case TomlTable.TableTypes.Inline: this.WriteTomlInlineTable(parentKey, key, table); break;
            }
        }

        private void WriteTomlTableArray(string parentKey, TomlKey key, TomlTableArray tableArray)
        {
            if (IsInlineTomlTableArray(tableArray))
            {
                var inlineWriter = new TomlInlineTableWriter(this.writer, this.settings);
                inlineWriter.WriteTomlTableArray(key, tableArray);
            }
            else
            {
                this.WritePrependComments(tableArray);

                foreach (var t in tableArray.Items)
                {
                    this.writer.Write("[[");
                    this.writer.Write(parentKey + key.ToString());
                    this.writer.Write("]]");
                    this.writer.WriteLine();
                    this.WriteAppendComments(tableArray);
                    this.WriteTableRows(CombineKey(parentKey, key), t);
                }
            }
        }
    }
}
