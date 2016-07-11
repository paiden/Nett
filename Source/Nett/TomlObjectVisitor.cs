namespace Nett
{
    using System;

    internal abstract class TomlObjectVisitor : ITomlObjectVisitor
    {
        protected Action<TomlArray> VisitArray { get; set; }

        protected Action<TomlBool> VisitBool { get; set; }

        protected Action<TomlDateTime> VisitDateTime { get; set; }

        protected Action<TomlFloat> VisitFloat { get; set; }

        protected Action<TomlInt> VisitInt { get; set; }

        protected Action<TomlString> VisitString { get; set; }

        protected Action<TomlTable> VisitTable { get; set; }

        protected Action<TomlTableArray> VisitTableArray { get; set; }

        protected Action<TomlTimeSpan> VisitTimespan { get; set; }

        public void Visit(TomlTable table)
        {
            if (this.VisitTable != null) { this.VisitTable(table); }
        }

        public void Visit(TomlTableArray tableArray)
        {
            if (this.VisitTableArray != null) { this.VisitTableArray(tableArray); }
        }

        public void Visit(TomlInt i)
        {
            if (this.VisitInt != null) { this.VisitInt(i); }
        }

        public void Visit(TomlFloat f)
        {
            if (this.VisitFloat != null) { this.VisitFloat(f); }
        }

        public void Visit(TomlBool b)
        {
            if (this.VisitBool != null) { this.VisitBool(b); }
        }

        public void Visit(TomlString s)
        {
            if (this.VisitString != null) { this.VisitString(s); }
        }

        public void Visit(TomlTimeSpan ts)
        {
            if (this.VisitTimespan != null) { this.VisitTimespan(ts); }
        }

        public void Visit(TomlDateTime dt)
        {
            if (this.VisitDateTime != null) { this.VisitDateTime(dt); }
        }

        public void Visit(TomlArray a)
        {
            if (this.VisitArray != null) { this.VisitArray(a); }
        }
    }
}
