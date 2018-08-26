namespace Nett.Writer
{
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Util;

    using static System.Diagnostics.Debug;

    internal abstract class TomlWriter
    {
        protected readonly TomlSettings settings;
        protected readonly FormattingStreamWriter writer;

        public TomlWriter(FormattingStreamWriter writer, TomlSettings settings)
        {
            this.writer = writer;
            this.settings = settings;
        }

        protected void WriteAppendComments(TomlObject obj)
        {
            var append = obj.Comments.Where((c) => this.settings.GetCommentLocation(c) == TomlCommentLocation.Append);
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

        protected void WriteArray(TomlKey key, TomlArray array)
        {
            this.writer.Write(key.ToString());
            this.writer.Write(" = ");

            WriteArrayPart(array);

            void WriteArrayPart(TomlArray ap)
            {
                this.writer.Write('[');

                for (int i = 0; i < ap.Items.Length - 1; i++)
                {
                    WriteArrayElement(ap.Items[i]);
                    this.writer.Write(", ");
                }

                if (ap.Items.Length > 0)
                {
                    WriteArrayElement(ap.Items[ap.Items.Length - 1]);
                }

                this.writer.Write(']');
            }

            void WriteArrayElement(TomlValue value)
            {
                if (value is TomlArray arr)
                {
                    WriteArrayPart(arr);
                }
                else
                {
                    this.WriteValue(value);
                }
            }
        }

        protected void WriteKeyedValue(KeyValuePair<TomlKey, TomlObject> kvp)
        {
            this.writer.Write(kvp.Key.ToString());
            this.writer.Write(" = ");
            this.WriteValue(kvp.Value);
        }

        protected void WritePrependComments(TomlObject obj)
        {
            var prepend = obj.Comments.Where((c) => this.settings.GetCommentLocation(c) == TomlCommentLocation.Prepend);
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

        protected void WriteValue(TomlObject obj)
        {
            switch (obj.TomlType)
            {
                case TomlObjectType.Bool: this.writer.Write(((TomlBool)obj).Value.ToString().ToLower()); break;
                case TomlObjectType.Float: this.writer.Write("{0:0.0###############}", ((TomlFloat)obj).Value); break;
                case TomlObjectType.Int: this.writer.Write(((TomlInt)obj).Value); break;
                case TomlObjectType.DateTime: this.writer.Write(((TomlDateTime)obj).ToString()); break;
                case TomlObjectType.TimeSpan: this.writer.Write(((TomlDuration)obj).ToString()); break;
                case TomlObjectType.String:
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

        private static string FixMultilineComment(string src) => src.Replace("\n", "\n#");

        private bool ShouldAppendWithNewline(TomlObject obj) => !this.ShouldPrependWithNewline(obj);

        private bool ShouldPrependWithNewline(TomlObject obj) =>
            (obj.TomlType == TomlObjectType.Table && ((TomlTable)obj).TableType != TomlTable.TableTypes.Inline)
            || obj.TomlType == TomlObjectType.ArrayOfTables;


    }
}
