namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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
        private static readonly Type EnumerableType = typeof(IEnumerable);

        private static readonly Type StringType = typeof(string);

        private static readonly Type TomlTableType = typeof(TomlTable);

        private readonly Dictionary<string, TomlObject> rows = new Dictionary<string, TomlObject>();

        internal TomlTable(IMetaDataStore metaData, TableTypes tableType = TableTypes.Default)
            : base(metaData)
        {
            this.TableType = tableType;
        }

        public enum TableTypes
        {
            Default,
            Inline,
        }

        public IEnumerable<KeyValuePair<string, TomlObject>> Rows => this.rows.AsEnumerable();

        public override string ReadableTypeName => "table";

        public TableTypes TableType { get; }

        public override TomlObjectType TomlType => TomlObjectType.Table;

        internal bool IsDefined { get; set; }

        public TomlObject this[string key]
        {
            get
            {
                TomlObject val;
                if (!this.rows.TryGetValue(key, out val))
                {
                    throw new KeyNotFoundException(string.Format("No row with key '{0}' exists in this TOML table.", key));
                }

                return val;
            }
        }

        public T Get<T>(string key) => this[key].Get<T>();

        public TomlObject Get(string key) => this[key];

        /*public void Add(string key, double @float) => this.Add(key, new TomlFloat(this.MetaData, @float));
        public void Add(string key, string @string) => this.Add(key, new TomlString(this.MetaData, @string));*/

        public override object Get(Type t)
        {
            if (t == TomlTableType) { return this; }

            var result = this.MetaData.Config.GetActivatedInstance(t);

            foreach (var r in this.rows)
            {
                var targetProperty = result.GetType().GetProperty(r.Key);
                if (targetProperty != null && !this.MetaData.Config.IsPropertyIgnored(result.GetType(), targetProperty))
                {
                    MapTableRowToProperty(result, targetProperty, r, this.MetaData);
                }
            }

            return result;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var converter = new ConvertTomlTableToDictionaryConversionVisitor();
            return converter.Convert(this);
        }

        public TomlObject TryGetValue(string key)
        {
            TomlObject o;
            this.rows.TryGetValue(key, out o);
            return o;
        }

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlTable CreateFromClass<T>(IMetaDataStore metaData, T obj, TableTypes tableType = TableTypes.Default)
            where T : class
        {
            if (metaData == null) { throw new ArgumentNullException(nameof(metaData)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            TomlTable tt = new TomlTable(metaData, tableType);

            var props = obj.GetType().GetProperties();
            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var p in props.Where(filter => !metaData.Config.IsPropertyIgnored(obj.GetType(), filter)))
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = TomlObject.CreateFrom(metaData, val, p);
                    AddComments(to, p);
                    allObjects.Add(Tuple.Create(p.Name, to));
                }
            }

            tt.AddScopeTypesLast(allObjects);

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

        internal void Add(string key, TomlObject value)
        {
            this.rows.Add(key, value);
        }

        internal object Merge(TomlTable tt)
        {
            throw new NotImplementedException();
        }

        internal override void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            base.OverwriteCommentsWithCommentsFrom(src, overwriteWithEmpty);

            var srcTable = src as TomlTable;

            if (srcTable != null)
            {
                foreach (var r in this.rows)
                {
                    TomlObject sourceVal;
                    if (srcTable.rows.TryGetValue(r.Key, out sourceVal))
                    {
                        r.Value.OverwriteCommentsWithCommentsFrom(sourceVal, overwriteWithEmpty);
                    }
                }
            }
        }

        private static void AddComments(TomlObject obj, PropertyInfo pi)
        {
            var comments = pi.GetCustomAttributes(typeof(TomlCommentAttribute), false).Cast<TomlCommentAttribute>();
            foreach (var c in comments)
            {
                obj.Comments.Add(new TomlComment(c.Comment, c.Location));
            }
        }

        private static void MapTableRowToProperty(
            object target, PropertyInfo property, KeyValuePair<string, TomlObject> tableRow, IMetaDataStore metaData)
        {
            Type keyMapingTargetType = metaData.Config.TryGetMappedType(tableRow.Key, property);

            var converter = metaData.Config.TryGetConverter(tableRow.Value.GetType(), property.PropertyType);
            if (converter != null && keyMapingTargetType == null)
            {
                var src = tableRow.Value.Get(converter.FromType);
                var val = converter.Convert(metaData, src, property.PropertyType);
                property.SetValue(target, val, null);
            }
            else
            {
                property.SetValue(target, tableRow.Value.Get(keyMapingTargetType ?? property.PropertyType), null);
            }
        }

        private static bool ScopeCreatingType(TomlObject obj) =>
            obj.TomlType == TomlObjectType.Table || obj.TomlType == TomlObjectType.ArrayOfTables;

        private void AddScopeTypesLast(List<Tuple<string, TomlObject>> allObjects)
        {
            foreach (var a in allObjects.Where(o => !ScopeCreatingType(o.Item2)))
            {
                this.Add(a.Item1, a.Item2);
            }

            foreach (var a in allObjects.Where(o => ScopeCreatingType(o.Item2)))
            {
                this.Add(a.Item1, a.Item2);
            }
        }
    }
}
