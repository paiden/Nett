namespace Nett.Writer
{
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Util;

    using static System.Diagnostics.Debug;

    internal abstract class TomlWriter
    {
        protected readonly TomlConfig config;
        protected readonly FormattingStreamWriter writer;

        public TomlWriter(FormattingStreamWriter writer, TomlConfig config)
        {
            this.writer = writer;
            this.config = config;
        }

        protected void WriteAppendComments(TomlObject obj)
        {
            var append = obj.Comments.Where((c) => this.config.GetCommentLocation(c) == TomlCommentLocation.Append);
            foreach (var a in append)
            {
                this.writer.Write(" #");
                this.writer.Write(FixMultilineComment(a.Text));
            }
        }

        protected void WriteAppendNewlines(TomlObject obj)
        {
            if (this.ShouldAppendWithNewline(obj))
            {
                this.writer.WriteLine();
            }
        }

        protected void WriteArray(string key, TomlArray array)
        {
            this.writer.Write(key);
            this.writer.Write(" = [");

            for (int i = 0; i < array.Items.Length - 1; i++)
            {
                this.WriteValue(array[i]);
                this.writer.Write(", ");
            }

            if (array.Items.Length > 0)
            {
                this.WriteValue(array.Items[array.Items.Length - 1]);
            }

            this.writer.Write(']');
        }

        protected void WriteKeyedValue(KeyValuePair<string, TomlObject> kvp)
        {
            this.writer.Write(kvp.Key);
            this.writer.Write(" = ");
            this.WriteValue(kvp.Value);
        }

        protected void WritePrependComments(TomlObject obj)
        {
            var prepend = obj.Comments.Where((c) => this.config.GetCommentLocation(c) == TomlCommentLocation.Prepend);
            foreach (var p in prepend)
            {
                this.writer.Write('#');
                this.writer.Write(FixMultilineComment(p.Text));
                this.writer.WriteLine();
            }
        }

        protected void WritePrependNewlines(TomlObject obj)
        {
            if (this.ShouldPrependWithNewline(obj))
            {
                this.writer.WriteLine();
            }
        }

        private static string FixMultilineComment(string src) => src.Replace("\n", "\n#");

        private bool ShouldAppendWithNewline(TomlObject obj) => !this.ShouldPrependWithNewline(obj);

        private bool ShouldPrependWithNewline(TomlObject obj) =>
            (obj.TomlType == TomlObject.TomlObjectType.Table && ((TomlTable)obj).TableType != TomlTable.TableTypes.Inline)
            || obj.TomlType == TomlObject.TomlObjectType.ArrayOfTables;

        private void WriteValue(TomlObject obj)
        {
            switch (obj.TomlType)
            {
                case TomlObject.TomlObjectType.Bool: this.writer.Write(((TomlBool)obj).Value.ToString().ToLower()); break;
                case TomlObject.TomlObjectType.Float: this.writer.Write("{0:0.0###############}", ((TomlFloat)obj).Value); break;
                case TomlObject.TomlObjectType.Int: this.writer.Write(((TomlInt)obj).Value); break;
                case TomlObject.TomlObjectType.DateTime: this.writer.Write(((TomlDateTime)obj).ToString()); break;
                case TomlObject.TomlObjectType.TimeSpan: this.writer.Write(((TomlTimeSpan)obj).Value); break;
                case TomlObject.TomlObjectType.String:
                    {
                        this.writer.Write('\"');
                        this.writer.Write(((TomlString)obj).Value.Escape() ?? string.Empty);
                        this.writer.Write('\"');
                        break;
                    }

                default:
                    Assert(false, "This method should only get called for simple TOML Types. Check invocation code.");
                    break;
            }
        }
    }
}
