namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    using static System.Diagnostics.Debug;

    public static class TomlTableExtensions
    {
        public static TomlInt Add(this TomlTable table, string key, int value)
        {
            var i = new TomlInt(table.Root, value);
            table.Add(key, i);
            return i;
        }

        public static TomlFloat Add(this TomlTable table, string key, double value)
        {
            var f = new TomlFloat(table.Root, value);
            table.Add(key, f);
            return f;
        }

        public static TomlString Add(this TomlTable table, string key, string value)
        {
            var s = new TomlString(table.Root, value);
            table.Add(key, s);
            return s;
        }

        public static TomlArray Add(this TomlTable table, string key, params long[] values)
        {
            var a = new TomlArray(table.Root, values.Select(v => new TomlInt(table.Root, v)).ToArray());
            table.Add(key, a);
            return a;
        }

        public static TomlTable Add<T>(
            this TomlTable table, string key, T obj, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
            where T : class
        {
            var t = TomlTable.CreateFromClass(table.Root, obj, tableType);
            table.Add(key, t);
            return t;
        }
    }

    public partial class TomlTable : TomlObject, IDictionary<string, TomlObject>
    {
        private readonly Dictionary<string, TomlObject> rows = new Dictionary<string, TomlObject>();

        private volatile bool isFrozen = false;

        internal TomlTable(ITomlRoot root, TableTypes tableType = TableTypes.Default)
            : this(root, Enumerable.Empty<KeyValuePair<string, TomlObject>>(), tableType)
        {
        }

        internal TomlTable(ITomlRoot root, IEnumerable<KeyValuePair<string, TomlObject>> pairs, TableTypes tableType = TableTypes.Default)
            : base(root)
        {
            this.TableType = tableType;
            foreach (var pair in pairs)
            {
                this.rows.Add(new TomlKey(pair.Key), pair.Value);
            }
        }

        public enum TableTypes
        {
            Default,
            Inline,
        }

        public int Count => this.rows.Count;

        public bool IsReadOnly => this.isFrozen;

        public ICollection<string> Keys => this.rows.Keys;

        public override string ReadableTypeName => "table";

        public IEnumerable<KeyValuePair<string, TomlObject>> Rows => this.rows.AsEnumerable();

        public TableTypes TableType { get; }

        public override TomlObjectType TomlType => TomlObjectType.Table;

        public ICollection<TomlObject> Values => this.rows.Values;

        internal bool IsDefined { get; set; }

        private IDictionary<string, TomlObject> AsDict => this;

        TomlObject IDictionary<string, TomlObject>.this[string key]
        {
            get
            {
                this.AssertIntegrity();
                TomlObject val;
                if (!this.rows.TryGetValue(key, out val))
                {
                    throw new KeyNotFoundException(string.Format("No row with key '{0}' exists in this TOML table.", key));
                }

                return val;
            }

            set
            {
                this.AssertIntegrity();
                this.CheckNotFrozen();
                this.rows[key] = this.EnsureCorrectRoot(value);
            }
        }

        public TomlObject this[string key]
        {
            get { return this.AsDict[key]; }
            internal set { this.AsDict[key] = value; }
        }

        public void Clear()
        {
            this.CheckNotFrozen();
            this.rows.Clear();
        }

        public bool Contains(KeyValuePair<string, TomlObject> item) => this.rows.Contains(item);

        public bool ContainsKey(string key) => this.rows.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, TomlObject>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Freeze()
        {
            if (this.isFrozen) { return false; }

            this.isFrozen = true;

            foreach (var r in this.rows)
            {
                var tbl = r.Value as TomlTable;
                if (tbl != null)
                {
                    tbl.Freeze();
                }
            }

            return true;
        }

        public T Get<T>(string key) => this[key].Get<T>();

        public TomlObject Get(string key) => this[key];

        public override object Get(Type t)
        {
            if (t == Types.TomlTableType) { return this; }

            var result = this.Root.Config.GetActivatedInstance(t);

            var conv = this.Root.Config.TryGetConverter(Types.TomlTableType, t);
            if (conv != null)
            {
                return conv.Convert(this.Root, this, t);
            }

            foreach (var r in this.rows)
            {
                var targetProperty = this.Root.Config.TryGetMappingProperty(result.GetType(), r.Key);
                if (targetProperty != null)
                {
                    Type keyMapingTargetType = this.Root.Config.TryGetMappedType(r.Key, targetProperty);
                    targetProperty.SetValue(result, r.Value.Get(keyMapingTargetType ?? targetProperty.PropertyType), null);
                }
            }

            return result;
        }

        public IEnumerator<KeyValuePair<string, TomlObject>> GetEnumerator() => this.rows.GetEnumerator();

        void ICollection<KeyValuePair<string, TomlObject>>.Add(KeyValuePair<string, TomlObject> item) => this.Add(item.Key, item.Value);

        void IDictionary<string, TomlObject>.Add(string key, TomlObject value) => this.Add(key, value);

        IEnumerator IEnumerable.GetEnumerator() => this.rows.GetEnumerator();

        public bool Remove(string key)
        {
            this.CheckNotFrozen();
            return this.rows.Remove(key);
        }

        public bool Remove(KeyValuePair<string, TomlObject> item)
        {
            this.CheckNotFrozen();
            return this.rows.Remove(item.Key);
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

        public bool TryGetValue(string key, out TomlObject value) => this.rows.TryGetValue(key, out value);

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal static TomlTable CreateFromClass<T>(ITomlRoot root, T obj, TableTypes tableType = TableTypes.Default)
            where T : class
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            TomlTable tt = new TomlTable(root, tableType);

            var props = root.Config.GetSerializationProperties(obj.GetType());
            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var p in props)
            {
                object val = p.GetValue(obj, null);
                if (val != null)
                {
                    TomlObject to = TomlObject.CreateFrom(root, val, p);
                    AddComments(to, p);
                    allObjects.Add(Tuple.Create(p.Name, to));
                }
            }

            tt.AddScopeTypesLast(allObjects);

            return tt;
        }

        internal static TomlTable CreateFromDictionary(ITomlRoot root, IDictionary dict, TableTypes tableType = TableTypes.Default)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (dict == null) { throw new ArgumentNullException(nameof(dict)); }

            var tomlTable = new TomlTable(root, tableType);

            foreach (DictionaryEntry r in dict)
            {
                var obj = TomlObject.CreateFrom(root, r.Value, null);
                tomlTable.Add((string)r.Key, obj);
            }

            return tomlTable;
        }

        internal TomlObject Add(string key, TomlObject value)
        {
            this.CheckNotFrozen();
            var toAdd = this.EnsureCorrectRoot(value);
            this.rows.Add(key, toAdd);
            return toAdd;
        }

        internal override void OverwriteCommentsWithCommentsFrom(TomlObject src, bool overwriteWithEmpty)
        {
            this.CheckNotFrozen();
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

        internal TomlTable TableWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            var table = new TomlTable(root, this.TableType);

            foreach (var r in this.rows)
            {
                table.Add(r.Key, r.Value.WithRoot(root));
            }

            return table;
        }

        internal override TomlObject WithRoot(ITomlRoot root) => this.TableWithRoot(root);

        private static void AddComments(TomlObject obj, PropertyInfo pi)
        {
            var comments = pi.GetCustomAttributes(typeof(TomlCommentAttribute), false).Cast<TomlCommentAttribute>();
            foreach (var c in comments)
            {
                obj.Comments.Add(new TomlComment(c.Comment, c.Location));
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

        [Conditional(Constants.Debug)]
        private void AssertIntegrity()
        {
            foreach (var r in this.rows)
            {
                const string message = "All objects that are part of the same TOML root table need to have the same root. "
                    + "Check that all add/insert operations ensure this condition if not change them accordingly. If this assertion "
                    + "triggers something in the TOML table implementation is broken and needs to be fixed.";

                Assert(r.Value.Root == this.Root, message);
                var tbl = r.Value as TomlTable;

                if (tbl != null)
                {
                    tbl.AssertIntegrity();
                }
            }
        }

        private void CheckNotFrozen()
        {
            if (this.isFrozen)
            {
                throw new InvalidOperationException("Cannot write into frozen TOML table");
            }
        }

        private TomlObject EnsureCorrectRoot(TomlObject obj) => obj.Root == this.Root ? obj : obj.WithRoot(this.Root);
    }
}
