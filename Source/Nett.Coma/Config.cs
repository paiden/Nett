namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
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

        internal bool Clear(TPath path)
        {
            var ste = path.TryApply(this.persistable.LoadSourcesTable()) as TomlSource;

            if (ste == null)
            {
                return false;
            }

            return this.Clear(path, ste.Value);
        }

        internal bool Clear(TPath path, IConfigSource source)
        {
            var sourceTable = this.persistable.Load(source);
            var wasRemoved = path.ClearFrom(sourceTable);
            this.persistable.Save(sourceTable, source);
            return wasRemoved;
        }

        internal TomlObject GetFromPath(TPath path)
        {
            path.CheckNotNull(nameof(path));

            var cfg = this.persistable.Load();
            return path.Apply(cfg);
        }

        internal void Set(TPath path, object value)
        {
            path.CheckNotNull(nameof(path));
            value.CheckNotNull(nameof(value));

            var src = this.TryGetSource(path);

            bool notStoredInConfigYet = this.TryGetSource(path) == null;
            if (notStoredInConfigYet)
            {
                this.Set(path, value, this.persistable.RootSource);
            }
            else
            {
                this.Set(tbl => path.SetValue(tbl, ClrToTomlTableConverter.Convert(value, tbl.Root)));
            }
        }

        internal void Set(TPath path, object value, IConfigSource source)
        {
            path.CheckNotNull(nameof(path));
            value.CheckNotNull(nameof(value));

            this.Set(tbl => path.SetValue(tbl, ClrToTomlTableConverter.Convert(value, tbl.Root)), source);
        }

        internal IConfigSource TryGetSource(TPath path)
        {
            path.CheckNotNull(nameof(path));

            var cfgTable = this.persistable.LoadSourcesTable();
            var source = path.TryApply(cfgTable) as TomlSource;
            return source?.Value;
        }

        private static MergeConfigStore CreateMergeStore(IConfigStoreWithSource store)
                    => new MergeConfigStore(new List<IConfigStoreWithSource>() { store });
    }
}
