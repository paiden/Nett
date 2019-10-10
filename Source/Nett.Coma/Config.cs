namespace Nett.Coma
{
    using System;
    using System.Linq;
    using Nett.Coma.Path;
    using Nett.Extensions;

    public sealed partial class Config
    {
        private IMergeConfigStore persistable;

        internal Config(IMergeConfigStore persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
        }

        public TRet Get<TRet>(Func<TomlTable, TRet> getter)
        {
            getter.CheckNotNull(nameof(getter));

            var cfg = this.persistable.Load();
            return getter(cfg);
        }

        public IConfigSource GetSource(Func<TomlTable, object> getter)
        {
            getter.CheckNotNull(nameof(getter));

            var cfg = this.persistable.LoadSourcesTable();
            return (IConfigSource)getter(cfg);
        }

        public void Set(Action<TomlTable> setter)
        {
            setter.CheckNotNull(nameof(setter));

            var tbl = this.persistable.Load();
            setter(tbl);
            this.persistable.Save(tbl);
        }

        public void Set(Action<TomlTable> setter, IConfigSource target)
        {
            setter.CheckNotNull(nameof(setter));

            var tbl = this.persistable.Load(target);
            setter(tbl);
            this.persistable.Save(tbl, target);
        }

        public IDisposable StartTransaction()
        {
            var transaction = Transaction.Start(this.persistable, onCloseTransactionCallback: original => this.persistable = original);
            this.persistable = transaction;
            return transaction;
        }

        public TomlTable Unmanaged() => this.persistable.Load();

        internal static Config<T> CreateInternal<T>(Func<T> createDefault, IMergeConfigStore store)
            where T : class
        {
            createDefault.CheckNotNull(nameof(createDefault));
            store.CheckNotNull(nameof(store));

            var cfg = createDefault();
            store.EnsureExists(Toml.Create(cfg));

            return new Config<T>(store);
        }

        internal bool Clear(TPath path, bool fromAllSources)
        {
            if (path.TryGet(this.persistable.LoadSourcesTable()) is TomlSource ste)
            {
                if (fromAllSources)
                {
                    bool cleared = false;
                    foreach (var s in this.persistable.Sources)
                    {
                        cleared |= this.Clear(path, s);
                    }
                }
                else
                {
                    return this.Clear(path, ste.Value);
                }
            }
            else if (path.TryGet(this.persistable.Load()) is TomlTable tbl)
            {
                return this.ClearTable(path, tbl, fromAllSources);
            }

            return false;
        }

        internal bool Clear(TPath path, IConfigSource source)
        {
            var sourceTable = this.persistable.Load(source);
            var wasRemoved = path.Clear(sourceTable);
            this.persistable.Save(sourceTable, source);
            return wasRemoved;
        }

        internal TomlObject GetFromPath(TPath path)
        {
            path.CheckNotNull(nameof(path));

            var cfg = this.persistable.Load();
            return path.Get(cfg);
        }

        internal void Set(TPath path, object value)
        {
            path.CheckNotNull(nameof(path));
            value.CheckNotNull(nameof(value));

            bool notStoredInConfigYet = this.TryGetSource(path) == null;
            if (notStoredInConfigYet)
            {
                this.Set(path, value, this.persistable.RootSource);
            }
            else
            {
                this.Set(tbl => path.Set(tbl, cur => CreateNewValueObject(cur, tbl.Root, value)));
            }
        }

        internal void Set(TPath path, object value, IConfigSource source)
        {
            path.CheckNotNull(nameof(path));
            value.CheckNotNull(nameof(value));

            this.Set(tbl => path.Set(tbl, cur => CreateNewValueObject(cur, tbl.Root, value)), source);
        }

        internal IConfigSource TryGetSource(TPath path)
        {
            path.CheckNotNull(nameof(path));
             
            var cfgTable = this.persistable.LoadSourcesTable();
            var source = path.TryGet(cfgTable) as TomlSource;
            return source?.Value;
        }

        private static TomlObject CreateNewValueObject(TomlObject current, ITomlRoot root, object value)
        {
            var newTobj = TomlObject.CreateFrom(root, value);
            newTobj.AddComments(current?.Comments ?? Enumerable.Empty<TomlComment>());
            return newTobj;
        }

        private bool ClearTable(TPath tablePath, TomlTable table, bool fromAllSources)
        {
            bool cleared = false;
            foreach (var rowPath in tablePath.BuildForTableItems(table))
            {
                cleared |= this.Clear(rowPath, fromAllSources);
            }

            this.persistable.RemoveEmptyTables();

            return cleared;
        }
    }
}
