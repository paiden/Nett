namespace Nett.Coma
{
    using System;
    using System.Linq;
    using Extensions;

    public class Config
    {
        private IPersistableConfig persistable;

        internal Config(IPersistableConfig persistable)
        {
            this.persistable = persistable.CheckNotNull(nameof(persistable));

            this.persistable = persistable;
        }

        internal delegate void SetAction(ref TomlTable table);

        public static Config<T> Create<T>(string filePath, Func<T> createDefault)
            where T : class
        {
            var cfg = createDefault();
            var persisted = new OptimizedFileConfig(new FileConfig(filePath));

            persisted.EnsureExists(Toml.Create(cfg));

            return new Config<T>(persisted);
        }

        public static Config<T> CreateMerged<T>(Func<T> createDefault, params string[] filePathes)
            where T : class
        {
            if (createDefault == null) { throw new ArgumentNullException(nameof(createDefault)); }
            if (filePathes == null) { throw new ArgumentNullException(nameof(createDefault)); }
            if (filePathes.Length < 2)
            {
                throw new ArgumentException($"'{nameof(CreateMerged)}' requires at least 2 config sources. " +
                    $"For single source configurations use '{nameof(Create)}'.");
            }

            var cfg = createDefault();

            var configs = filePathes.Select(fp => new OptimizedFileConfig(new FileConfig(fp)));
            configs.First().EnsureExists(Toml.Create(cfg));

            return new Config<T>(new MergedConfig(configs));
        }

        public TRet Get<TRet>(Func<TomlTable, TRet> getter)
        {
            getter.CheckNotNull(nameof(getter));

            var cfg = this.persistable.Load();
            return getter(cfg);
        }

        public void Set(Action<TomlTable> setter)
        {
            setter.CheckNotNull(nameof(setter));

            this.SetInternal((ref TomlTable tbl) => setter(tbl));
        }

        public TomlTable Unmanaged() => this.persistable.Load();

        internal void SetInternal(SetAction setter)
        {
            var cfg = this.persistable.Load();
            setter(ref cfg);
            this.persistable.Save(cfg);
        }
    }
}
