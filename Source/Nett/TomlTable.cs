using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nett
{
    public sealed class TomlTable : TomlObject
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

        public T Get<T>(string key) => this[key].Get<T>(TomlConfig.DefaultInstance);
        public T Get<T>(string key, TomlConfig config) => this[key].Get<T>(config);
        public TomlObject Get(string key) => this[key];

        public TomlTable(TableTypes tableType = TableTypes.Default)
        {
            this.TableType = tableType;
        }

        public void Add(string key, TomlObject value)
        {
            this.Rows.Add(key, value);
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override object Get(Type t, TomlConfig config)
        {
            if (t == TomlTableType) { return this; }

            var result = config.GetActivatedInstance(t);

            foreach (var p in this.Rows)
            {
                var targetProperty = result.GetType().GetProperty(p.Key);
                if (targetProperty != null)
                {
                    var converter = config.TryGetConverter(p.Value.GetType(), targetProperty.PropertyType);
                    if (converter != null)
                    {
                        var src = p.Value.Get(converter.FromType, config);
                        var val = converter.Convert(src, targetProperty.PropertyType);
                        targetProperty.SetValue(result, val, null);
                    }
                    else
                    {
                        targetProperty.SetValue(result, p.Value.Get(targetProperty.PropertyType, config), null);
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

        public static TomlTable From<T>(T obj, TomlConfig config, TableTypes tableType = TomlTable.TableTypes.Default)
        {
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }
            if (config == null) { throw new ArgumentNullException(nameof(config)); }

            TomlTable tt = new TomlTable(tableType);

            var t = obj.GetType();
            var props = t.GetProperties();

            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = TomlObject.From(val, p, config);
                    AddComments(to, p);
                    allObjects.Add(Tuple.Create(p.Name, to));
                }
            }

            tt.AddValues(allObjects);
            tt.AddComplex(allObjects);

            return tt;
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
