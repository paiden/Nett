using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nett
{
    public static class TomlTableExtensions
    {
        public static TomlInt Add(this TomlTable table, string key, int value)
        {
            var i = new TomlInt(table.MetaData, value);
            table.Add(key, i);
            return i;
        }

        public static TomlFloat Add(this TomlTable table, string key, double value)
        {
            var f = new TomlFloat(table.MetaData, value);
            table.Add(key, f);
            return f;
        }

        public static TomlString Add(this TomlTable table, string key, string value)
        {
            var s = new TomlString(table.MetaData, value);
            table.Add(key, s);
            return s;
        }

        public static TomlArray Add(this TomlTable table, string key, params long[] values)
        {
            var a = new TomlArray(table.MetaData, values.Select(v => new TomlInt(table.MetaData, v)).ToArray());
            table.Add(key, a);
            return a;
        }
    }

    public partial class TomlTable : TomlObject
    {
        public enum TableTypes
        {
            Default,
            Inline,
        }

        private static readonly Type EnumerableType = typeof(IEnumerable);
        private static readonly Type StringType = typeof(string);
        private static readonly Type TomlTableType = typeof(TomlTable);

        public Dictionary<string, TomlObject> Rows { get; } = new Dictionary<string, TomlObject>();

        internal bool IsDefined { get; set; }

        public override string ReadableTypeName => "table";

        public TomlObject this[string key]
        {
            get
            {
                TomlObject val;
                if (!this.Rows.TryGetValue(key, out val))
                {
                    throw new KeyNotFoundException(string.Format("No row with key '{0}' exists in this TOML table.", key));
                }

                return val;
            }
        }

        public TableTypes TableType { get; }

        public T Get<T>(string key) => this[key].Get<T>();
        public TomlObject Get(string key) => this[key];

        internal TomlTable(IMetaDataStore metaData, TableTypes tableType = TableTypes.Default)
            : base(metaData)
        {
            this.TableType = tableType;
        }

        /*public void Add(string key, double @float) => this.Add(key, new TomlFloat(this.MetaData, @float));
        public void Add(string key, string @string) => this.Add(key, new TomlString(this.MetaData, @string));*/

        internal void Add(string key, TomlObject value)
        {
            this.Rows.Add(key, value);
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override object Get(Type t)
        {
            if (t == TomlTableType) { return this; }

            var result = this.MetaData.Config.GetActivatedInstance(t);

            foreach (var p in this.Rows)
            {
                var targetProperty = result.GetType().GetProperty(p.Key);
                if (targetProperty != null)
                {
                    var converter = this.MetaData.Config.TryGetConverter(p.Value.GetType(), targetProperty.PropertyType);
                    if (converter != null)
                    {
                        var src = p.Value.Get(converter.FromType);
                        var val = converter.Convert(this.MetaData, src, targetProperty.PropertyType);
                        targetProperty.SetValue(result, val, null);
                    }
                    else
                    {
                        targetProperty.SetValue(result, p.Value.Get(targetProperty.PropertyType), null);
                    }
                }
            }

            return result;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var converter = new TomlTableToDictionaryConverter();
            return converter.Convert(this);
        }

        public T TryGet<T>(string key) where T : TomlObject
        {
            TomlObject o;
            this.Rows.TryGetValue(key, out o);
            return o as T;
        }

        internal static TomlTable CreateFromClass<T>(IMetaDataStore metaData, T obj, TableTypes tableType = TableTypes.Default)
            where T : class
        {
            if (metaData == null) { throw new ArgumentNullException(nameof(metaData)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            TomlTable tt = new TomlTable(metaData, tableType);

            var props = obj.GetType().GetProperties();
            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = TomlObject.CreateFrom(metaData, val, p);
                    AddComments(to, p);
                    allObjects.Add(Tuple.Create(p.Name, to));
                }
            }

            tt.AddValues(allObjects);
            tt.AddComplex(allObjects);

            return tt;
        }

        internal static TomlTable CreateFromDictionary(IMetaDataStore metaData, IDictionary dict, TableTypes tableType = TableTypes.Default)
        {
            if (metaData == null) { throw new ArgumentNullException(nameof(metaData)); }
            if (dict == null) { throw new ArgumentNullException(nameof(dict)); }

            var tomlTable = new TomlTable(metaData, tableType);

            foreach (DictionaryEntry r in dict)
            {
                var obj = TomlObject.CreateFrom(metaData, r.Value, null);
                tomlTable.Add((string)r.Key, obj);
            }

            return tomlTable;
        }

        internal override void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            base.OverwriteCommentsWithCommentsFrom(src, overwriteWithEmpty);

            var srcTable = src as TomlTable;

            if (srcTable != null)
            {
                foreach (var r in this.Rows)
                {
                    TomlObject sourceVal;
                    if (srcTable.Rows.TryGetValue(r.Key, out sourceVal))
                    {
                        r.Value.OverwriteCommentsWithCommentsFrom(sourceVal, overwriteWithEmpty);
                    }
                }
            }
        }

        internal object Merge(TomlTable tt)
        {
            throw new NotImplementedException();
        }

        private static void AddComments(TomlObject obj, PropertyInfo pi)
        {
            var comments = pi.GetCustomAttributes(typeof(TomlCommentAttribute), false).Cast<TomlCommentAttribute>();
            foreach (var c in comments)
            {
                obj.Comments.Add(new TomlComment(c.Comment, c.Location));
            }
        }

        private void AddValues(List<Tuple<string, TomlObject>> allObjects)
        {
            var adder = new ValueAdder(this);
            this.AddViaAdder(adder, allObjects);
        }

        private void AddComplex(List<Tuple<string, TomlObject>> allObjects)
        {
            var adder = new ComplexAdder(this);
            this.AddViaAdder(adder, allObjects);
        }

        private void AddViaAdder(Adder adder, List<Tuple<string, TomlObject>> allObjects)
        {
            foreach (var o in allObjects)
            {
                adder.RowKey = o.Item1;
                o.Item2.Visit(adder);
            }
        }

        private abstract class Adder : TomlObjectVisitor
        {
            public string RowKey { set; protected get; }
        }

        private sealed class ValueAdder : Adder
        {
            private readonly TomlTable target;

            public ValueAdder(TomlTable target)
            {
                this.target = target;
                this.VisitBool = (b) => target.Add(this.RowKey, b);
                this.VisitArray = (a) => target.Add(this.RowKey, a);
                this.VisitDateTime = (dt) => target.Add(this.RowKey, dt);
                this.VisitFloat = (f) => target.Add(this.RowKey, f);
                this.VisitInt = (i) => target.Add(this.RowKey, i);
                this.VisitString = (s) => target.Add(this.RowKey, s);
                this.VisitTimespan = (ts) => target.Add(this.RowKey, ts);
            }
        }

        private sealed class ComplexAdder : Adder
        {
            public ComplexAdder(TomlTable target)
            {
                this.VisitTableArray = (rowTblArray) => target.Add(this.RowKey, rowTblArray);
                this.VisitTable = (rowTbl) => target.Add(this.RowKey, rowTbl);
            }
        }
    }
}
