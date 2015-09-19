using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Nett
{
    internal sealed class TomlStreamWriter : TomlObjectVisitor
    {
        private readonly StreamWriter sw;
        private readonly TomlConfig config;
        private readonly Stack<string> rowKeys = new Stack<string>();
        private bool writeValueKey = true;
        private bool writeTableKey = true;

        public TomlStreamWriter(StreamWriter streamWriter, TomlConfig config)
        {
            if (streamWriter == null) { throw new ArgumentNullException(nameof(StreamWriter)); }
            this.config = config ?? TomlConfig.DefaultInstance;

            this.sw = streamWriter;

            this.VisitBool = (b) => this.WriteKeyedValue(b, () => this.sw.Write(b.Value.ToString().ToLower()));
            this.VisitFloat = (f) => this.WriteKeyedValue(f, () => this.sw.Write(f.Value));
            this.VisitInt = (i) => this.WriteKeyedValue(i, () => this.sw.Write(i.Value));
            this.VisitDateTime = (dt) => this.WriteKeyedValue(dt, () => this.sw.Write(dt.Value));
            this.VisitTimespan = (ts) => this.WriteKeyedValue(ts, () => this.sw.Write(ts.Value));
            this.VisitString = (s) => this.WriteKeyedValue(s, () => this.sw.Write(string.Format("\"{0}\"", s.Value.Escape() ?? "")));

            this.VisitArray = (a) => this.WriteTomlArray(a);
            this.VisitTable = (t) => this.WriteTomlTable(t);
            this.VisitTableArray = (ta) => this.WriteTomlTableArray(ta);
        }

        private string CurrentRowKey { get { return this.rowKeys.Count > 0 ? this.rowKeys.Peek() : null; } }

        private void WriteKeyedValue(TomlObject obj, Action writeValue)
        {
            this.WritePrependComments(obj);
            if (writeValueKey)
            {
                Debug.Assert(this.CurrentRowKey != null);
                this.sw.Write($"{this.CurrentRowKey} = ");
            }

            writeValue();
            this.WriteApppendComments(obj);
        }

        private void WriteTomlTable(TomlTable table)
        {
            this.WritePrependComments(table);
            if (this.writeTableKey && !string.IsNullOrEmpty(this.CurrentRowKey))
            {
                sw.WriteLine();
                this.sw.WriteLine("[{0}]", this.CurrentRowKey);
                this.WriteApppendComments(table);
            }

            foreach (var r in table.Rows)
            {
                this.rowKeys.Push(r.Key);
                r.Value.Visit(this);
                this.sw.WriteLine();
                this.rowKeys.Pop();
            }
        }

        private void WriteTomlArray(TomlArray array)
        {
            using (new DisableWriteValueKeyContext(this))
            {
                Debug.Assert(this.CurrentRowKey != null);
                this.WritePrependComments(array);
                this.sw.Write($"{this.CurrentRowKey} = [");

                for (int i = 0; i < array.Items.Count - 1; i++)
                {
                    array.Items[i].Visit(this);
                    this.sw.Write(", ");
                }

                if (array.Items.Count > 0)
                {
                    array.Items[array.Items.Count - 1].Visit(this);
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
                    Debug.Assert(this.CurrentRowKey != null);
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
                this.sw.WriteLine($"#{FixMultilineComment(p.CommentText)}");
            }
        }

        private void WriteApppendComments(TomlObject obj)
        {
            var append = obj.Comments.Where((c) => this.config.GetCommentLocation(c) == TomlCommentLocation.Append);
            foreach (var a in append)
            {
                this.sw.Write($" #{FixMultilineComment(a.CommentText)}");
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

        private class DisableWriteTableKeyContext : IDisposable
        {
            private readonly TomlStreamWriter tomlWriter;
            public DisableWriteTableKeyContext(TomlStreamWriter tw)
            {
                Debug.Assert(tw != null);

                this.tomlWriter = tw;
                this.tomlWriter.writeTableKey = false;
            }
            public void Dispose()
            {
                this.tomlWriter.writeTableKey = true;
            }
        }
    }
}
