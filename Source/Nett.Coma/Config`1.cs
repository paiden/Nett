namespace Nett.Coma
{
    using System;

    public sealed class Config<T> : IDisposable
        where T : class
    {
        private readonly Config config;

        // Used by factory method in Config
        internal Config(IPersistableConfig persistable)
        {
            this.config = new Config(persistable);
        }

        public void Dispose() => this.config.Dispose();

        public TRet Get<TRet>(Func<T, TRet> getter)
        {
            if (getter == null) { throw new ArgumentNullException(nameof(getter)); }

            return this.config.Get(tbl => getter(tbl.Get<T>()));
        }

        public void Set(Action<T> setter)
        {
            if (setter == null) { throw new ArgumentNullException(nameof(setter)); }

            this.config.SetInternal((ref TomlTable tbl) =>
            {
                var typedTable = tbl.Get<T>();
                setter(typedTable);
                tbl = Toml.Create(typedTable);
            });
        }

        public T Unmanaged() => this.config.Unmanaged().Get<T>();
    }
}
