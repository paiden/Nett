namespace Nett.Coma
{
    using System;
    using System.Linq;

    public static class ComaConfig
    {
        public static ComaConfig<T> Create<T>(string filePath, Func<T> createDefault)
            where T : class
        {
            var cfg = createDefault();
            var persisted = new FileConfig(filePath);

            persisted.EnsureExists(Toml.Create(cfg));

            return new ComaConfig<T>(persisted);
        }

        public static ComaConfig<T> CreateMerged<T>(Func<T> createDefault, params string[] filePathes)
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

            var configs = filePathes.Select(fp => new FileConfig(fp));
            configs.First().EnsureExists(Toml.Create(cfg));

            return new ComaConfig<T>(new MergedConfig(configs));
        }
    }

    public sealed class ComaConfig<T>
        where T : class
    {
        private IPersistableConfig persistable;

        internal ComaConfig(IPersistableConfig persistable)
        {
            if (persistable == null) { throw new ArgumentNullException(nameof(persistable)); }

            this.persistable = persistable;
        }

        public TRet Get<TRet>(Func<T, TRet> getter)
        {
            if (getter == null) { throw new ArgumentNullException(nameof(getter)); }

            var cfg = this.persistable.Load().Get<T>();
            return getter(cfg);
        }

        public void Set(Action<T> setter)
        {
            if (setter == null) { throw new ArgumentNullException(nameof(setter)); }

            var cfg = this.persistable.Load().Get<T>();
            setter(cfg);
            this.persistable.Save(Toml.Create(cfg));
        }
    }
}
