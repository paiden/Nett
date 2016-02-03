using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Nett.Util;
using static System.Diagnostics.Debug;

namespace Nett
{
    internal sealed class TomlStreamWriter : TomlObjectVisitor
    {
        private readonly Stack<ParentKeyContext> parentKeyContexts = new Stack<ParentKeyContext>();
        private static readonly IDisposable NotAvailable = new DummyContext();

        private readonly TextWriter sw;
        private readonly TomlConfig config;
        private readonly Stack<string> rowKeys = new Stack<string>();
        private bool writeValueKey = true;
        private bool writeTableKey = true;
        private int writeInlineTableInvocationsRunning = 0;

        public TomlStreamWriter(FormattingStreamWriter writer, TomlConfig config)
        {
            Assert(writer != null);
            Assert(config != null);

            this.config = config;

            this.sw = writer;

            this.VisitBool = (b) => this.WriteKeyedValue(b, () => this.sw.Write(b.Value.ToString().ToLower()));
            this.VisitFloat = (f) => this.WriteKeyedValue(f, () => this.sw.Write(f.Value));
            this.VisitInt = (i) => this.WriteKeyedValue(i, () => this.sw.Write(i.Value));
            this.VisitDateTime = (dt) => this.WriteKeyedValue(dt, () => this.sw.Write(dt.ToString()));
            this.VisitTimespan = (ts) => this.WriteKeyedValue(ts, () => this.sw.Write(ts.Value));
            this.VisitString = (s) => this.WriteKeyedValue(s, () => this.sw.Write(string.Format("\"{0}\"", s.Value.Escape() ?? "")));

            this.VisitArray = (a) => this.WriteTomlArray(a);
            this.VisitTable = (t) => this.WriteTomlTable(t);
            this.VisitTableArray = (ta) => this.WriteTomlTableArray(ta);
        }

        internal void WriteToml(TomlTable table)
        {
            Assert(table != null);

            table.Visit(this);
            this.sw.Flush();
        }

        private string CurrentRowKey { get { return this.rowKeys.Count > 0 ? this.rowKeys.Peek() : null; } }

        private string GetKey(string current) => this.parentKeyContexts.Count > 0 ? this.parentKeyContexts.Peek().GetKey(current) : current;
        private IDisposable NewParentKeyContext() =>
            this.CurrentRowKey != null ? new ParentKeyContext(this.CurrentRowKey, this.parentKeyContexts) : NotAvailable;

        private void WriteKeyedValue(TomlObject obj, Action writeValue)
        {
            this.WritePrependComments(obj);
            if (writeValueKey)
            {
                Assert(this.CurrentRowKey != null);
                this.sw.Write($"{this.CurrentRowKey} = ");
            }

            writeValue();
            this.WriteApppendComments(obj);
        }

        private void WriteTomlTable(TomlTable table)
        {
            switch (table.TableType)
            {
                case TomlTable.TableTypes.Default: this.WriteNormalTomlTable(table); break;
                case TomlTable.TableTypes.Inline: this.WriteTomlInlineTable(table); break;
            }
        }

        private void WriteTomlInlineTable(TomlTable table)
        {
            this.writeInlineTableInvocationsRunning++;
            this.WritePrependComments(table);

            this.sw.Write(this.CurrentRowKey);
            this.sw.Write(" = {");

            var rows = table.Rows.ToArray();

            for (int i = 0; i < rows.Length - 1; i++)
            {
                this.WriteTableRow(rows[i]);
                this.sw.Write(", ");
            }

            if (rows.Length > 0)
            {
                this.WriteTableRow(rows[rows.Length - 1]);
            }

            this.sw.Write("}");
            this.WriteApppendComments(table);
            this.writeInlineTableInvocationsRunning--;
        }

        private void WriteNormalTomlTable(TomlTable table)
        {
            if (this.writeInlineTableInvocationsRunning > 0)
            {
                throw new InvalidOperationException("Cannot write normal table inside inline table.");
            }

            this.WritePrependComments(table);
            if (this.writeTableKey && !string.IsNullOrEmpty(this.CurrentRowKey))
            {
                sw.WriteLine();
                this.sw.WriteLine("[{0}]", this.GetKey(this.CurrentRowKey));
                this.WriteApppendComments(table);
            }

            using (this.NewParentKeyContext())
            using (new EnableWriteTableKeyContext(this))
            {
                foreach (var r in table.Rows)
                {
                    this.WriteTableRow(r);
                    this.sw.WriteLine();
                    this.rowKeys.Pop();
                }
            }
        }

        private void WriteTableRow(KeyValuePair<string, TomlObject> row)
        {
            this.rowKeys.Push(row.Key);
            row.Value.Visit(this);
        }

        private void WriteTomlArray(TomlArray array)
        {
            using (new DisableWriteValueKeyContext(this))
            {
                Assert(this.CurrentRowKey != null);
                this.WritePrependComments(array);
                this.sw.Write($"{this.CurrentRowKey} = [");

                for (int i = 0; i < array.Items.Length - 1; i++)
                {
                    array.Items[i].Visit(this);
                    this.sw.Write(", ");
                }

                if (array.Items.Length > 0)
                {
                    array.Items[array.Items.Length - 1].Visit(this);
                }

                this.sw.Write("]");
                this.WriteApppendComments(array);
            }
        }

        private void WriteTomlTableArray(TomlTableArray tableArray)
        {
            this.WritePrependComments(tableArray);

            using (new DisableWriteTableKeyContext(this))
            {
                foreach (var t in tableArray.Items)
                {
                    Assert(this.CurrentRowKey != null);
                    this.sw.WriteLine($"[[{this.CurrentRowKey}]]");
                    this.WriteApppendComments(tableArray);
                    t.Visit(this);
                    this.sw.WriteLine();
                }
            }
        }

        private void WritePrependComments(TomlObject obj)
        {
            var prepend = obj.Comments.Where((c) => this.config.GetCommentLocation(c) == TomlCommentLocation.Prepend);
            foreach (var p in prepend)
            {
                this.sw.WriteLine($"#{FixMultilineComment(p.Text)}");
            }
        }

        private void WriteApppendComments(TomlObject obj)
        {
            var append = obj.Comments.Where((c) => this.config.GetCommentLocation(c) == TomlCommentLocation.Append);
            foreach (var a in append)
            {
                this.sw.Write($" #{FixMultilineComment(a.Text)}");
            }
        }

        private static string FixMultilineComment(string src)
        {
            return src.Replace("\n", "\n#");
        }

        private class DisableWriteValueKeyContext : IDisposable
        {
            private readonly TomlStreamWriter tomlWriter;
            public DisableWriteValueKeyContext(TomlStreamWriter tw)
            {
                Debug.Assert(tw != null);

                this.tomlWriter = tw;
                this.tomlWriter.writeValueKey = false;
            }
            public void Dispose()
            {
                this.tomlWriter.writeValueKey = true;
            }
        }

        private abstract class SetWriteTableKeyContext : IDisposable
        {
            private readonly TomlStreamWriter tomlWriter;
            private readonly bool restoreOnExit;

            public SetWriteTableKeyContext(TomlStreamWriter tw, bool contextState)
            {
                Assert(tw != null);

                this.restoreOnExit = tw.writeTableKey;
                this.tomlWriter = tw;
                this.tomlWriter.writeTableKey = contextState;
            }

            public void Dispose()
            {
                this.tomlWriter.writeTableKey = this.restoreOnExit;
            }
        }

        /// <summary>
        /// Helper to provide keys for nested elements.
        /// </summary>
        /// <remarks>
        /// Only tables can have nested elements with a key. So only tables are allowed to create a
        /// new parent key context. No other element should establish a new key context.
        /// </remarks>
        [DebuggerDisplay("[{parentKeyChain}]")]
        private class ParentKeyContext : IDisposable
        {
            private readonly Stack<ParentKeyContext> parentKeyContexts;
            private readonly string parentKeyChain;

            public ParentKeyContext(string key, Stack<ParentKeyContext> parentKeyContexts)
            {
                Assert(parentKeyContexts != null);
                Assert(!string.IsNullOrWhiteSpace(key));

                this.parentKeyChain = parentKeyContexts.Count <= 0 ? key : parentKeyContexts.Peek().parentKeyChain + $".{key}";
                this.parentKeyContexts = parentKeyContexts;
                this.parentKeyContexts.Push(this);
            }

            public string GetKey(string current) => this.parentKeyChain + $".{current}";

            public void Dispose()
            {
                Assert(this.parentKeyContexts.Peek() == this);
                this.parentKeyContexts.Pop();
            }
        }

        /// <summary>
        /// Needed in table arrays as in such cases the table array writes the key for it's child elements
        /// </summary>
        /// <remarks>
        /// Not quite happy with the context solution but don't see much better solution for now as the table
        /// itself simply doesn't know if it gets written inside a table array or not. So the array has to pass
        /// some information via this context to the next write table invocation (if there is any).
        /// </remarks>
        private sealed class DisableWriteTableKeyContext : SetWriteTableKeyContext
        {
            public DisableWriteTableKeyContext(TomlStreamWriter sw)
                : base(sw, contextState: false)
            {
            }
        }

        /// <summary>
        /// Used inside tables after write key to re-enable key writing for sub tables
        /// </summary>
        /// <remarks>
        /// Only affect serialization if the current table gets serialized inside a table
        /// array.
        /// </remarks>
        private sealed class EnableWriteTableKeyContext : SetWriteTableKeyContext
        {
            public EnableWriteTableKeyContext(TomlStreamWriter sw)
                : base(sw, contextState: true)
            {
            }
        }

        private sealed class DummyContext : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
