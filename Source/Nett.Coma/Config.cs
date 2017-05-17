namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Coma.Path;
    using Nett.Extensions;

    public sealed class Config
    {
        private IMergeConfigStore persistable;

        internal Config(IMergeConfigStore persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));
        }

        public static Config<T> Create<T>(Func<T> createDefault, string filePath)
            where T : class
            => Create(createDefault, new MergeConfigStore(new List<IConfigStore>() { new FileConfigStore(filePath) }));

        public static Config<T> Create<T>(Func<T> createDefault, params string[] filePaths)
            where T : class
        {
            var store = new MergeConfigStore(filePaths.Select(fp => new FileConfigStore(fp)));
            return Create(createDefault, store);
        }

        public static Config<T> Create<T>(Func<T> createDefault, IConfigSource source)
            where T : class
        {
            switch (source)
            {
                case IMergeConfigStore merged: return CreateInternal(createDefault, merged);
                case IConfigStore store: return CreateInternal(createDefault, CreateMergeStore(store));
                default: throw new ArgumentException(nameof(source));
            }
        }

        internal static Config<T> CreateInternal<T>(Func<T> createDefault, IMergeConfigStore store)
            where T : class
        {
            createDefault.CheckNotNull(nameof(createDefault));
            store.CheckNotNull(nameof(store));

            var cfg = createDefault();
            store.EnsureExists(Toml.Create(cfg));

            return new Config<T>(store);
        }

        private static MergeConfigStore CreateMergeStore(IConfigStore store)
            => new MergeConfigStore(new List<IConfigStore>() { store });

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
                this.Set(path, value, this.persistable.Sources.First());
            }
            else
            {
                this.Set(tbl => path.SetValue(tbl, TomlObject.CreateFrom(tbl.Root, value, null)));
            }
        }

        internal void Set(TPath path, object value, IConfigSource source)
        {
            path.CheckNotNull(nameof(path));
            value.CheckNotNull(nameof(value));

            this.Set(tbl => path.SetValue(tbl, TomlObject.CreateFrom(tbl.Root, value, null)), source);
        }

        internal IConfigSource TryGetSource(TPath path)
        {
            path.CheckNotNull(nameof(path));

            var cfgTable = this.persistable.LoadSourcesTable();
            var source = path.TryApply(cfgTable) as TomlSource;
            return source?.Value;
        }
    }
}
