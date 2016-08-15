namespace Nett.Coma
{
    using System;

    public sealed class ComaConfig<T>
        where T : class
    {
        private readonly ComaConfig config;

        internal ComaConfig(IPersistableConfig persistable)
        {
            this.config = new ComaConfig(persistable);
        }

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
    }
}
