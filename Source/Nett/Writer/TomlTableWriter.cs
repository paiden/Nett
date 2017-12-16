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
            this.WriteTableRows(rootParentKey, table, level: -1);
            this.writer.WriteLine();
            this.writer.Flush();
        }

        private static string CombineKey(string parent, TomlKey key) => parent + key.ToString() + ".";

        private static bool IsInlineTomlTableArray(TomlTableArray a)
            => a.Items.Any(t => t.TableType == TomlTable.TableTypes.Inline);

        private void WriteKeyedValueWithComments(KeyValuePair<TomlKey, TomlObject> row, int alignColumn, int level)
        {
            this.WritePrependComments(row.Value, level);
            this.WriteKeyedValue(row, alignColumn, level);
            this.WriteAppendComments(row.Value);
        }

        private void WriteNormalTomlTable(string parentKey, TomlKey key, TomlTable table, int level)
        {
            this.WriteTableSpacing();

            this.WritePrependComments(table, level);
            this.writer.Write(this.settings.GetIndentString(level));
            this.writer.Write('[');
            this.writer.Write(parentKey + key);
            this.writer.Write(']');
            this.writer.WriteLine();
            this.WriteAppendComments(table);

            int alignColumn = this.settings.GetKeyValueAlignColumn(table);
            int rowIndex = 0;
            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(CombineKey(parentKey, key), r, rowIndex++, alignColumn, level);
            }
        }

        private void WriteTableSpacing()
        {
            for (int i = 0; i < this.settings.TableSpacing; i++)
            {
                this.writer.WriteLine();
            }
        }

        private void WriteTomlArrayWithComments(TomlKey key, TomlArray array, int level)
        {
            this.WritePrependComments(array, level);
            this.WriteArray(key, array);
            this.WriteAppendComments(array);
        }

        private void WriteTableRow(string parentKey, KeyValuePair<TomlKey, TomlObject> r, int rowIndex, int alignColumn, int level)
        {
            if (rowIndex > 0)
            {
                this.writer.WriteLine();
            }

            if (r.Value.TomlType == TomlObjectType.Array)
            {
                this.WriteTomlArrayWithComments(r.Key, (TomlArray)r.Value, level);
            }
            else if (r.Value.TomlType == TomlObjectType.Table)
            {
                this.WriteTomlTable(parentKey, r.Key, (TomlTable)r.Value, level + 1);
            }
            else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
            {
                this.WriteTomlTableArray(parentKey, r.Key, (TomlTableArray)r.Value, level + 1);
            }
            else { this.WriteKeyedValueWithComments(r, alignColumn, level); }
        }

        private void WriteTableRows(string parentKey, TomlTable table, int level)
        {
            Assert(table != null);
            int alignColumn = this.settings.GetKeyValueAlignColumn(table);

            int rowIndex = 0;
            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(parentKey, r, rowIndex++, alignColumn, level);
            }
        }

        private void WriteTomlInlineTable(string parentKey, TomlKey key, TomlTable table, int level)
        {
            var inlineWriter = new TomlInlineTableWriter(this.writer, this.settings);
            inlineWriter.WriteInlineTable(key, table, level);
        }

        private void WriteTomlTable(string parentKey, TomlKey key, TomlTable table, int level)
        {
            switch (table.TableType)
            {
                case TomlTable.TableTypes.Default: this.WriteNormalTomlTable(parentKey, key, table, level); break;
                case TomlTable.TableTypes.Inline: this.WriteTomlInlineTable(parentKey, key, table, level); break;
            }
        }

        private void WriteTomlTableArray(string parentKey, TomlKey key, TomlTableArray tableArray, int level)
        {
            if (IsInlineTomlTableArray(tableArray))
            {
                var inlineWriter = new TomlInlineTableWriter(this.writer, this.settings);
                inlineWriter.WriteTomlTableArray(key, tableArray, level);
            }
            else
            {
                this.WriteTableSpacing();
                this.WritePrependComments(tableArray, level);

                for (int i = 0; i < tableArray.Items.Count; i++)
                {
                    if (i > 0) { this.writer.WriteLine(); }
                    this.writer.Write(this.settings.GetIndentString(level));
                    this.writer.Write("[[");
                    this.writer.Write(parentKey + key.ToString());
                    this.writer.Write("]]");
                    this.writer.WriteLine();
                    this.WriteAppendComments(tableArray);
                    this.WriteTableRows(CombineKey(parentKey, key), tableArray.Items[i], level);
                }
            }
        }
    }
}
