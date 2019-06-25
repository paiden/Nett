namespace Nett
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Extensions;
    using Nett.Writer;
    using static System.Diagnostics.Debug;

    public partial class TomlTable : TomlObject, IDictionary<string, TomlObject>
    {
        private readonly Dictionary<TomlKey, TomlObject> rows = new Dictionary<TomlKey, TomlObject>();

        private volatile bool isFrozen = false;

        internal TomlTable(ITomlRoot root, TableTypes tableType = TableTypes.Default)
            : base(root)
        {
            this.TableType = tableType;
        }

        public enum TableTypes
        {
            Default,
            Inline,
            Dotted,
        }

        public int Count => this.rows.Count;

        public bool IsReadOnly => this.isFrozen;

        public ICollection<string> Keys => this.rows.Keys.Select(k => k.Value).ToList();

        public override string ReadableTypeName => "table";

        public IEnumerable<KeyValuePair<string, TomlObject>> Rows
        {
            get
            {
                var all = this.rows.Select(kvp => new KeyValuePair<string, TomlObject>(kvp.Key.Value, kvp.Value));
                var nonscoping = all.Where(kvp => !ScopeCreatingType(kvp.Value));
                var scoping = all.Where(kvp => ScopeCreatingType(kvp.Value));
                return nonscoping.Concat(scoping);
            }
        }

        public TableTypes TableType { get; internal set; }

        public override TomlObjectType TomlType => TomlObjectType.Table;

        public ICollection<TomlObject> Values => this.rows.Values;

        internal bool IsDefined { get; set; }

        internal IEnumerable<KeyValuePair<TomlKey, TomlObject>> InternalRows
            => this.NonScopingRows.Concat(this.ScopingRows);

        internal IEnumerable<KeyValuePair<TomlKey, TomlObject>> NonScopingRows
            => this.rows.Where(kvp => !ScopeCreatingType(kvp.Value));

        internal IEnumerable<KeyValuePair<TomlKey, TomlObject>> ScopingRows
            => this.rows.Where(kvp => ScopeCreatingType(kvp.Value));

        public TomlObject this[string key]
        {
            get
            {
                this.AssertIntegrity();
                if (!this.rows.TryGetValue(new TomlKey(key), out var val))
                {
                    throw new KeyNotFoundException(string.Format("No row with key '{0}' exists in this TOML table.", key));
                }

                return val;
            }

            set
            {
                value = value.Root == this.Root ? value : value.CloneFor(this.Root);
                this.AssertIntegrity();
                this.CheckNotFrozen();
                this.rows[new TomlKey(key)] = this.EnsureCorrectRoot(value);
                this.OnRowValueSet(key);
            }
        }

        /// <summary>
        /// Allows to combine two TOML tables to a new result table.
        /// </summary>
        /// <remarks>
        /// The given lambda is used to configure what combination operation should be performed.
        /// </remarks>
        /// <example>
        /// <code>
        /// var x = Toml.Create(); // Assume rows are added to X
        /// var y = Toml.Create(); // Assume rows are added to Y
        ///
        /// // Create table that has all rows of X + rows of Y that had no equivalent in row in X
        /// var r1 = Toml.CombineTables(op => op.Overwrite(X).With(Y).ForRowsOnlyInSource());
        ///
        /// // Create table that has all rows of X overwritten with the
        /// // equivalent rows from Y and added all rows that had no equivalent row in X yet
        /// var r2 = Toml.CombineTables(op => op.Overwrite(X).With(Y).ForAllSourceRows());
        ///
        /// // Create table that has all rows of X overwritten with the
        /// // equivalent row of Y, if such a row existed in Y
        /// var r3 = Toml.CombineTables(op => op.Overwrite(X).With(Y).ForAllTargetRows());
        ///
        /// // These operations create the following tables
        /// // Key | X   | Y   | r1 | r2 | r3
        /// // ------------------------------
        /// // a   | 1   |     | 1  | 1  | 1
        /// // b   |     | 2   | 2  | 2  |
        /// // c   | 3   | 4   | 3  | 4  | 4
        /// </code>
        /// </example>
        /// <param name="operation">Lambda used to configure the operation that should be performed.</param>
        /// <returns>
        /// A new TomlTable instance containing the table resulting from the operation. The new table will
        /// be a completely new deep clone of the original tables/rows.
        /// </returns>
        public static TomlTable Combine(Func<ITargetSelector, ITableCombiner> operation)
        {
            var builtOperation = (ITableOperation)operation(new TomlTable.TableOperationBuilder());
            return builtOperation.Execute();
        }

        public void Clear()
        {
            this.CheckNotFrozen();
            this.rows.Clear();
        }

        public bool Contains(KeyValuePair<string, TomlObject> item)
            => this.rows.Contains(new KeyValuePair<TomlKey, TomlObject>(new TomlKey(item.Key), item.Value));

        public bool ContainsKey(string key) => this.rows.ContainsKey(new TomlKey(key));

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

        public IEnumerator<KeyValuePair<string, TomlObject>> GetEnumerator()
        {
            var iterator = this.rows.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return new KeyValuePair<string, TomlObject>(iterator.Current.Key.Value, iterator.Current.Value);
            }
        }

        void ICollection<KeyValuePair<string, TomlObject>>.Add(KeyValuePair<string, TomlObject> item)
            => this.AddRow(new TomlKey(item.Key), item.Value);

        void IDictionary<string, TomlObject>.Add(string key, TomlObject value)
            => this.AddRow(new TomlKey(key), value);

        IEnumerator IEnumerable.GetEnumerator() => this.rows.GetEnumerator();

        public bool Remove(string key)
        {
            this.CheckNotFrozen();
            return this.rows.Remove(new TomlKey(key));
        }

        public bool Remove(KeyValuePair<string, TomlObject> item)
        {
            this.CheckNotFrozen();
            return this.rows.Remove(new TomlKey(item.Key));
        }

        public Dictionary<string, object> ToDictionary()
        {
            var converter = new ConvertTomlTableToDictionaryConversionVisitor();
            return converter.Convert(this);
        }

        public TomlObject TryGetValue(string key)
        {
            TomlObject o;
            this.rows.TryGetValue(new TomlKey(key), out o);
            return o;
        }

        public bool TryGetValue(string key, out TomlObject value)
            => this.rows.TryGetValue(new TomlKey(key), out value);

        public override void Visit(ITomlObjectVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
            => TomlTableWriter.WriteTomlFragment(this);

        internal static TomlTable CreateFromComplexObject<T>(ITomlRoot root, T obj, TableTypes tableType)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            TomlTable tt = new TomlTable(root, tableType);

            var members = root.Settings.GetSerializationMembers(obj.GetType());
            var allObjects = new List<Tuple<string, TomlObject>>();

            foreach (var m in members)
            {
                object val = m.GetValue(obj);
                if (val != null)
                {
                    TomlObject to = TomlObject.CreateFrom(root, val);
                    to.AddComments(root.Settings.GetComments(obj.GetType(), m.Member));
                    tt.AddRow(m.Key, to);
                }
            }

            return tt;
        }

        internal static TomlTable CreateFromDictionary<T>(ITomlRoot root, IDictionary<string, T> dict, TableTypes tableType)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (dict == null) { throw new ArgumentNullException(nameof(dict)); }

            var tomlTable = new TomlTable(root, tableType);

            foreach (var kvp in dict)
            {
                var obj = TomlObject.CreateFrom(root, kvp.Value);
                tomlTable.AddRow(new TomlKey(kvp.Key), obj);
            }

            return tomlTable;
        }

        internal static TomlTable CreateFromDictionary(ITomlRoot root, IDictionary dict, TableTypes tableType)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (dict == null) { throw new ArgumentNullException(nameof(dict)); }

            var tomlTable = new TomlTable(root, tableType);

            foreach (DictionaryEntry r in dict)
            {
                var obj = TomlObject.CreateFrom(root, r.Value);
                tomlTable.AddRow(new TomlKey((string)r.Key), obj);
            }

            return tomlTable;
        }

        internal TomlObject AddRow(TomlKey key, TomlObject value)
        {
            this.CheckNotFrozen();
            var toAdd = this.EnsureCorrectRoot(value);
            this.rows.Add(key, toAdd);
            return toAdd;
        }

        internal override TomlObject CloneFor(ITomlRoot root) => this.CloneTableFor(root);

        internal TomlTable CloneTableFor(ITomlRoot root)
        {
            var tbl = new TomlTable(root, this.TableType);

            foreach (var r in this.rows)
            {
                tbl.AddRow(r.Key, r.Value.CloneFor(root));
            }

            return tbl;
        }

        internal override object GetInternal(Type t, Func<IEnumerable<string>> getMyKeyChain)
        {
            if (t == Types.TomlTableType) { return this; }

            var conv = this.Root.Settings.TryGetConverter(Types.TomlTableType, t);
            if (conv != null)
            {
                return conv.Convert(this.Root, this, t);
            }

            var result = this.Root.Settings.GetActivatedInstance(t);
            foreach (var r in this.rows)
            {
                var targetMember = this.Root.Settings.TryGetMappedMember(result.GetType(), r.Key.Value);
                if (targetMember.HasValue)
                {
                    Type keyMapingTargetType = this.Root.Settings.TryGetMappedType(r.Key.Value, targetMember);

                    object value = null;
                    try
                    {
                        value = r.Value.GetInternal(
                            keyMapingTargetType ?? targetMember.Value.MemberType,
                            () => getMyKeyChain().Concat(r.Key.Value.ToEnumerable()));
                    }
                    catch (Exception exc)
                    {
                        throw new InvalidOperationException(
                            $"Failed to convert TOML object with key '{r.Key}', " + $"type '{r.Value.ReadableTypeName}' " +
                            $"and value '{r.Value.ToString()}' to object property " +
                            $"with name '{targetMember.Value.MemberInfo.Name}' and type '{targetMember.Value.MemberType.FullName}'.",
                            exc);
                    }

                    targetMember.Value.SetValue(result, value);
                }
                else
                {
                    this.Root.Settings.OnTargetPropertyNotFound(
                        getMyKeyChain().Concat(r.Key.Value.ToEnumerable()).ToArray(), result, r.Value);
                }
            }

            return result;
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

        internal void SetRow(TomlKey key, TomlObject value)
        {
            this.rows[key] = value;
        }

        internal TomlObject TryGetValue(TomlKey key) => this.TryGetValue(key.Value);

        internal TomlTable TableWithRoot(ITomlRoot root)
        {
            root.CheckNotNull(nameof(root));

            var table = new TomlTable(root, this.TableType);

            foreach (var r in this.rows)
            {
                table.AddRow(r.Key, r.Value.WithRoot(root));
            }

            return table;
        }

        internal override TomlObject WithRoot(ITomlRoot root) => this.TableWithRoot(root);

        protected virtual void OnRowValueSet(string rowKey)
        {
        }

        private static bool ScopeCreatingType(TomlObject obj) =>
            (obj.TomlType == TomlObjectType.Table && ((TomlTable)obj).TableType != TableTypes.Inline)
            || obj.TomlType == TomlObjectType.ArrayOfTables;

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
