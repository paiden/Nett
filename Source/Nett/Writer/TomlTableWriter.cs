namespace Nett.Writer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Nett.Util;
    using static System.Diagnostics.Debug;

    internal sealed partial class TomlTableWriter : TomlWriter
    {
        private static readonly TomlKey KeyNotAvailable = new TomlKey("_");

        public TomlTableWriter(FormattingStreamWriter writer, TomlSettings settings)
            : base(writer, settings)
        {
            Assert(writer != null);
            Assert(settings != null);
        }

        internal static string WriteTomlFragment(TomlTable table)
        {
            return table.TableType == TomlTable.TableTypes.Inline
                ? WriteIntoStream(WriteInlineTable)
                : WriteIntoStream(WriteTable);

            void WriteInlineTable(FormattingStreamWriter stream)
            {
                var writer = new TomlInlineTableWriter(stream, table.Root.Settings);
                writer.WriteInlineTableBody(table);
            }

            void WriteTable(FormattingStreamWriter stream)
            {
                var writer = new TomlTableWriter(stream, table.Root.Settings);
                writer.WriteToml(table);
            }
        }

        internal static string WriteTomlFragment(TomlArray array)
        {
            return WriteIntoStream(WriteArray);

            void WriteArray(FormattingStreamWriter stream)
            {
                var writer = new TomlTableWriter(stream, array.Root.Settings);
                writer.WriteArrayPart(array);
            }
        }

        internal static string WriteTomlFragment(TomlTableArray array)
        {
            return WriteIntoStream(WriteTomlTableArray);

            void WriteTomlTableArray(FormattingStreamWriter stream)
            {
                var writer = new TomlTableWriter(stream, array.Root.Settings);
                writer.WriteTomlTableArray(string.Empty, KeyNotAvailable, array);
            }
        }

        internal void WriteToml(TomlTable table)
        {
            const string rootParentKey = "";
            this.WriteTableRows(rootParentKey, table);
            this.writer.Flush();
        }

        private static string WriteIntoStream(Action<FormattingStreamWriter> write)
        {
            using (var ms = new MemoryStream(1024))
            {
                var sw = new FormattingStreamWriter(ms, CultureInfo.InvariantCulture);
                write(sw);
                sw.Flush();
                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        private static string CombineKey(string parent, TomlKey key) => parent + key.ToString() + ".";

        private static bool IsInlineTomlTableArray(TomlTableArray a)
            => a.Items.Any(t => t.TableType == TomlTable.TableTypes.Inline);

        private void WriteKeyedValueWithComments(KeyValuePair<TomlKey, TomlObject> row)
        {
            this.WritePrependComments(row.Value);
            this.WriteKeyedValue(row);
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

            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(CombineKey(parentKey, key), r);
            }
        }

        private void WriteTomlArrayWithComments(TomlKey key, TomlArray array)
        {
            this.WritePrependComments(array);
            this.WriteArray(key, array);
            this.WriteAppendComments(array);
        }

        private void WriteTableRow(string parentKey, KeyValuePair<TomlKey, TomlObject> r)
        {
            this.WritePrependNewlines(r.Value);

            if (r.Value.TomlType == TomlObjectType.Array) { this.WriteTomlArrayWithComments(r.Key, (TomlArray)r.Value); }
            else if (r.Value.TomlType == TomlObjectType.Table) { this.WriteTomlTable(parentKey, r.Key, (TomlTable)r.Value); }
            else if (r.Value.TomlType == TomlObjectType.ArrayOfTables)
            {
                this.WriteTomlTableArray(parentKey, r.Key, (TomlTableArray)r.Value);
            }
            else { this.WriteKeyedValueWithComments(r); }

            this.WriteAppendNewlines(r.Value);
        }

        private void WriteTableRows(string parentKey, TomlTable table)
        {
            Assert(table != null);

            foreach (var r in table.InternalRows)
            {
                this.WriteTableRow(parentKey, r);
            }
        }

        private void WriteTomlInlineTable(string parentKey, TomlKey key, TomlTable table)
        {
            var inlineWriter = new TomlInlineTableWriter(this.writer, this.settings);
            inlineWriter.WriteInlineTable(key, table);
        }

        private void WriteTomlDottedTable(TomlKey key, TomlTable table)
        {
            var dottedWriter = new DottedTableWriter(this.writer, this.settings);
            dottedWriter.WriteDottedTable(table, key);
        }

        private void WriteTomlTable(string parentKey, TomlKey key, TomlTable table)
        {
            switch (table.TableType)
            {
                case TomlTable.TableTypes.Inline: this.WriteTomlInlineTable(parentKey, key, table); break;
                case TomlTable.TableTypes.Dotted: this.WriteTomlDottedTable(key, table); break;
                default: this.WriteNormalTomlTable(parentKey, key, table); break;
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
