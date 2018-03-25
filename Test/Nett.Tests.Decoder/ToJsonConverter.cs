using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Nett.TestDecoder
{
    public class ToJsonConverter : ITomlObjectVisitor
    {
        private StringBuilder sb = new StringBuilder();

        public string Convert(TomlTable table)
        {
            table.Visit(this);
            return this.sb.ToString();
        }

        void ITomlObjectVisitor.Visit(TomlInt i) =>
            sb.Append("{").Append("\"type\":\"integer\", \"value\":\"").Append(i.Value).Append("\"}");

        void ITomlObjectVisitor.Visit(TomlBool b) =>
            sb.Append("{").Append("\"type\":\"bool\", \"value\":\"").Append(b.Value ? "true" : "false").Append("\"}");

        void ITomlObjectVisitor.Visit(TomlDuration ts) =>
            sb.Append("{").Append("\"type\":\"timespan\", \"value\":\"").Append(ts.Value).Append("\"}");

        void ITomlObjectVisitor.Visit(TomlArray a)
        {
            sb.Append("{").Append("\"type\":\"array\", \"value\":[");

            for (int i = 0; i < a.Length - 1; i++)
            {
                a[i].Visit(this);
                this.sb.Append(",");
            }

            if (a.Length > 0) { a[a.Length - 1].Visit(this); }

            this.sb.Append("]}");
        }

        void ITomlObjectVisitor.Visit(TomlDateTime dt) =>
            sb.Append("{").Append("\"type\":\"datetime\", \"value\":\"").Append(XmlConvert.ToString(dt.Value.UtcDateTime, XmlDateTimeSerializationMode.Utc)).Append("\"}");

        void ITomlObjectVisitor.Visit(TomlString s) =>
            sb.Append("{").Append("\"type\":\"string\", \"value\":\"").Append(s.Value.Replace(Environment.NewLine, "\n").Escape()).Append("\"}");

        void ITomlObjectVisitor.Visit(TomlFloat f) =>
            sb.Append("{").Append("\"type\":\"float\", \"value\":\"").Append(f.Value.ToString(CultureInfo.InvariantCulture)).Append("\"}");

        void ITomlObjectVisitor.Visit(TomlTableArray a)
        {
            sb.Append("[");

            for (int i = 0; i < a.Count - 1; i++)
            {
                a[i].Visit(this);
                this.sb.Append(",");
            }

            if (a.Count > 0) { a[a.Count - 1].Visit(this); }

            this.sb.Append("]");
        }

        void ITomlObjectVisitor.Visit(TomlTable table)
        {
            this.sb.Append("{");

            foreach (var r in table.Rows)
            {
                this.sb.Append("\"").Append(r.Key).Append("\":");
                r.Value.Visit(this);
                this.sb.Append(",");
            }

            if (this.sb[this.sb.Length - 1] == ',')
                this.sb.Remove(this.sb.Length - 1, 1);

            this.sb.Append("}");
        }
    }
}
