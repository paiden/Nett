namespace Nett.Coma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nett.Extensions;

    public class Config
    {
        private IMergeableConfig persistable;

        internal Config(IMergeableConfig persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));

            this.persistable = persistable;
        }

        public static Config<T> Create<T>(Func<T> createDefault, string filePath)
            where T : class
            => Create(createDefault, ConfigSource.CreateFileSource(filePath));

        public static Config<T> Create<T>(Func<T> createDefault, params string[] filePaths)
            where T : class
        {
            var sources = filePaths.Select(fp => ConfigSource.CreateFileSource(fp)).ToArray();
            var merged = ConfigSource.Merged(sources);
            return Create(createDefault, merged);
        }

        public static Config<T> Create<T>(Func<T> createDefault, IConfigSource source)
            where T : class
        {
            createDefault.CheckNotNull(nameof(createDefault));
            source.CheckNotNull(nameof(source));

            var cfg = createDefault();

            var persisted = ((ISourceFactory)source).CreateMergedPersistable();
            persisted.EnsureExists(Toml.Create(cfg));

            return new Config<T>(persisted);
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

        internal IConfigSource GetSource(IList<string> keyChain)
        {
            var table = this.persistable.LoadSourcesTable();
            var source = table.ResolveKeyChain<TomlSource>(keyChain);
            return source.Value;
        }
    }
}
