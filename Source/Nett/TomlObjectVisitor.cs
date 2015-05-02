using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nett
{
    public abstract class TomlObjectVisitor
    {
        protected Action<TomlInt> VisitInt { get; set; }
        protected Action<TomlFloat> VisitFloat { get; set; }
        protected Action<TomlBool> VisitBool { get; set; }

        protected Action<TomlString> VisitString { get; set; }

        protected Action<TomlTimeSpan> VisitTimespan { get; set; }

        protected Action<TomlTable> VisitTable { get; set; }

        protected Action<TomlTableArray> VisitTableArray { get; set; }

        protected Action<TomlDateTime> VisitDateTime { get; set; }
        protected Action<TomlArray> VisitArray { get; set; }

        internal void Visit(TomlTable table)
        {
            if(this.VisitTable != null) { this.VisitTable(table); }
        }

        internal void Visit(TomlTableArray tableArray)
        {
            if(this.VisitTableArray != null) { this.VisitTableArray(tableArray); }
        }

        internal void Visit(TomlInt i)
        {
            if(this.VisitInt != null) { this.VisitInt(i); }
        }

        internal void Visit(TomlFloat f)
        {
            if(this.VisitFloat != null) { this.VisitFloat(f); }
        }

        internal void Visit(TomlBool b)
        {
            if(this.VisitBool != null) { this.VisitBool(b); }
        }

        internal void Visit(TomlString s)
        {
            if(this.VisitString != null) { this.VisitString(s); }
        }

        internal void Visit(TomlTimeSpan ts)
        {
            if(this.VisitTimespan != null) { this.VisitTimespan(ts); }
        }

        internal void Visit(TomlDateTime dt)
        {
            if(this.VisitDateTime != null) { this.VisitDateTime(dt); }
        }

        internal void Visit(TomlArray a)
        {
            if(this.VisitArray != null) { this.VisitArray(a); }
        }
    }
}
