using System.Collections.Generic;
using System.Diagnostics;

namespace Nett
{
    internal sealed class TomlTableToDictionaryConverter : ITomlObjectVisitor
    {
#if DEBUG
        private bool InvokedConvert;
#endif
        private readonly Dictionary<string, object> table = new Dictionary<string, object>();
        private string currentKey;

        public TomlTableToDictionaryConverter()
        {
        }

        public Dictionary<string, object> Convert(TomlTable table)
        {
#if DEBUG
            Debug.Assert(!this.InvokedConvert, "Do not reuse the TomlTable Converter. The converter can only be used once. Create a new converter instead.");
            this.InvokedConvert = true;
#endif
            foreach (var r in table.Rows)
            {
                this.currentKey = r.Key;
                r.Value.Visit(this);
            }

            return this.table;
        }

        void ITomlObjectVisitor.Visit(TomlInt i) => this.table[this.currentKey] = i.Value;

        void ITomlObjectVisitor.Visit(TomlBool b) => this.table[this.currentKey] = b.Value;

        void ITomlObjectVisitor.Visit(TomlTimeSpan ts) => this.table[this.currentKey] = ts.Value;

        void ITomlObjectVisitor.Visit(TomlArray a)
        {
            var clrArray = new object[a.Length];
            var extractItem = new ExtractItemValue();
            for (int i = 0; i < a.Items.Length; i++)
            {
                a[i].Visit(extractItem);
                clrArray[i] = extractItem.Item;
            }

            this.table[this.currentKey] = clrArray;
        }

        void ITomlObjectVisitor.Visit(TomlDateTime dt) => this.table[this.currentKey] = dt.Value;

        void ITomlObjectVisitor.Visit(TomlString s) => this.table[this.currentKey] = s.Value;

        void ITomlObjectVisitor.Visit(TomlFloat f) => this.table[this.currentKey] = f.Value;

        void ITomlObjectVisitor.Visit(TomlTableArray tableArray)
        {
            var arr = new Dictionary<string, object>[tableArray.Count];

            for (int i = 0; i < tableArray.Count; i++)
            {
                var conv = new TomlTableToDictionaryConverter();
                arr[i] = conv.Convert(tableArray[i]);
            }

            this.table[this.currentKey] = arr;
        }

        void ITomlObjectVisitor.Visit(TomlTable table)
        {
            var conv = new TomlTableToDictionaryConverter();
            this.table[this.currentKey] = conv.Convert(table); ;
        }

        private class ExtractItemValue : ITomlObjectVisitor
        {
            public object Item;
            public void Visit(TomlInt i) => this.Item = i.Value;

            public void Visit(TomlBool b) => this.Item = b.Value;

            public void Visit(TomlTimeSpan ts) => this.Item = ts.Value;

            public void Visit(TomlArray a)
            {
                var clrArray = new object[a.Length];
                var extractItem = new ExtractItemValue();
                for (int i = 0; i < a.Items.Length; i++)
                {
                    a[i].Visit(extractItem);
                    clrArray[i] = extractItem.Item;
                }

                this.Item = clrArray;
            }

            public void Visit(TomlDateTime dt) => this.Item = dt.Value;

            public void Visit(TomlString s) => this.Item = s.Value;

            public void Visit(TomlFloat f) => this.Item = f.Value;

            public void Visit(TomlTableArray tableArray) => Debug.Assert(false);

            public void Visit(TomlTable table) => Debug.Assert(false);
        }
    }
}
