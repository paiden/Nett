namespace Nett.Coma
{
    using System;

    public static class ComaConfig
    {
        public static ComaConfig<T> Create<T>(string filePath, Func<T> createDefault)
            where T : class
        {
            var cfg = createDefault();
            var persisted = new FileConfig<T>(filePath);

            persisted.EnsureExists(cfg);

            return new ComaConfig<T>(persisted);
        }
    }

    public sealed class ComaConfig<T>
        where T : class
    {
        private IPersistedConfig<T> persistable;

        internal ComaConfig(IPersistedConfig<T> persistable)
        {
            if (persistable == null) { throw new ArgumentNullException(nameof(persistable)); }

            this.persistable = persistable;
        }

        public TRet Get<TRet>(Func<T, TRet> getter)
        {
            if (getter == null) { throw new ArgumentNullException(nameof(getter)); }

            var cfg = this.persistable.Load();
            return getter(cfg);
        }

        public void Set(Action<T> setter)
        {
            var cfg = this.persistable.Load();
            setter(cfg);
            this.persistable.Save(cfg);
        }
    }
}
